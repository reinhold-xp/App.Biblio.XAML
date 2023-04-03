using System;
using System.Collections.Generic;
using System.Net;
using Biblio.Models;
using Newtonsoft.Json;
using Xamarin.Forms;
using static Xamarin.Forms.Internals.GIFBitmap;
using Xamarin.Essentials;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using System.ComponentModel;
using System.Web;
using System.IO;
using File = System.IO.File;

namespace Biblio.Views
{
    public partial class BooksPage : ContentPage
    {
        const String URL_API = "http://api.reinhold-info.com/books";
        const String URL_WWW = "";

        List<Book> books;
        List<String> favBooks = new List<string>();

        private enum e_Sorting
        {
            TRI_NONE,
            TRI_TITLE,
            TRI_PAGES,
            TRI_FAV
        }

        // Persistance du dernier réglage (user settings)
        const string KEY_SORT = "sort";
        const string KEY_FAV = "fav";

        // Générer un nom de fichier
        String jsonFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "books.json");
        String tempFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp");

        // Tri par défaut
        e_Sorting tri = e_Sorting.TRI_NONE;

        public BooksPage()
        {
            InitializeComponent();

            // Pull to refresh
            booksListView.RefreshCommand = new Command((obj) =>
            {
                DownloadData((books) =>
                {
                    if (books != null)
                        booksListView.ItemsSource = GetBooksCells(GetBooksFromSorting(tri, books), favBooks);

                    // Animation Wait
                    booksListView.IsRefreshing = false;
                });
            });


            booksListView.IsVisible = false;
            waitLayout.IsVisible = true;
            LoadFavList();

            // Hors ligne ?
            if (File.Exists(jsonFileName))
            {
                String booksJson = File.ReadAllText(jsonFileName);

                if (!String.IsNullOrEmpty(booksJson))
                {
                    books = DecodeHtml(JsonConvert.DeserializeObject<List<Book>>(booksJson));
                    booksListView.ItemsSource = GetBooksCells(GetBooksFromSorting(tri, books), favBooks);
                    booksListView.IsVisible = true;
                    waitLayout.IsVisible = false;
                }
            }


            // Persistance du dernier réglage, user settings ?
            if (Application.Current.Properties.ContainsKey(KEY_SORT))
                tri = (e_Sorting)Application.Current.Properties[KEY_SORT];


            // Téléchargement des données,
            // au chargement l'application
            DownloadData((books) =>
            {
                if (books != null)
                    booksListView.ItemsSource = GetBooksCells(GetBooksFromSorting(tri, books), favBooks);

                booksListView.IsVisible = true;
                waitLayout.IsVisible = false;
            });

            // Gestion du click sur les cellules :
            // abonnement event (fonction anonyme)
            booksListView.ItemSelected += (sender, e) =>
            {
                if (booksListView.SelectedItem != null)
                {
                    BookCell item = booksListView.SelectedItem as BookCell;
                    DisplayAlert(item.book.titre, item.book.resume, "Fermer");

                    // Reset
                    booksListView.SelectedItem = null;
                }
            };
        }

        // Paramètre delegate type action :
        // principe de simple responsabilité
        private void DownloadData(Action<List<Book>> action)
        {

            // Using : bonne pratique gestion mémoire avec
            // des objets Stream (IDisposalbe)
            using (var webClient = new WebClient())
            {
                try
                {
                    webClient.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                    {
                        Exception ex = e.Error;

                        if (ex == null)
                        {
                            // Back-up du fichier téléchargé
                            File.Copy(tempFileName, jsonFileName, true);

                            string booksJson = File.ReadAllText(jsonFileName);
                            books = DecodeHtml(JsonConvert.DeserializeObject<List<Book>>(booksJson));

                            // ListView étant un composant graphique,
                            // on appelle la delegate dans le contexte de la Main thread (UI)
                            // depuis un contexte de thread Réseau (DownloadStringAsync)
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                action.Invoke(books);
                            });
                        }

                        else
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                // On attend la validation de
                                // l'utilisateur pour continuer
                                await DisplayAlert("Erreur", "Une erreur réseau s'est produite: " + ex.Message, "OK");
                                action.Invoke(null);
                            });
                        }
                    };

                    // Appel asynchrone (téléchargement des données)
                    webClient.DownloadFileAsync(new Uri(URL_API), tempFileName);

                }
                catch (Exception ex)
                {
                    // On rend la main au main thread (UI)
                    // depuis un contexte de thread de réseau
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        DisplayAlert("Erreur", "Une erreur réseau s'est produite : " + ex.Message, "Ok");
                        action.Invoke(null);
                    });
                }
            }
        }

        private List<Book> DecodeHtml(List<Book> rootBooks)
        {
            if (rootBooks == null)
                return null;

            foreach (Book book in rootBooks)
            {
                book.titre = HttpUtility.HtmlDecode(book.titre);
                book.resume = HttpUtility.HtmlDecode(book.resume);
                book.auteur = HttpUtility.HtmlDecode(book.auteur);
            }
            return rootBooks;
        }

        // Persistance des favoris
        private void OnFavChanged(BookCell bookCell)
        {

            // Ajouter ou supprimer des favs ?
            bool isInFavList = favBooks.Contains(bookCell.book.titre);

            if (bookCell.isFavorite && !isInFavList)
            {
                favBooks.Add(bookCell.book.titre);
                SaveFavList();
            }
            else if (!bookCell.isFavorite && isInFavList)
            {
                favBooks.Remove(bookCell.book.titre);
            }

        }

        private List<BookCell> GetBooksCells(List<Book> rootBooks, List<String> favBooks)
        {
            List<BookCell> cellBooks = new List<BookCell>();

            if (rootBooks == null)
            {
                return null;
            }

            foreach (Book book in rootBooks)
            {
                bool isFav = favBooks.Contains(book.titre);

                if (tri == e_Sorting.TRI_FAV)
                {
                    if (isFav)
                    {
                        cellBooks.Add(new BookCell { book = book, isFavorite = isFav, favChangedAction = OnFavChanged });
                    }
                }
                else
                {
                    cellBooks.Add(new BookCell { book = book, isFavorite = isFav, favChangedAction = OnFavChanged });

                }

            }

            return cellBooks;
        }

        private List<Book> GetBooksFromSorting(e_Sorting sortSetting, List<Book> rootBooks)
        {
            if (rootBooks == null)
                return null;

            switch (sortSetting)
            {
                case e_Sorting.TRI_TITLE:
                case e_Sorting.TRI_FAV:
                    {
                        // Copie liste source
                        List<Book> sortedList = new List<Book>(rootBooks);

                        // Tri croissant
                        sortedList.Sort((book1, book2) => { return book1.titre.CompareTo(book2.titre); });
                        return sortedList;
                    }
                case e_Sorting.TRI_PAGES:
                    {
                        // Copie liste source
                        List<Book> sortedList = new List<Book>(rootBooks);

                        // Tri decroissant
                        sortedList.Sort((book1, book2) => { return book2.nbPages.CompareTo(book1.nbPages); });
                        return sortedList;
                    }
            }

            // Tri = AUCUN
            return rootBooks;
        }

        // Persistance des favoris
        // en local / user settings
        private void SaveFavList()
        {
            string json = JsonConvert.SerializeObject(favBooks);
            Application.Current.Properties[KEY_FAV] = json;
            Application.Current.SavePropertiesAsync();
        }

        private void LoadFavList()
        {
            if (Application.Current.Properties.ContainsKey(KEY_FAV))
            {
                string json = Application.Current.Properties[KEY_FAV].ToString();
                favBooks = JsonConvert.DeserializeObject<List<string>>(json);
            }
        }

        private void SortButtonClicked(System.Object sender, System.EventArgs e)
        {

            if (tri == e_Sorting.TRI_NONE)
            {
                tri = e_Sorting.TRI_TITLE;
            }
            else if (tri == e_Sorting.TRI_TITLE)
            {
                tri = e_Sorting.TRI_PAGES;
            }
            else if (tri == e_Sorting.TRI_PAGES)
            {
                tri = e_Sorting.TRI_FAV;
            }
            else if (tri == e_Sorting.TRI_FAV)
            {
                tri = e_Sorting.TRI_NONE;
            }

            booksListView.ItemsSource = GetBooksCells(GetBooksFromSorting(tri, books), favBooks);

            // Persistance du dernier réglage (Key <-> String)
            Application.Current.Properties[KEY_SORT] = (int)tri;
            Application.Current.SavePropertiesAsync();
        }


        private void ModifyButtonClicked(System.Object sender, System.EventArgs e)
        {

            // On ouvre une url dans l'application (WebView)
            Navigation.PushAsync(new WebViewPage(URL_WWW));

            // ou en dehors, dans un navigateur 
            // var task = ModifyButtonClickedAsync();
        }

        private async Task ModifyButtonClickedAsync()
        {
            await Launcher.OpenAsync(URL_WWW);
        }

    }

}


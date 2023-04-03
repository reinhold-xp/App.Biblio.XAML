using System;
using System.ComponentModel;
using System.Web;
using System.Windows.Input;
using Xamarin.Forms;

namespace Biblio.Models
{

    // Implementation interface INotifyPropertyChanged
    public class BookCell : INotifyPropertyChanged 
	{
		public Book book { get; set; }
		public bool isFavorite { get; set; }
		public string imageSourceFav { get { return isFavorite ? "pin1.png" : "pin2.png"; } }
        public ICommand favClickCommand { get; set; }
        public Action<BookCell> favChangedAction { get; set; }

        public BookCell()
        {

            // Binding Command / image button 
            favClickCommand = new Command((obj) =>
            {

                // Binding CommandParameter ?
                Book param = obj as Book;

                Console.WriteLine("FavClickCommand ");
                Console.WriteLine("FavClickCommand: " + param.titre);

                // On alterne les images pin1 et pin2,
                // à chaque click sur l'image buttton,
                // quand la propriété isFavorite change ...
                isFavorite = !isFavorite;
                OnPropertyChanged("imageSourceFav");

                // Appel action (delegate) pour 
                // persister la liste des favoris
                favChangedAction.Invoke(this);

            });
        }

        // Implementation INotifyPropertyChanged 
        public event PropertyChangedEventHandler PropertyChanged;


        // On signale les changements d'état (param name = nom de la propriété qui a changé)
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}


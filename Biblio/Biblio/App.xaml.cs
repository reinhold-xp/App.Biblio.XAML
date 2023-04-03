using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Biblio.Views;

namespace Biblio
{
    public partial class App : Application
    {
        public App ()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new BooksPage());

        }

        protected override void OnStart ()
        {
        }

        protected override void OnSleep ()
        {
        }

        protected override void OnResume ()
        {
        }
    }
}


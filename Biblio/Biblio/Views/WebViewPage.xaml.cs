using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Biblio.Views
{
    public partial class WebViewPage : ContentPage
    {
        public WebViewPage()
        {
            InitializeComponent();
        }

        public WebViewPage(string url)
        {
            InitializeComponent();
            webView.Source = url;
        }
    }
}


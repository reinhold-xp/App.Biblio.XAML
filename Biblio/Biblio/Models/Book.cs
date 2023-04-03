using System;
using System.Web;

namespace Biblio.Models
{
    public class Book
    {
        public int id { get; set; }
        public string titre { get; set; }
        public string auteur { get; set; }
        public int nbPages { get; set; }
        public string image { get; set; }
        public string resume { get; set; }
        public string nbPagesToString { get { return nbPages + " pages"; } }

        public Book()
        {
        }
    }
}


using System;
using System.Web;

namespace ImagesHosting.Models
{
    public class Image_base 
    {
        public int Id { get; set; }
        public string url { get; set; }
        public string user_description { get; set; }
        public string load_date { get; set; }
        public string change_date { get; set; }
        public string imgtype { get; set; }
     
    }
}

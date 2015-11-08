namespace ImagesHosting.Models
{
    public class ImageBase 
    {
        public int Id { get; set; }
        public string url { get; set; }
        public string user_description { get; set; }
        public string load_date { get; set; }
        public string change_date { get; set; }
        public string imgtype { get; set; }
     
    }

}

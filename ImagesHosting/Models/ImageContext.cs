using System.Data.Entity;

namespace ImagesHosting.Models
{
    public class ImageContext : DbContext
    {
        public ImageContext() : base("DBConnection") { }
        public DbSet<ImageBase> Images { get; set; }
    
    }
}

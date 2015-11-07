using System;
using System.Web;
using System.Collections.Generic;
using System.Data.Entity;

namespace ImagesHosting.Models
{
    public class Image_context : DbContext
    {
        public Image_context() : base("DBConnection") { }
        public DbSet<Image_base> Images { get; set; }
    
    }
}

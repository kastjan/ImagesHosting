namespace ImagesHosting.Migrations
{
    using System;
    using System.IO;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Web;
    using System.Web.Hosting;
    using System.Collections.Generic;
    using ImagesHosting.Models;
    using System.Configuration;

    internal sealed class Configuration : DbMigrationsConfiguration<ImagesHosting.Models.ImageContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "ImagesHosting.Models.ImageContext";
        }

        protected override void Seed(ImagesHosting.Models.ImageContext context)
        {
            var images = new List<ImageBase> { };
            string folder_with_images = ConfigurationManager.AppSettings["FolderWithImages"];         
            string destination_path = ConfigurationManager.AppSettings["StoredImages"];               
            string[] filePaths = Directory.GetFiles(folder_with_images);
            foreach (string filePath in filePaths)
            {
                string fileName = Path.GetFileName(filePath);
                ImageBase image = new ImageBase
                {
                    url = Path.Combine(destination_path, fileName),
                    user_description = null,
                    load_date = DateTime.Now.ToString(),
                    change_date = DateTime.Now.ToString(),
                    imgtype = MimeMapping.GetMimeMapping(fileName)
                };
                File.Copy(Path.Combine(folder_with_images, fileName), Path.Combine(destination_path, fileName), true);
                images.Add(image);
            }
            images.ForEach(i => context.Images.AddOrUpdate(u => u.url, i));
            context.SaveChanges();
           
        }
    }
}

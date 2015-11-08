﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace ImagesHosting.Models
{
    public class AccessToDatabase
    {
        private string ServerPath = new DirectoryInfo(HostingEnvironment.ApplicationPhysicalPath).Parent.FullName;
        public string AddImage(HttpPostedFileBase file)
        {
            try
            {
                ImageContext db = new ImageContext();
                MemoryStream ms = new MemoryStream();
                file.InputStream.CopyTo(ms);
                ImageBase image = new ImageBase
                {
                    url = ServerPath + "\\images\\" + file.FileName,
                    user_description = null,
                    load_date = DateTime.Now.ToString(),
                    change_date = DateTime.Now.ToString(),
                    imgtype = file.ContentType
                };
                System.IO.Directory.CreateDirectory(ServerPath + "\\images\\");
                string path = ServerPath + "\\images\\" + file.FileName;
                FileStream newfile = new FileStream(path, FileMode.Create, FileAccess.Write);
                ms.WriteTo(newfile);
                newfile.Close();
                db.Images.Add(image);
                db.SaveChanges();
                return null;
            }
            catch(Exception ex)
            {
                return ex.ToString();
            }
        }
        public string RemoveImage(int id)
        {
            try
            {
                ImageContext db = new ImageContext();
                ImageBase image = db.Images.SingleOrDefault(f => f.Id == id);
                string url = image.url;
                var img = new ImageBase { Id = id };
                if (System.IO.File.Exists(url))
                    System.IO.File.Delete(url);
                db.Images.Attach(image);
                db.Images.Remove(image);
                db.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public ImageBase GetImageData(int id)
        {
            try
            {
                ImageContext db = new ImageContext();
                ImageBase img = db.Images.SingleOrDefault(f => f.Id == id);
                if (img == null)
                    throw new Exception();
                return img;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public string ChangeImgData(ImageBase img)
        {
            try
            {
                ImageContext db = new ImageContext();
                var original = db.Images.Find(img.Id);
                if (original != null)
                {
                    original.change_date = img.change_date;
                    original.user_description = img.user_description;
                    db.SaveChanges();
                }
                return null;
            }
            catch(Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
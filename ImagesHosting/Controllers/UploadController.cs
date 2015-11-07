using ImagesHosting.Models;
using ExifLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;


namespace ImagesHosting.Controllers
{
    public class UploadController : Controller
    {
        //
        // GET: /Upload/
        string ServerPath = new DirectoryInfo(HostingEnvironment.ApplicationPhysicalPath).Parent.FullName;
        public ActionResult UploadImage(HttpPostedFileBase[] files)
        {
            if (files[0]!=null)
            {
                foreach (var file in files)
                {
                    using (Image_context db = new Image_context())
                    {
                        MemoryStream ms = new MemoryStream();
                        file.InputStream.CopyTo(ms);
                        Image_base image = new Image_base 
                        { 
                            url = ServerPath + "\\images\\" + file.FileName, 
                            user_description = null,
                            load_date = DateTime.Now.ToString(),
                            change_date = DateTime.Now.ToString(),
                            imgtype = file.ContentType
                        };
                        db.Images.Add(image);
                        db.SaveChanges();
                        System.IO.Directory.CreateDirectory(ServerPath + "\\images\\");
                        string path =ServerPath + "\\images\\" + file.FileName;
                        FileStream newfile = new FileStream(path, FileMode.Create, FileAccess.Write);
                        ms.WriteTo(newfile);
                        newfile.Close();
                        Response.Write(true);
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult RemoveImage(string Id)
        {
            using (Image_context db = new Image_context())
            {
                int id = Int32.Parse(Id);
                Image_base image = db.Images.SingleOrDefault(f => f.Id == id);
                string url = image.url;
                var img = new Image_base { Id = id };
                if (System.IO.File.Exists(url))
                    System.IO.File.Delete(url);
                db.Images.Attach(image);
                db.Images.Remove(image);
                db.SaveChanges();
            }
            return RedirectToAction("Index", "Home");
        }

        public FileContentResult viewimage(int id)
        {
            using (Image_context db = new Image_context())
            {
                Image_base img = db.Images.SingleOrDefault(f => f.Id == id);
                FileStream file = new FileStream(img.url, FileMode.Open, FileAccess.Read);
                MemoryStream ms = new MemoryStream();
                file.CopyTo(ms);
                file.Close();
                return File(ms.GetBuffer(), img.imgtype);
            }
        }
        public ActionResult GetImageGPS(string Id)
        {
            using (Image_context db = new Image_context())
            {
                int id = Int32.Parse(Id);
                Image_base img = db.Images.SingleOrDefault(f => f.Id == id);
                StringBuilder sb = new StringBuilder();
                try
                {

                    using (var reader = new ExifReader(img.url))
                    {
                        object val;
                        reader.GetTagValue(ExifTags.GPSLatitudeRef, out val);
                        string gps = RenderTag(val);
                        reader.GetTagValue(ExifTags.GPSLatitude, out val);
                        gps = gps + " " + RenderTag(val);
                        reader.GetTagValue(ExifTags.GPSLongitudeRef, out val);
                        gps = gps + " " + RenderTag(val);
                        reader.GetTagValue(ExifTags.GPSLongitude, out val);
                        gps = gps + " " + RenderTag(val);
                        Response.Write(gps);
                    }
                }
                catch (Exception ex)
                {
                    var error = ex.Message.ToString();
                    Response.Write(null);
                }
            }
            return null;
        }

        public ActionResult GetImageInfo(string Id)
        {
            using (Image_context db = new Image_context())
            {
                int id = Int32.Parse(Id);
                Image_base img = db.Images.SingleOrDefault(f => f.Id == id);
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("<h2>IMAGE INFO FROM DATABASE</h2>");
                sb.AppendFormat("<p>Load Date: {0}</p>", img.load_date);
                sb.AppendFormat("<p>Change Date: {0}</p>", img.change_date);
                try
                {
                    using (var reader = new ExifReader(img.url))
                    {
                        // Parse through all available fields and generate key-value labels
                        var props = Enum.GetValues(typeof(ExifTags)).Cast<ushort>().Select(tagID =>
                        {
                            object val;
                            if (reader.GetTagValue(tagID, out val))
                            {
                                // Special case - some doubles are encoded as TIFF rationals. These
                                // items can be retrieved as 2 element arrays of {numerator, denominator}
                                if (val is double)
                                {
                                    int[] rational;
                                    if (reader.GetTagValue(tagID, out rational))
                                        val = string.Format("{0} ({1}/{2})", val, rational[0], rational[1]);
                                }

                                return string.Format("<p>{0}: {1}</p>", Enum.GetName(typeof(ExifTags), tagID), RenderTag(val));
                            }

                            return null;

                        }).Where(x => x != null).ToArray();
                         var exifdata = string.Join("\r\n", props);
                         sb.AppendFormat("<h2>EXIF FROM IMAGE</h2>");
                         sb.AppendFormat("<p>{0}</p>", exifdata);
                    }
                }
                catch (Exception ex)
                {
                    // Something didn't work!
                    sb.AppendFormat("<h2>EXIF FROM IMAGE</h2>");
                    sb.AppendFormat("<p>{0}</p>", ex.Message.ToString());
                }
                Response.Write(sb.ToString());
                return null;
	            
            }
        }
        private static string RenderTag(object tagValue)
        {
            // Arrays don't render well without assistance.
            var array = tagValue as Array;
            if (array != null)
            {
                // Hex rendering for really big byte arrays (ugly otherwise)
                if (array.Length > 20 && array.GetType().GetElementType() == typeof(byte))
                    return "0x" + string.Join("", array.Cast<byte>().Select(x => x.ToString("X2")).ToArray());

                return string.Join(" ", array.Cast<object>().Select(x => x.ToString()).ToArray());
            }

            return tagValue.ToString();
        }

        [HttpPost]
        public ActionResult GetSetComments(user_request request)
        {
            using (Image_context db = new Image_context())
            {
                int id = Int32.Parse(request.Id);
                Image_base img = db.Images.SingleOrDefault(f => f.Id == id);
                StringBuilder sb = new StringBuilder();
                if (request.Text == null)
                {
                    if (img.user_description != null)
                    {
                        sb.AppendFormat("<li class=\"editable\" data-value=\"{0}\"> {1} </li>", id, img.user_description);
                        Response.Write(sb.ToString());
                    }
                    else
                    {
                        sb.AppendFormat("<li class=\"editable\" data-value=\"{0}\"> No Comments </li>", id);
                        Response.Write(sb.ToString());
                    }
                }
                else
                {
                    img.user_description = request.Text;
                    img.change_date = DateTime.Now.ToString();
                    var original = db.Images.Find(img.Id);
                    if (original != null)
                    {
                        original.change_date = img.change_date;
                        original.user_description = img.user_description;
                        db.SaveChanges();
                    }    
                }
                return null;
            }
        }
    }


}

using ImagesHosting.Models;
using ExifLib;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Collections.Generic;


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
                    using (ImageContext db = new ImageContext())
                    {
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
            using (ImageContext db = new ImageContext())
            {
                int id = Int32.Parse(Id);
                ImageBase image = db.Images.SingleOrDefault(f => f.Id == id);
                string url = image.url;
                var img = new ImageBase { Id = id };
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
            using (ImageContext db = new ImageContext())
            {
                ImageBase img = db.Images.SingleOrDefault(f => f.Id == id);
                FileStream file = new FileStream(img.url, FileMode.Open, FileAccess.Read);
                MemoryStream ms = new MemoryStream();
                file.CopyTo(ms);
                file.Close();
                return File(ms.GetBuffer(), img.imgtype);
            }
        }
        public JsonResult GetImageGPS(string Id)
        {
            var gps = new List<JSONDataFormat>();
            using (ImageContext db = new ImageContext())
            {
                int id = Int32.Parse(Id);
                ImageBase img = db.Images.SingleOrDefault(f => f.Id == id);
                //StringBuilder sb = new StringBuilder();
                try
                {

                    using (var reader = new ExifReader(img.url))
                    {
                        object val;
                        reader.GetTagValue(ExifTags.GPSLatitudeRef, out val);
                        gps.Add(new JSONDataFormat {parameter = "GPSLatitudeRef", data = RenderTag(val) });
                        reader.GetTagValue(ExifTags.GPSLatitude, out val);
                        gps.Add(new JSONDataFormat { parameter = "GPSLatitude", data = RenderTag(val) });
                        reader.GetTagValue(ExifTags.GPSLongitudeRef, out val);
                        gps.Add(new JSONDataFormat { parameter = "GPSLongitudeRef", data = RenderTag(val) });
                        reader.GetTagValue(ExifTags.GPSLongitude, out val);
                        gps.Add(new JSONDataFormat { parameter = "GPSLongitude", data = RenderTag(val) });
                        //Response.Write(gps);
                    }
                }
                catch (Exception ex)
                {
                    //Response.Write(null);
                    gps.Add(new JSONDataFormat { parameter = null, data = ex.Message.ToString()});
                }
            }
            return Json(gps, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetImageInfo(string Id)
        {
            using (ImageContext db = new ImageContext())
            {
                List<JSONDataFormat> data = new List<JSONDataFormat>();
                int id = Int32.Parse(Id);
                ImageBase img = db.Images.SingleOrDefault(f => f.Id == id);
                data.Add(new JSONDataFormat { parameter = "IMAGE INFO FROM DATABASE" , data = null});
                data.Add(new JSONDataFormat { parameter = "Load Date: ", data = img.load_date });
                data.Add(new JSONDataFormat { parameter = "Change Date: ", data = img.change_date });

                //StringBuilder sb = new StringBuilder();
                /*sb.AppendFormat("<h2>IMAGE INFO FROM DATABASE</h2>");
                sb.AppendFormat("<p>Load Date: {0}</p>", img.load_date);
                sb.AppendFormat("<p>Change Date: {0}</p>", img.change_date);*/
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
                                var inf = new JSONDataFormat();
                                inf.parameter = Enum.GetName(typeof(ExifTags), tagID);
                                inf.data = RenderTag(val);
                                //return string.Format("<p>{0}: {1}</p>", Enum.GetName(typeof(ExifTags), tagID), RenderTag(val));
                                return inf;
                            }

                            return null;

                        }).Where(x => x != null).ToList();
                         //var exifdata = string.Join("\r\n", props);
                        data.Add(new JSONDataFormat { parameter = "EXIF FROM IMAGE", data = null });
                        data.AddRange(props);
                       /* sb.AppendFormat("<h2>EXIF FROM IMAGE</h2>");
                        sb.AppendFormat("<p>{0}</p>", exifdata);*/
                    }
                }
                catch (Exception ex)
                {
                    // Something didn't work!
                    /*sb.AppendFormat("<h2>EXIF FROM IMAGE</h2>");
                    sb.AppendFormat("<p>{0}</p>", ex.Message.ToString());*/
                    data.Add(new JSONDataFormat { parameter = "EXIF FROM IMAGE", data = ex.Message.ToString() });
                }
                //Response.Write(sb.ToString());
                return Json(data, JsonRequestBehavior.AllowGet);
	            
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
        public JsonResult GetSetComments(UserRequest request)
        {
            using (ImageContext db = new ImageContext())
            {
                int id = Int32.Parse(request.Id);
                ImageBase img = db.Images.SingleOrDefault(f => f.Id == id);
                //StringBuilder sb = new StringBuilder();
                var userdescr = new List<JSONDataFormat>();
                if (request.Text == null)
                {
                    if (img.user_description != null)
                    {
                        /*sb.AppendFormat("<li class=\"editable\" data-value=\"{0}\"> {1} </li>", id, img.user_description);
                        Response.Write(sb.ToString());*/
                        userdescr.Add(new JSONDataFormat { parameter = id.ToString(), data = img.user_description});

                    }
                    else
                    {
                        //sb.AppendFormat("<li class=\"editable\" data-value=\"{0}\"> No Comments </li>", id);
                        //Response.Write(sb.ToString());
                        userdescr.Add(new JSONDataFormat { parameter = id.ToString(), data = "No Comments"});
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
                return Json(userdescr, JsonRequestBehavior.AllowGet);
            }
        }
    }


}

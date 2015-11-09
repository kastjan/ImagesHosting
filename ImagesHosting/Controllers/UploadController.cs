using ImagesHosting.Models;
using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Collections.Generic;


namespace ImagesHosting.Controllers
{
    //Send images to database
    public class UploadController : Controller
    {
        string ServerPath = new DirectoryInfo(HostingEnvironment.ApplicationPhysicalPath).Parent.FullName;
        public ActionResult UploadImage(HttpPostedFileBase[] files)
        {
            try
            {
                if (files[0] != null)
                {
                    foreach (var file in files)
                    {
                        var database = new AccessToDatabase();
                        if (database.AddImage(file) != null)
                            return View("Error");
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            catch(Exception)
            {
                return View("Error");
            }
        }

        //Remove images from database
        public ActionResult RemoveImage(string Id)
        {
            try
            {
                int id = Int32.Parse(Id);
                var database = new AccessToDatabase();
                if (database.RemoveImage(id) != null)
                    return View("error");
                return RedirectToAction("Index", "Home");
            }
            catch(Exception)
            {
                return View("error");
            }
        }

        //Get image from dadabase by ID
        public ActionResult viewimage(string Id)
        {
            try
            {
                int id = Int32.Parse(Id);
                var database = new AccessToDatabase();
                var img = database.GetImageData(id);
                if (img == null)
                    throw new Exception();
                FileStream file = new FileStream(img.url, FileMode.Open, FileAccess.Read);
                MemoryStream ms = new MemoryStream();
                file.CopyTo(ms);
                file.Close();
                return File(ms.GetBuffer(), img.imgtype);
            }
            catch (Exception)
            {
                return View("error");
            }
        }
        //Get image GPS coordinates from database by ID 
        public JsonResult GetImageGPS(string Id)
        {
            try
            {
                int id = Int32.Parse(Id);
                var database = new AccessToDatabase();
                var img = database.GetImageData(id);
                var exifdata = new GetExifData();
                return Json(exifdata.GetGPS(img), JsonRequestBehavior.AllowGet);
            }
            catch(Exception)
            {
                return null;
            }
        }

        // Get Full image EXIF information from database by ID 
        public JsonResult GetImageInfo(string Id)
        {
            try
            {
                List<JSONDataFormat> data = new List<JSONDataFormat>();
                int id = Int32.Parse(Id);
                var database = new AccessToDatabase();
                var img = database.GetImageData(id);
                data.Add(new JSONDataFormat { parameter = "IMAGE INFO FROM DATABASE", data = null });
                data.Add(new JSONDataFormat { parameter = "Load Date: ", data = img.load_date });
                data.Add(new JSONDataFormat { parameter = "Change Date: ", data = img.change_date });
                var exifdata = new GetExifData();
                data.AddRange(exifdata.GetFullEXIF(img));
                return Json(data, JsonRequestBehavior.AllowGet);
            }
	        catch (Exception)
            {
                return null;
            }   
        }

        //Get and Set user comments in database by image ID
        [HttpPost]
        public JsonResult GetSetComments(UserRequest request)
        {
            try
            {
                int id = Int32.Parse(request.Id);
                var database = new AccessToDatabase();
                var img = database.GetImageData(id);
                if (img == null)
                    throw new Exception();
                var userdescr = new List<JSONDataFormat>();
                if (request.Text == null)
                {
                    if (img.user_description != null)
                        userdescr.Add(new JSONDataFormat { parameter = id.ToString(), data = img.user_description });

                    else
                        userdescr.Add(new JSONDataFormat { parameter = id.ToString(), data = "No Comments" });
                }
                else
                {
                    img.user_description = request.Text;
                    img.change_date = DateTime.Now.ToString();
                    if (database.ChangeImgData(img) != null)
                        throw new Exception();
                }
                return Json(userdescr, JsonRequestBehavior.AllowGet);
            }
            catch(Exception)
            {
                return null;
            }
           
        }
    }


}

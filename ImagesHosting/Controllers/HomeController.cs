using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ImagesHosting.Models;

namespace ImagesHosting.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            
            using (Image_context db = new Image_context())
            {
                IEnumerable<int> guids = db.Images.Select(img => img.Id).ToList();
                return View(guids);
            }
    
        }
        public ActionResult About()
        {
            return View();
        }
    }
}

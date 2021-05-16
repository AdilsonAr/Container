using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Container.FileRepository;
using System.Threading.Tasks;

namespace Container.Controllers

{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult IndexParcial()
        {
            return View();
        }
        
        public ActionResult LoginParcial()
        {
            return View();
        }
   
        public async Task<JsonResult> Test()
        {
            FileRepo repo = new FileRepo();
            string s = await repo.testListBucketsAsync();
            return Json(s,JsonRequestBehavior.AllowGet);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Container.FileRepository;
using System.Threading.Tasks;
using Container.Models;
using Container.Dto;

namespace Container.Controllers
{
    public class FileController : Controller
    {
        ContainerEntities db = new ContainerEntities();
        public async Task<JsonResult> Test()
        {
            FileRepo repo = new FileRepo();
            string s = await repo.testListBucketsAsync();
            return Json(s, JsonRequestBehavior.AllowGet);
        }

        // GET: File
        public ActionResult Files()
        {
            string nombreUsuario = System.Web.HttpContext.Current.Session["usernameS"].ToString();
            if (nombreUsuario == null)
            {
                Response.StatusCode = 401;
            }
            return View();
        }

        public ActionResult FileList(int id_repo)
        {
            int idUsuario = (int)System.Web.HttpContext.Current.Session["userIdS"];
            string nombreUsuario = System.Web.HttpContext.Current.Session["usernameS"].ToString();
            if (nombreUsuario == null)
            {
                Response.StatusCode = 401;
            }
            else
            {
                repositorio repo = db.repositorio.Where(x => x.id_repositorio == id_repo).First();
                if (repo == null)
                {
                    Response.StatusCode = 400;
                }
                else
                {
                    suscripcion sus = db.suscripcion.Where(x => x.id_repositorio == id_repo
                      && x.id_usuario == idUsuario).First();
                    if (sus == null)
                    {
                        Response.StatusCode = 401;
                    }
                    else
                    {
                        List<Archivo_s3_ResponseDto> archDto = new List<Archivo_s3_ResponseDto>();
                        List <referencia> refer = repo.referencia.ToList();
                        foreach(referencia c in refer)
                        {
                            Archivo_s3_ResponseDto arch = new Archivo_s3_ResponseDto();
                            arch.nombre = c.archivo_s3.nombre_archivo_app;
                            arch.id = c.archivo_s3.id_archivo;
                            archDto.Add(arch);
                        }
                        ViewBag.files = archDto;
                    }
                }
            }
            return View();
        }

        public ActionResult RepoList()
        {
            int idUsuario = (int)System.Web.HttpContext.Current.Session["userIdS"];
            string nombreUsuario = System.Web.HttpContext.Current.Session["usernameS"].ToString();
            if (nombreUsuario == null)
            {
                Response.StatusCode = 401;
            }
            else
            {
                List<RepoResponseDto> repos = new List<RepoResponseDto>();
                List<suscripcion> sus = db.suscripcion.Where(x => x.id_usuario == idUsuario).ToList();
                foreach(suscripcion c in sus)
                {
                    RepoResponseDto r = new RepoResponseDto();
                    r.id = c.repositorio.id_repositorio;
                    r.nombre = c.repositorio.nombre;
                    repos.Add(r);
                }
                ViewBag.repos = repos;
            }
            return View();
        }

        // GET: File/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: File/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: File/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: File/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: File/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: File/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: File/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}

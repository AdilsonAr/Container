using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Container.FileRepository;
using System.Threading.Tasks;
using Container.Models;
using Container.Dto;
using System.IO;

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

        [HttpPost]
        public async Task<JsonResult> UploadFile(HttpPostedFileBase file)
        {
            FileRepo repo = new FileRepo();
            bool r= await repo.upload(file, Path.Combine(Server.MapPath("~/Uploads"), Path.GetFileName(file.FileName)));
            return Json(r);
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
                        ViewBag.repo = id_repo;
                        ViewBag.repoName = repo.nombre;
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

    }
}

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
        FileRepo repo = new FileRepo();
        string bucket = "ar-container-bucket";

        public async Task<JsonResult> Test()
        {            
            string s = await repo.testListBucketsAsync();
            return Json(s, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> UploadFile(HttpPostedFileBase file, int repo_id)
        {
            if (System.Web.HttpContext.Current.Session["usernameS"] == null) { 
                Response.StatusCode = 401;
                return Json(false);
            }
            else
            {
                int usuario_id = (int)System.Web.HttpContext.Current.Session["userIdS"];
                string nombre = Guid.NewGuid().ToString();
                bool r = await repo.upload(file, 
                    Path.Combine(Server.MapPath("~/Uploads"), Path.GetFileName(file.FileName)), 
                    bucket,
                    nombre);
                if (r)
                {
                    archivo_s3 ar = new archivo_s3();
                    ar.nombre_archivo_app = file.FileName;
                    ar.nombre_archivo_s3 = nombre;
                    ar.id_autor = usuario_id;
                    ar.nombre_bucket = bucket;
                    ar.fecha = new DateTime();
                    ar.peso_Mb = file.ContentLength / (1024*1024);
                    ar.tipo = file.ContentType;
                    ar.clave_archivo = Guid.NewGuid().ToString();

                    ContainerEntities db = new ContainerEntities();
                    db.archivo_s3.Add(ar);
                    db.SaveChanges();

                    ContainerEntities db2 = new ContainerEntities();
                    archivo_s3 ar2 = db2.archivo_s3.Where(x => x.clave_archivo.Equals(ar.clave_archivo)
                      && x.nombre_archivo_s3.Equals(ar.nombre_archivo_s3)).First();
                    referencia refer = new referencia();
                    refer.id_archivo = ar2.id_archivo;
                    refer.id_repositorio = repo_id;
                    refer.id_usuario_creador = usuario_id;
                    refer.rama = 1;
                    refer.fecha = new DateTime();
                    refer.clave_archivo = ar.clave_archivo;
                    db2.referencia.Add(refer);
                    db2.SaveChanges();
                }
                return Json(r);
            }
            
        }

        // GET: File
        public ActionResult Files()
        {
            if (System.Web.HttpContext.Current.Session["usernameS"] == null) {Response.StatusCode = 401;}
            return View();
        }

        public ActionResult FileList(int id_repo)
        {
            if (System.Web.HttpContext.Current.Session["usernameS"] == null) { Response.StatusCode = 401; }
            else
            {
                int idUsuario = (int)System.Web.HttpContext.Current.Session["userIdS"];
                string nombreUsuario = System.Web.HttpContext.Current.Session["usernameS"].ToString();
                if (nombreUsuario == null)
                {
                    Response.StatusCode = 401;
                }
                else
                {
                    ContainerEntities db = new ContainerEntities();
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
                            List<referencia> refer = repo.referencia.ToList();
                            foreach (referencia c in refer)
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
            }
            
            return View();
        }

        public ActionResult RepoList()
        {
            if (System.Web.HttpContext.Current.Session["usernameS"] == null) { Response.StatusCode = 401; }
            else
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
                    ContainerEntities db = new ContainerEntities();
                    List<suscripcion> sus = db.suscripcion.Where(x => x.id_usuario == idUsuario).ToList();
                    foreach (suscripcion c in sus)
                    {
                        RepoResponseDto r = new RepoResponseDto();
                        r.id = c.repositorio.id_repositorio;
                        r.nombre = c.repositorio.nombre;
                        repos.Add(r);
                    }
                    ViewBag.repos = repos;
                }
            }
            
            return View();
        }

    }
}

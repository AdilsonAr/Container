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
using Amazon.S3.Transfer;

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
        public JsonResult CreateRepo(string nombre)
        {
            if (System.Web.HttpContext.Current.Session["usernameS"] == null)
            {
                Response.StatusCode = 401;
                return Json(false);
            }
            else
            {
                ContainerEntities db = new ContainerEntities();
                nombre = System.Web.HttpContext.Current.Session["usernameS"].ToString() + "-" + nombre;
                int apariciones = db.repositorio.Where(x => x.nombre.Equals(nombre)).Count();              
                if (apariciones > 0)
                {
                    while (db.repositorio.Any(x => x.nombre.Equals(nombre)))
                    {
                        nombre = nombre + "(" + (++apariciones) + ")";
                    }                   
                }
                int id = (int)System.Web.HttpContext.Current.Session["userIdS"];
                repositorio repo = new repositorio();
                repo.nombre = nombre;
                repo.tipo = "grupo";
                db.repositorio.Add(repo);
                db.SaveChanges();

                ContainerEntities db2 = new ContainerEntities();
                repositorio repoactual = db2.repositorio.Where(x => x.nombre.Equals(nombre)).First();
                suscripcion sus = new suscripcion();
                sus.nivel = "admin";
                sus.id_usuario = id;
                sus.id_repositorio = repoactual.id_repositorio;
                db2.suscripcion.Add(sus);
                db2.SaveChanges();
                return Json(true);
            }
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
                string tipo = file.ContentType;
                decimal peso = file.ContentLength / (1024 * 1024);
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
                    ar.fecha = DateTime.Now;
                    ar.peso_Mb = peso;
                    ar.tipo = tipo;
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
                    refer.fecha = DateTime.Now;
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
            if (System.Web.HttpContext.Current.Session["usernameS"] == null) { Response.StatusCode = 401; }
            return View();
        }

        public JsonResult GetLink(int id)
        {
            if (System.Web.HttpContext.Current.Session["usernameS"] == null)
            {
                Response.StatusCode = 401;
                return Json(new Response(false, "no tienes permisos"), JsonRequestBehavior.AllowGet);
            }
            else
            {
                ContainerEntities db = new ContainerEntities();
                int idUsuario = (int)System.Web.HttpContext.Current.Session["userIdS"];
                List<suscripcion> sus = db.suscripcion.Where(x => x.id_usuario == idUsuario).ToList();
                suscripcion s = sus.Where(x => x.repositorio.referencia.Any(y => y.id_archivo == id)).FirstOrDefault();
                if (s != null)
                {
                    referencia r = s.repositorio.referencia.Where(y => y.id_archivo == id).FirstOrDefault();
                    archivo_s3 ar = db.archivo_s3.Where(x => x.id_archivo == id).FirstOrDefault();
                    link l = new link();
                    l.nombre = Guid.NewGuid().ToString();
                    l.id_referencia = r.id_referencia;
                    l.id_usuario_creador = idUsuario;
                    l.tipo = "public";
                    link existente = db.link.Where(x => x.id_referencia == l.id_referencia && x.tipo.Equals(l.tipo)).FirstOrDefault();
                    if (existente == null)
                    {
                        db.link.Add(l);
                        db.SaveChanges();
                    }
                    else
                    {
                        l.nombre = existente.nombre;
                    }
                    
                    return Json(new Response(true, "/File/Link?nombre=" + l.nombre), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(new Response(false, "no se encontro el recurso"), JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Link(string nombre)
        {
            ViewBag.res = "Ningun archivo encontrado";
            ViewBag.found = false;
            ContainerEntities db = new ContainerEntities();
            link l = db.link.Where(x => x.nombre == nombre).FirstOrDefault();
            if (l != null && l.tipo.Equals("public")) {
                archivo_s3 a = db.archivo_s3.Where(x => x.id_archivo == l.referencia.id_archivo).FirstOrDefault();
                if (a != null)
                {
                    ViewBag.res = a.nombre_archivo_app;
                    ViewBag.tipo = a.tipo;
                    ViewBag.peso = a.peso_Mb;
                    ViewBag.link = nombre;
                    ViewBag.found = true;
                }
            }
            return View();
        }

        public FileResult DownloadLink(string nombre)
        {
            ContainerEntities db = new ContainerEntities();
            link l = db.link.Where(x => x.nombre == nombre).FirstOrDefault();
            if (l != null)
            {
                archivo_s3 ar = db.archivo_s3.Where(x => x.id_archivo == l.referencia.id_archivo).FirstOrDefault();
                if (ar == null)
                {
                    Response.StatusCode = 400;
                    return null;
                }
                if (l.tipo.Equals("public"))
                {
                    //download
                    string path = Path.Combine(Server.MapPath("~/Uploads"), Path.GetFileName(ar.nombre_archivo_app));
                    TransferUtility t = repo.getUtility();
                    t.Download(path, bucket, ar.nombre_archivo_s3);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                    t.Dispose();
                    string fileName = ar.nombre_archivo_app;
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    if (System.Web.HttpContext.Current.Session["usernameS"] == null)
                    {
                        Response.StatusCode = 401;
                        return null;
                    }
                    else
                    {
                        //download
                        string path = Path.Combine(Server.MapPath("~/Uploads"), Path.GetFileName(ar.nombre_archivo_app));
                        TransferUtility t = repo.getUtility();
                        t.Download(path, bucket, ar.nombre_archivo_s3);
                        byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                        t.Dispose();
                        string fileName = ar.nombre_archivo_app;
                        return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                    }
                }
            }
            else{
                Response.StatusCode = 400;
                return null;
            }

        }

        public FileResult Download(int id)
        {
            if (System.Web.HttpContext.Current.Session["usernameS"] == null) { 
                Response.StatusCode = 401;
                return null;
            }
            else
            {
                ContainerEntities db = new ContainerEntities();
                int idUsuario = (int)System.Web.HttpContext.Current.Session["userIdS"];
                List<suscripcion> sus = db.suscripcion.Where(x => x.id_usuario == idUsuario).ToList();
                bool valid = false;
                foreach(suscripcion c in sus)
                {
                    if(c.repositorio.referencia.Any(y => y.id_archivo == id))
                    {
                        valid = true;
                        break;
                    }
                }
                if (valid)
                {
                    archivo_s3 ar = db.archivo_s3.Where(x => x.id_archivo == id).FirstOrDefault();
                    if (ar == null)
                    {
                        Response.StatusCode = 400;
                        return null;
                    }
                    string path = Path.Combine(Server.MapPath("~/Uploads"), Path.GetFileName(ar.nombre_archivo_app));
                    TransferUtility t = repo.getUtility();
                    t.Download(path, bucket, ar.nombre_archivo_s3);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                    t.Dispose();
                    string fileName = ar.nombre_archivo_app;
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    Response.StatusCode = 401;
                    return null;
                }                
            }
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

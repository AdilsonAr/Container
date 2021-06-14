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
    public class LinkController : Controller
    {
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
                suscripcion s = db.suscripcion.Where(x => x.id_usuario == idUsuario)
                    .Where(x => x.repositorio.referencia.Any(y => y.id_referencia == id)).FirstOrDefault();

                if (s != null)
                {
                    referencia r = s.repositorio.referencia.Where(y => y.id_referencia == id).FirstOrDefault();
                    link l = new link();
                    l.nombre = Guid.NewGuid().ToString();
                    l.id_referencia = r.id_referencia;
                    l.id_usuario_creador = idUsuario;
                    link existente = db.link.Where(x => x.id_referencia == l.id_referencia).FirstOrDefault();
                    if (existente == null)
                    {
                        db.link.Add(l);
                        db.SaveChanges();
                    }
                    else
                    {
                        l.nombre = existente.nombre;
                    }

                    return Json(new Response(true, "/Link/Link?nombre=" + l.nombre), JsonRequestBehavior.AllowGet);
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
            if (l != null)
            {
                archivo_s3 a = db.archivo_s3.Where(x => x.id_archivo == l.referencia.id_archivo).FirstOrDefault();
                if (a != null)
                {
                    ViewBag.res = a.nombre_archivo_app;
                    ViewBag.tipo = a.tipo;
                    ViewBag.peso = a.peso_bytes;
                    ViewBag.link = nombre;
                    ViewBag.found = true;
                }
            }
            return View();
        }

        public FileResult DownloadLink(string nombre)
        {
            FileRepo repo = new FileRepo();
            string bucket = "ar-container-bucket";

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
                Response.StatusCode = 400;
                return null;
            }

        }
    }
}
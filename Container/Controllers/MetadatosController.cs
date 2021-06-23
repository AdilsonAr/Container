using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Container.Dto;
using Container.Models;
namespace Container.Controllers
{
    public class MetadatosController : Controller
    {
        public ActionResult FileDetails(int id)
        {
            if (System.Web.HttpContext.Current.Session["usernameS"] == null)
            {
                Response.StatusCode = 401;
                return null;
            }
            else
            {
                ContainerEntities db = new ContainerEntities();
                int idUsuario = (int)System.Web.HttpContext.Current.Session["userIdS"];
                List<suscripcion> sus = db.suscripcion.Where(x => x.id_usuario == idUsuario).ToList();
                suscripcion susCurrent = null;
                bool valid = false;
                foreach (suscripcion c in sus)
                {
                    if (c.repositorio.referencia.Any(y => y.id_referencia == id))
                    {
                        valid = true;
                        susCurrent = c;
                        break;
                    }
                }
                if (valid)
                {
                    string nivel = "na";
                    referencia r = db.referencia.Where(y => y.id_referencia == id).FirstOrDefault();
                    archivo_s3 ar = db.archivo_s3.Where(x => x.id_archivo == r.id_archivo).FirstOrDefault();
                    usuario creador = db.usuario.Where(x => x.id_usuario == ar.id_autor).FirstOrDefault();
                    if (ar == null)
                    {
                        Response.StatusCode = 400;
                        return null;
                    }
                    if(creador.id_usuario==idUsuario || susCurrent.nivel.Equals("admin"))
                    {
                        nivel = "admin";
                    }
                    ViewBag.nombre = ar.nombre_archivo_app;
                    ViewBag.tipo = ar.tipo;
                    ViewBag.version = r.vers.Value;
                    ViewBag.fecha = ar.fecha.Value;
                    ViewBag.peso = ar.peso_bytes.Value;
                    ViewBag.autor = creador.usuario1;
                    ViewBag.nivel = nivel;
                    return View();
                }
                else
                {
                    Response.StatusCode = 401;
                    return null;
                }
            }
        }
    }
}

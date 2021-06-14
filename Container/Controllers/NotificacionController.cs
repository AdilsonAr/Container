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
    public class NotificacionController : Controller
    {
        public ActionResult Notificaciones()
        {
            int idUsuario = (int)System.Web.HttpContext.Current.Session["userIdS"];
            ContainerEntities db = new ContainerEntities();
            List<link_app> l = db.link_app.Where(z => z.id_usuario == idUsuario && z.aceptada.Equals("no")).ToList();
            List<Link_appDto> listDto = new List<Link_appDto>();
            foreach (link_app c in l)
            {
                Link_appDto dto = new Link_appDto();
                dto.mensaje = db.usuario.Where(z => z.id_usuario == c.id_usuario_creador).FirstOrDefault().usuario1
                    + " te envio el link de " + db.archivo_s3.Where(z => z.id_archivo
                      == (db.referencia.Where(a => a.id_referencia
                       == c.id_referencia).FirstOrDefault()).id_referencia).FirstOrDefault().nombre_archivo_app;
                dto.id_elemento = c.id_link_app;
                listDto.Add(dto);
            }

            List<suscripcion> sus = db.suscripcion.Where(z => z.id_usuario == idUsuario && z.aceptada.Equals("no")).ToList();
            List<SuscripcionDto> listDto2 = new List<SuscripcionDto>();
            foreach (suscripcion c in sus)
            {
                SuscripcionDto dto2 = new SuscripcionDto();
                dto2.mensaje = db.usuario.Where(z => z.id_usuario == c.id_usuario_creador).FirstOrDefault()
                    + "te ha invitado al repositorio " + c.repositorio.nombre;
                dto2.id_elemento = c.id_suscripcion;
                listDto2.Add(dto2);
            }

            ViewBag.notificaciones = listDto;
            ViewBag.invitaciones = listDto2;
            return View();
        }

        public JsonResult AceptarInvitacion(int id)
        {
            ContainerEntities db = new ContainerEntities();
            suscripcion sus = db.suscripcion.Where(x => x.id_suscripcion == id).FirstOrDefault();
            if (System.Web.HttpContext.Current.Session["usernameS"] == null
                || sus == null
                || sus.id_usuario != (int)System.Web.HttpContext.Current.Session["userIdS"])
            {
                Response.StatusCode = 400;
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            else
            {
                sus.aceptada = "si";
                db.SaveChanges();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult EliminarInvitacion(int id)
        {
            ContainerEntities db = new ContainerEntities();
            suscripcion sus = db.suscripcion.Where(x => x.id_suscripcion == id).FirstOrDefault();
            if (System.Web.HttpContext.Current.Session["usernameS"] == null
                || sus == null
                || sus.id_usuario != (int)System.Web.HttpContext.Current.Session["userIdS"])
            {
                Response.StatusCode = 400;
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            else
            {
                db.suscripcion.Remove(sus);
                db.SaveChanges();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult AceptarLink(int id)
        {
            ContainerEntities db = new ContainerEntities();
            link_app link = db.link_app.Where(x => x.id_link_app == id).FirstOrDefault();
            if (System.Web.HttpContext.Current.Session["usernameS"] == null
                || link == null
                || link.id_usuario != (int)System.Web.HttpContext.Current.Session["userIdS"])
            {
                Response.StatusCode = 400;
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            else
            {
                link.aceptada = "si";
                db.SaveChanges();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult EliminarLink(int id)
        {
            ContainerEntities db = new ContainerEntities();
            link_app link = db.link_app.Where(x => x.id_link_app == id).FirstOrDefault();
            if (System.Web.HttpContext.Current.Session["usernameS"] == null
                || link == null
                || link.id_usuario != (int)System.Web.HttpContext.Current.Session["userIdS"])
            {
                Response.StatusCode = 400;
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            else
            {
                db.link_app.Remove(link);
                db.SaveChanges();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
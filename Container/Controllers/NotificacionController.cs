using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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

            List<suscripcion> sus = db.suscripcion.Where(z => z.id_usuario == idUsuario && z.aceptada.Equals("no")).ToList();
            List<SuscripcionDto> listDto2 = new List<SuscripcionDto>();
            foreach (suscripcion c in sus)
            {
                SuscripcionDto dto2 = new SuscripcionDto();
                dto2.mensaje = db.usuario.Where(z => z.id_usuario == c.id_usuario_creador).FirstOrDefault().usuario1
                    + " te ha invitado al repositorio " + c.repositorio.nombre;
                dto2.id_elemento = c.id_suscripcion;
                listDto2.Add(dto2);
            }
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
    }
}
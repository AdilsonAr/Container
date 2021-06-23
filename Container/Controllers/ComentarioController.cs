using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Container.Models;
using Container.Dto;
using Container.Service;
namespace Container.Controllers
{
    public class ComentarioController : Controller
    {
        public JsonResult NewComentario(string contenido, int id_referencia)
        {
            if (System.Web.HttpContext.Current.Session["usernameS"] == null)
            {
                Response.StatusCode = 401;
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            int idUsuario = (int)System.Web.HttpContext.Current.Session["userIdS"];
          
            if (!AccesoService.hasAccess(id_referencia, idUsuario))
            {
                Response.StatusCode = 400;
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            ContainerEntities db = new ContainerEntities();
            comentario co = new comentario();
            co.contenido = contenido;
            co.fecha = DateTime.Now;
            co.id_referencia = id_referencia;
            co.id_usuario = idUsuario;
            db.comentario.Add(co);
            db.SaveChanges();
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteComentario(int id_comentario)
        {
            if (System.Web.HttpContext.Current.Session["usernameS"] == null)
            {
                Response.StatusCode = 401;
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            int idUsuario = (int)System.Web.HttpContext.Current.Session["userIdS"];
            ContainerEntities db = new ContainerEntities();
            comentario co = db.comentario.Where(x => x.id_comentario
            == id_comentario && x.id_usuario == idUsuario).FirstOrDefault();

            if (co == null)
            {
                Response.StatusCode = 400;
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            co.contenido = "Comentario eliminado";
            db.SaveChanges();
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Comentarios(int id_referencia)
        {
            if (System.Web.HttpContext.Current.Session["usernameS"] == null)
            {
                Response.StatusCode = 401;
                return View();
            }

            int idUsuario = (int)System.Web.HttpContext.Current.Session["userIdS"];

            if (!AccesoService.hasAccess(id_referencia, idUsuario))
            {
                Response.StatusCode = 400;
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            ContainerEntities db = new ContainerEntities();
            List<comentario> coms = db.comentario.Where(z => z.id_referencia == id_referencia)
                .OrderByDescending(x=>x.fecha).ToList();
            List<ComentarioDto> dtos = new List<ComentarioDto>();
            foreach (comentario c in coms)
            {
                ComentarioDto dto = new ComentarioDto();
                dto.contenido = c.contenido;
                dto.id = c.id_comentario;
                dto.id_user = c.id_usuario.Value;
                dto.user = c.usuario.usuario1;
                dto.fecha = c.fecha.Value;
                dtos.Add(dto);
            }
            ViewBag.comentarios = dtos;
            return View();
        }
    }
}
        
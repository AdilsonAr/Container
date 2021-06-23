using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Container.Models;
namespace Container.Service
{
    public class AccesoService
    {
        public static bool hasAccess(int id_referencia, int id_usuario)
        {
            ContainerEntities db = new ContainerEntities();
            int idUsuario = (int)System.Web.HttpContext.Current.Session["userIdS"];
            referencia refer = db.referencia.Where(x => x.id_referencia == id_referencia).FirstOrDefault();
            if (refer == null) return false;
            suscripcion sus = refer.repositorio.suscripcion.Where(z => z.id_usuario == id_usuario).FirstOrDefault();
            if (sus == null) return false;
            return true;
        }

        public static suscripcion hasAccessWithSuscripcion(int id_referencia, int id_usuario)
        {
            ContainerEntities db = new ContainerEntities();
            int idUsuario = (int)System.Web.HttpContext.Current.Session["userIdS"];
            referencia refer = db.referencia.Where(x => x.id_referencia == id_referencia).FirstOrDefault();
            if (refer == null) return null;
            suscripcion sus = refer.repositorio.suscripcion.Where(z => z.id_usuario == id_usuario).FirstOrDefault();
            if (sus == null) return null;
            return sus;
        }
    }
}
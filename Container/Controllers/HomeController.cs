using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Container.Models;
using Container.Dto;

namespace Container.Controllers

{
    public class HomeController : Controller
    {
        ContainerEntities db = new ContainerEntities();

        [HttpGet]
        public ActionResult Container()
        {
            if (System.Web.HttpContext.Current.Session["usernameS"] == null) { Response.StatusCode = 401; }
            return View();
        }
        // GET: Home
        public ActionResult IndexParcial()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SignUpParcial()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignUpParcial(usuarioDto u)
        {
            if (ModelState.IsValid)
            {
                if (db.usuario.Any(x => x.usuario1.Equals(u.usuario1)))
                {
                    ViewBag.Notification = "Esta cuenta ya existe";
                    Response.StatusCode = 400;
                    return View();
                }
                else
                {
                    u.nivel = "principiante";
                    db.usuario.Add(usuarioDto.toModel(u));
                    repositorio repo = new repositorio();
                    repo.tipo = "personal";
                    repo.nombre = u.usuario1;
                    db.repositorio.Add(repo);
                    db.SaveChanges();

                    ContainerEntities db2 = new ContainerEntities();
                    usuario actual = db2.usuario.Where(x => x.usuario1.Equals(u.usuario1)).First();
                    repositorio repoactual = db2.repositorio.Where(x => x.nombre.Equals(actual.usuario1)).First();
                    suscripcion sus = new suscripcion();
                    sus.nivel = "admin";
                    sus.id_usuario = actual.id_usuario;
                    sus.id_repositorio = repoactual.id_repositorio;
                    db2.suscripcion.Add(sus);
                    db2.SaveChanges();
                    Session["userIdS"] = actual.id_usuario;
                    Session["usernameS"] = actual.usuario1.ToString();
                    Response.StatusCode = 200;
                    return View();
                }
            }
            else
            {
                ViewBag.Notification = "Errores en los datos insertados";
                Response.StatusCode = 400;
                return View();
            }           
        }

        public ActionResult LogOut()
        {
            if (System.Web.HttpContext.Current.Session["usernameS"] == null) { Response.StatusCode = 401; }
            Session.Clear();
            return RedirectToAction("LoginParcial", "Home");
        }

        [HttpGet]
        public ActionResult LoginParcial()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoginParcial(loginDto dto)
        {
            if (ModelState.IsValid)
            {
                var login = db.usuario.Where(x => x.usuario1.Equals(dto.usuario) && x.clave.Equals(dto.clave)).FirstOrDefault();
                if (login != null)
                {
                    Session["userIdS"] = login.id_usuario;
                    Session["usernameS"] = login.usuario1.ToString();
                    Response.StatusCode = 200;
                    return View();
                }
                else
                {
                    ViewBag.Notification = "Usuario o Password incorrectos ";
                    Response.StatusCode = 401;
                    return View();
                }
                
            }
            else
            {
                ViewBag.Notification = "Errores en los datos insertados";
                Response.StatusCode = 400;
                return View();
            }                       
        }
    }
}
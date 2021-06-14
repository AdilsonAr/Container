﻿using System;
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

        public ActionResult RepoConf(int id_repo)
        {
            return View();
        }
        public ActionResult Usuarios(string nombre)
        {
            List<string> l = new List<string>();
            if (nombre.Length >= 1)
            {
                ContainerEntities db = new ContainerEntities();
                List<usuario> us = db.usuario.Where(z => z.usuario1.Contains(nombre)).ToList();
                foreach(usuario c in us)
                {
                    l.Add(c.usuario1);
                }
            }
            ViewBag.usuarios = l;
            return View();
        }

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
                    while (db.repositorio.Any(x => x.nombre.Equals(nombre + "(" + apariciones + ")")))
                    {
                        apariciones++;
                    }
                    nombre = nombre + "(" + apariciones + ")";
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
                sus.aceptada = "si";
                db2.suscripcion.Add(sus);
                db2.SaveChanges();
                return Json(true);
            }
        }

        [HttpPost]
        public async Task<JsonResult> UploadFile(HttpPostedFileBase file, int repo_id, string rama)
        {
            ContainerEntities db = new ContainerEntities();
            int usuario_id = (int)System.Web.HttpContext.Current.Session["userIdS"];
            if (System.Web.HttpContext.Current.Session["usernameS"] == null ||
                db.suscripcion.Where(x => x.id_usuario == usuario_id && x.id_repositorio == repo_id).FirstOrDefault()==null) {
                Response.StatusCode = 401;
                return Json(false);
            }
            else
            {  
                string nombre = Guid.NewGuid().ToString();
                string tipo = file.ContentType;
                int peso = file.ContentLength;
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
                    ar.peso_bytes = peso;
                    ar.tipo = tipo;
                    ar.clave_archivo = Guid.NewGuid().ToString();
                    
                    db.archivo_s3.Add(ar);
                    db.SaveChanges();

                    ContainerEntities db2 = new ContainerEntities();
                    archivo_s3 ar2 = db2.archivo_s3.Where(x => x.clave_archivo.Equals(ar.clave_archivo)
                      && x.nombre_archivo_s3.Equals(ar.nombre_archivo_s3)).First();
                    referencia refer = new referencia();
                    refer.id_archivo = ar2.id_archivo;
                    refer.id_repositorio = repo_id;
                    refer.id_usuario_creador = usuario_id;

                    if (rama.Length < 20)
                    {
                        refer.rama = Guid.NewGuid().ToString();
                        refer.vers = 1; 
                    }
                    else
                    {
                        refer.rama = rama;
                        refer.vers = db.referencia.Where(x => x.rama.Equals(rama)).Count() + 1;
                    }                    

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
                    if(c.repositorio.referencia.Any(y => y.id_referencia == id))
                    {
                        valid = true;
                        break;
                    }
                }
                if (valid)
                {
                    referencia r = db.referencia.Where(y => y.id_referencia == id).FirstOrDefault();
                    archivo_s3 ar = db.archivo_s3.Where(x => x.id_archivo == r.id_archivo).FirstOrDefault();
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

                ContainerEntities db = new ContainerEntities();
                repositorio repo = db.repositorio.Where(x => x.id_repositorio == id_repo).First();
                suscripcion sus = db.suscripcion.Where(x => x.id_repositorio == id_repo
                     && x.id_usuario == idUsuario && x.aceptada.Equals("si")).First();
                if (repo == null || sus == null)
                {
                    Response.StatusCode = 400;
                }
                else
                {
                    List<Archivo_s3_ResponseDto> archDto = new List<Archivo_s3_ResponseDto>();
                    //unicamente las versiones 1
                    List<referencia> refer = repo.referencia.Where(x=>x.vers==1).ToList();
                    foreach (referencia c in refer)
                    {
                        Archivo_s3_ResponseDto arch = new Archivo_s3_ResponseDto();
                        arch.nombre = c.archivo_s3.nombre_archivo_app;
                        arch.rama = c.rama;
                        archDto.Add(arch);
                    }
                    ViewBag.files = archDto;
                    ViewBag.repo = id_repo;
                    ViewBag.repoName = repo.nombre;
                }
            }
            
            return View();
        }

        public ActionResult Versiones(int id_repo, string rama)
        {
            if (System.Web.HttpContext.Current.Session["usernameS"] == null) { Response.StatusCode = 401; }
            else
            {
                int idUsuario = (int)System.Web.HttpContext.Current.Session["userIdS"];

                ContainerEntities db = new ContainerEntities();
                repositorio repo = db.repositorio.Where(x => x.id_repositorio == id_repo).First();
                suscripcion sus = db.suscripcion.Where(x => x.id_repositorio == id_repo
                     && x.id_usuario == idUsuario && x.aceptada.Equals("si")).First();
                string nivel = sus.nivel;
                List<referencia> refer = repo.referencia.Where(x => x.rama.Equals(rama)).ToList();
                if (repo == null || sus == null || refer == null)
                {
                    Response.StatusCode = 400;
                }
                else
                {
                    List<VersionDto> archDto = new List<VersionDto>();
                    //unicamente las versiones 1

                    foreach (referencia c in refer)
                    {
                        VersionDto arch = new VersionDto();
                        arch.nombre = c.archivo_s3.nombre_archivo_app;
                        arch.id_referencia = c.id_referencia;
                        arch.version = c.vers.Value;
                        archDto.Add(arch);
                    }
                    ViewBag.files = archDto;
                    ViewBag.repo = id_repo;
                    ViewBag.rama = rama;
                    ViewBag.repoName = repo.nombre;
                    ViewBag.nivel = nivel;
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
                    List<suscripcion> sus = db.suscripcion.Where(x => x.id_usuario 
                    == idUsuario && x.aceptada.Equals("si")).ToList();
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

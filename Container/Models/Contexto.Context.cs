﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Container.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ContainerEntities : DbContext
    {
        public ContainerEntities()
            : base("name=ContainerEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<archivo_s3> archivo_s3 { get; set; }
        public virtual DbSet<comentario> comentario { get; set; }
        public virtual DbSet<link> link { get; set; }
        public virtual DbSet<link_app> link_app { get; set; }
        public virtual DbSet<referencia> referencia { get; set; }
        public virtual DbSet<repositorio> repositorio { get; set; }
        public virtual DbSet<suscripcion> suscripcion { get; set; }
        public virtual DbSet<usuario> usuario { get; set; }
    }
}

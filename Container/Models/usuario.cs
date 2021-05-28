//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class usuario
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public usuario()
        {
            this.archivo_s3 = new HashSet<archivo_s3>();
            this.avatar_s3 = new HashSet<avatar_s3>();
            this.comentario = new HashSet<comentario>();
            this.link = new HashSet<link>();
            this.referencia = new HashSet<referencia>();
            this.suscripcion = new HashSet<suscripcion>();
        }
    
        public int id_usuario { get; set; }
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string usuario1 { get; set; }
        public string correo { get; set; }
        public string nivel { get; set; }
        public string clave { get; set; }

        public usuario(int id_usuario, string nombres, string apellidos, string usuario1, 
            string correo,string nivel, string clave)
        {
            this.id_usuario = id_usuario;
            this.nombres = nombres;
            this.apellidos = apellidos;
            this.usuario1 = usuario1;
            this.correo = correo;
            this.nivel = nivel;
            this.clave = clave;
        }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<archivo_s3> archivo_s3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<avatar_s3> avatar_s3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<comentario> comentario { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<link> link { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<referencia> referencia { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<suscripcion> suscripcion { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Container.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Container.Dto
{
    public class usuarioDto
    {
        public int id_usuario { get; set; }
        [Required(ErrorMessage = "Este campo es requerido")]
        [Display(Name = "Nombres")]
        public string nombres { get; set; }
        [Required(ErrorMessage = "Este campo es requerido")]
        [Display(Name = "Apellidos")]
        public string apellidos { get; set; }
        [Required(ErrorMessage = "Este campo es requerido")]
        [Display(Name = "Nombre de usuario")]
        public string usuario1 { get; set; }
        [Required(ErrorMessage = "Este campo es requerido")]
        [Display(Name = "E-mail")]
        public string correo { get; set; }
        public string nivel { get; set; }
        [Required(ErrorMessage = "Este campo es requerido")]
        [Display(Name = "Contraseña")]
        public string clave { get; set; }
        [Required(ErrorMessage = "Este campo es requerido")]
        [Display(Name = "Repita Contraseña")]
        [DataType(DataType.Password)]
        [NotMapped]
        [Compare("clave", ErrorMessage = "Contraseñas no coinciden")]
        public string clave2 { get; set; }

        public static usuario toModel(usuarioDto u)
        {
            return new usuario(u.nombres, u.apellidos, u.usuario1, u.correo,u.nivel, u.clave);
        }
    }
}
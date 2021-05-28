using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Container.Dto
{
    public class loginDto
    {
        [Required(ErrorMessage = "Este campo es requerido")]
        [Display(Name = "Nombre de usuario")]
        public string usuario { get; set; }
        [Required(ErrorMessage = "Este campo es requerido")]
        [Display(Name = "Contraseña")]
        public string clave { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Container.Dto
{
    public class ComentarioDto
    {
        public int id { get; set; }
        public int id_user { get; set; }
        public string user { get; set; }
        public string contenido { get; set; }
        public DateTime fecha { get; set; }
    }
}
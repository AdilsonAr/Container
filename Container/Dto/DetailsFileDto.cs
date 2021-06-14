using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Container.Dto
{
    public class DetailsFileDto
    {
        public int version { get; set; }
        public string nombre { get; set; }
        public string tipo { get; set; }
        public DateTime fecha { get; set; }
        public int peso { get; set; }
    }
}
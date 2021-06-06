using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Container.Dto
{
    public class Response
    {
        public bool success;
        public string m;

        public Response(bool success, string m)
        {
            this.success = success;
            this.m = m;
        }
    }
}
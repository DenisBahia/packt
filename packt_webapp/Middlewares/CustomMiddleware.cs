using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace packt_webapp.Middlewares
{
    public class CustomMiddleware
    {
        public readonly RequestDelegate _next;

        public CustomMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            Debug.WriteLine($" ----> Request asked for {httpContext.Request.Path}");
            await _next.Invoke(httpContext);
        }

    }
}

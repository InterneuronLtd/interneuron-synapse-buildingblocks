 //Interneuron synapse

//Copyright(C) 2024 Interneuron Limited

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

//See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
﻿using Microsoft.AspNetCore.Http;

namespace Interneuron.Web.Logger
{
    public class InterneuronResetRequestBodyStreamMiddleware
    {
        private readonly RequestDelegate _next;

        public InterneuronResetRequestBodyStreamMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Still enable buffering before anything reads
                if (context != null && context.Request != null)
                    context.Request.EnableBuffering();
            }
            catch { }

            // Call the next delegate/middleware in the pipeline
            await _next(context);

            try
            {
                // Reset the request body stream position to the start so we can read it
                if (context != null && context.Request != null && context.Request.Body != null && ((string.Compare(context.Request.Method, "post", true) == 0) || (string.Compare(context.Request.Method, "put", true) == 0) || (string.Compare(context.Request.Method, "patch", true) == 0)))
                    context.Request.Body.Position = 0;
            }
            catch { }
        }
    }
}

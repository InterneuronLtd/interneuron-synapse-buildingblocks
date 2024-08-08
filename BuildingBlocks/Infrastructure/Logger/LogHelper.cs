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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Text;

namespace Interneuron.Web.Logger
{
    public static class LogHelper
    {
        public static async void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
        {
            //This properties are optional and can be supressed
            try
            {
                var request = httpContext.Request;
                var userName = httpContext?.User?.FindFirst("IPUId")?.Value;
                userName = (userName == null || userName == "") ? httpContext?.User?.Identity?.Name : userName;
                userName = (userName == null || userName == "") ? "UNKNOWN" : userName;

                // Set all the common properties available for every request
                diagnosticContext.Set("Host", request.Host);
                diagnosticContext.Set("Protocol", request.Protocol);
                diagnosticContext.Set("Scheme", request.Scheme);
                diagnosticContext.Set("UserName", userName);

                try
                {
                    if (request != null && ((string.Compare(request.Method, "post", true) == 0) || (string.Compare(request.Method, "put", true) == 0) || (string.Compare(request.Method, "patch", true) == 0)))
                    {
                        // Read and log request body data
                        var requestBodyPayload = await GetRawBodyString(httpContext, Encoding.UTF8);
                        //var requestBodyPayload = await GetRawBodyString(httpContext, Encoding.UTF8);
                        //var requestBodyPayload = await ReadRequestBodyAsync(httpContext);
                        diagnosticContext.Set("RequestBody", requestBodyPayload);
                    }
                }
                catch { }
                //// Read and log request body data
                //var requestBodyPayload = await GetRawBodyString(httpContext, Encoding.UTF8);
                ////var requestBodyPayload = await GetRawBodyString(httpContext, Encoding.UTF8);
                ////var requestBodyPayload = await ReadRequestBodyAsync(httpContext);
                //diagnosticContext.Set("RequestBody", requestBodyPayload);

                //var x = await GetRawBodyString(httpContext, Encoding.UTF8);
                //var y = await ReadRequestBodyAsync1(httpContext);


                // Only set it if available. You're not sending sensitive data in a querystring right?!
                if (request.QueryString.HasValue)
                {
                    diagnosticContext.Set("QueryString", request.QueryString.Value);
                }


                // Set the content-type of the Response at this point
                diagnosticContext.Set("ContentType", httpContext.Response.ContentType);

                // Retrieve the IEndpointFeature selected for the request
                var endpoint = httpContext.GetEndpoint();
                if (endpoint is object) // endpoint != null
                {
                    diagnosticContext.Set("EndpointName", endpoint.DisplayName);
                }
            }
            catch { }
        }

        public static async Task<string> GetRawBodyString(HttpContext httpContext, Encoding encoding)
        {
            var body = "";

            if (httpContext.Request.ContentLength == null || !(httpContext.Request.ContentLength > 0) ||
                !httpContext.Request.Body.CanRead) return body;
            httpContext.Request.EnableBuffering();
            httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(httpContext.Request.Body, encoding, true, 1024, true))
            {
                body = await reader.ReadToEndAsync();
            }
            httpContext.Request.Body.Position = 0;
            return body;
        }

        public static async Task<string> GetRawBodyStringAsync(HttpContext httpContext, Encoding encoding)
        {

            var body = "";
            httpContext.Request.EnableBuffering();

            httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            using (StreamReader stream = new StreamReader(httpContext.Request.Body))
            {
                string body1 = await stream.ReadToEndAsync();
            }
            httpContext.Request.Body.Position = 0;

            // Leave the body open so the next middleware can read it.
            using (var reader = new StreamReader(
                httpContext.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                // Do some processing with body…

                // Reset the request body stream position so the next middleware can read it
                httpContext.Request.Body.Position = 0;
            }
            return body;

            //if (httpContext.Request.ContentLength == null || !(httpContext.Request.ContentLength > 0) ||
            //    !httpContext.Request.Body.CanSeek) return body;
            //httpContext.Request.EnableBuffering();
            //httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            //using (var reader = new StreamReader(httpContext.Request.Body, encoding, true, 1024, true))
            //{
            //    body = reader.ReadToEnd();
            //}
            //httpContext.Request.Body.Position = 0;
            //return body;
        }

        private static async Task<string> ReadRequestBodyAsync(HttpContext httpContext)
        {
            var request = httpContext.Request;

            //HttpRequestRewindExtensions.EnableBuffering(request);

            //var body = request.Body;
            //var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            //await request.Body.ReadAsync(buffer, 0, buffer.Length);
            //string requestBody = Encoding.UTF8.GetString(buffer);
            //body.Seek(0, SeekOrigin.Begin);
            //request.Body = body;

            //return $"{requestBody}";

            // Getting the request body is a little tricky because it's a stream
            // So, we need to read the stream and then rewind it back to the beginning
            //string requestBody = "";
            //httpContext.Request.EnableBuffering();
            //Stream body = httpContext.Request.Body;
            //byte[] buffer = new byte[Convert.ToInt32(httpContext.Request.ContentLength)];
            //await httpContext.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            //requestBody = Encoding.UTF8.GetString(buffer);
            //body.Seek(0, SeekOrigin.Begin);
            //httpContext.Request.Body = body;
            //return $"{requestBody}";



            request.EnableBuffering();
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            //get body string here...
            var requestContent = Encoding.UTF8.GetString(buffer);

            var requestBody = "";
            request.EnableBuffering();
            request.Body.Seek(0, SeekOrigin.Begin);
            using (StreamReader stream = new StreamReader(request.Body))
            {
                requestBody = await stream.ReadToEndAsync();
            }

            request.Body.Position = 0;  //rewinding the stream to 0

            return $"{requestBody}";


            // Reset the request body stream position to the start so we can read it
            //httpContext.Request.Body.Position = 0;

            //// Leave the body open so the next middleware can read it.
            //using StreamReader reader = new(
            //    httpContext.Request.Body,
            //    encoding: Encoding.UTF8,
            //    detectEncodingFromByteOrderMarks: false);

            //string body1 = await reader.ReadToEndAsync();
            //return body1;
            //if (body1.Length is 0)
            //    return "";

            //object? obj = JsonSerializer.Deserialize<object>(body1);
        }

        private static async Task<JObject> ReadRequestBodyAsync1(HttpContext httpContext)
        {
            var request = httpContext.Request;

            JObject objRequestBody = new JObject();
            try
            {
                // set the pointer to the beninig of the stream in case it already been readed
                request.Body.Position = 0;
                using (StreamReader reader = new StreamReader(request.Body))
                {
                    string strRequestBody = await reader.ReadToEndAsync();
                    objRequestBody = JsonConvert.DeserializeObject<JObject>(strRequestBody);
                }
            }
            finally
            {
                request.Body.Position = 0;
            }
            return objRequestBody;
        }
    }
}

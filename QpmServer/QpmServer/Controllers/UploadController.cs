using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace QpmServer.Controllers
{
    public class UploadController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            var root = HttpContext.Current.Server.MapPath("~/App_Data");
            var files = Directory.EnumerateFiles(root).ToList();
            return Json(files);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

            // Read the file and form data.
            MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Extract the fields from the form data.
            string description = provider.FormData["description"];
            int uploadType;
            if (!Int32.TryParse(provider.FormData["uploadType"], out uploadType))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
            }

            // Check if files are on the request.
            if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
            }

            IList<string> uploadedFiles = new List<string>();
            foreach (KeyValuePair<string, Stream> file in provider.FileStreams)
            {
                string fileName = file.Key;
                Stream stream = file.Value;

                // Do something with the uploaded file
                //UploadManager.Upload(stream, fileName, uploadType, description);

                // Keep track of the filename for the response
                uploadedFiles.Add(fileName);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Successfully Uploaded: " + string.Join(", ", uploadedFiles));
        }

        //public async Task<HttpResponseMessage> PostFormData()
        //{
        //    // Check if the request contains multipart/form-data.
        //    if (!Request.Content.IsMimeMultipartContent())
        //    {
        //        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        //    }

        //    var root = HttpContext.Current.Server.MapPath("~/App_Data");
        //    var provider = new MultipartFormDataStreamProvider(root);

        //    try
        //    {
        //        // Read the form data.
        //        await Request.Content.ReadAsMultipartAsync(provider);

        //        // This illustrates how to get the file names.
        //        foreach (var file in provider.FileData)
        //        {
        //            Trace.WriteLine(file.Headers.ContentDisposition.FileName);
        //            Trace.WriteLine("Server file path: " + file.LocalFileName);
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK);
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
        //    }
        //}
    }
}
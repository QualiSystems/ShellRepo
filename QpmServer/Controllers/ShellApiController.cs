using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using ShellRepo.Engine;
using ShellRepo.Models;

namespace ShellRepo.Controllers
{
    public class ShellApiController : ApiController
    {
        private readonly IWebErrorLogger webErrorLogger;
        private readonly IShellEntityContentRetriever shellEntityContentRetriever;
        private readonly IShellContentEntityCreator shellContentEntityCreator;
        private readonly IShellEntityFinder shellEntityFinder;

        public ShellApiController(IWebErrorLogger webErrorLogger, IShellEntityContentRetriever shellEntityContentRetriever, IShellContentEntityCreator shellContentEntityCreator, IShellEntityFinder shellEntityFinder)
        {
            this.webErrorLogger = webErrorLogger;
            this.shellEntityContentRetriever = shellEntityContentRetriever;
            this.shellContentEntityCreator = shellContentEntityCreator;
            this.shellEntityFinder = shellEntityFinder;
        }

        [AcceptVerbs("GET")]
        [HttpGet]
        [Route("api/shell/list/{shellName}/{version?}")]
        public IHttpActionResult List(string shellName, string version = null)
        {
            try
            {
                Version versionObject = null;
                if (!string.IsNullOrEmpty(version) && !Version.TryParse(version, out versionObject))
                {
                    return BadRequest(string.Format("Invalid version provided '{0}'", version));
                }

                var shellEntities = shellEntityFinder.FindShellEntities(shellName, versionObject);

                return Json(shellEntities);
            }
            catch (Exception exception)
            {
                webErrorLogger.LogError(exception.Message);
                return BadRequest(exception.Message);
            }
        }

        [HttpPost]
        [Route("api/shell/upload")]
        public async Task<HttpResponseMessage> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                webErrorLogger.LogError("Unsupported media type.");
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

            // Read the file and form data.
            var provider = new MultipartFormDataMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Check if files are on the request.
            if (!provider.FileStreams.Any())
            {
                webErrorLogger.LogError("No shell in request.");
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No shell in request.");
            }

            if (provider.FileStreams.Count > 1)
            {
                webErrorLogger.LogError("Single shell publishing allowed.");
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Single shell publishing allowed.");
            }

            try
            {
                var fileStreamKeyValue = provider.FileStreams.Single();

                await shellContentEntityCreator.CreateShellContentEntity(fileStreamKeyValue.Key, fileStreamKeyValue.Value);

                return Request.CreateResponse(HttpStatusCode.OK, "Successfully uploaded: " + fileStreamKeyValue.Key);
            }
            catch (Exception exception)
            {
                webErrorLogger.LogError(exception.Message);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        [HttpGet]
        [Route("api/shell/download/{shellName}/{version?}")]
        public HttpResponseMessage Download(string shellName, string version = null)
        {
            Version versionObject = null;
            if (!string.IsNullOrEmpty(version) && !Version.TryParse(version, out versionObject))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                    string.Format("Invalid version provided '{0}'", version));
            }

            try
            {
                var shellContentEntity = shellEntityContentRetriever.GetShellContentEntity(shellName, versionObject);
                return CreateHttpResponseMessage(shellContentEntity);
            }
            catch (Exception exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, exception.Message);
            }
        }

        private static HttpResponseMessage CreateHttpResponseMessage(ShellContentEntity shellContentEntity)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(shellContentEntity.Content)
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = Path.ChangeExtension(shellContentEntity.Name, ".zip")
            };
            return result;
        }
    }
}
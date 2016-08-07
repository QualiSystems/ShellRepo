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
        private readonly ISearchingService searchingService;
        private readonly IShellEntityContentDownloader shellEntityContentDownloader;
        private readonly IShellEntityFinder shellEntityFinder;
        private readonly IShellUploader shellUploader;
        private readonly IWebErrorLogger webErrorLogger;

        public ShellApiController(IWebErrorLogger webErrorLogger,
            IShellEntityContentDownloader shellEntityContentDownloader, 
            IShellUploader shellUploader,
            IShellEntityFinder shellEntityFinder, 
            ISearchingService searchingService)
        {
            this.webErrorLogger = webErrorLogger;
            this.shellEntityContentDownloader = shellEntityContentDownloader;
            this.shellUploader = shellUploader;
            this.shellEntityFinder = shellEntityFinder;
            this.searchingService = searchingService;
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

        [AcceptVerbs("GET")]
        [HttpGet]
        [Route("api/shell/all")]
        public IHttpActionResult GetAllList()
        {
            try
            {
                var shellEntities = shellEntityFinder.GetAll();

                return Json(shellEntities);
            }
            catch (Exception exception)
            {
                webErrorLogger.LogError(exception.Message);
                return BadRequest(exception.Message);
            }
        }

        [AcceptVerbs("GET")]
        [HttpGet]
        [Route("api/shell/search/{text}")]
        public IHttpActionResult Search(string text)
        {
            try
            {
                var shellEntities = searchingService.Search(text);

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

                await shellUploader.UploadShell(fileStreamKeyValue.Key, fileStreamKeyValue.Value);

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
                var shellContentEntity = shellEntityContentDownloader.DownloadShellContentEntity(shellName,
                    versionObject);
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
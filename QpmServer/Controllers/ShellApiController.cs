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
        private readonly IShellContentEntityRetriever shellContentEntityRetriever;
        private readonly IShellEntityContentDownloader shellEntityContentDownloader;
        private readonly IShellEntityRetriever shellEntityRetriever;
        private readonly IShellUploader shellUploader;
        private readonly IWebErrorLogger webErrorLogger;

        public ShellApiController(IWebErrorLogger webErrorLogger,
            IShellEntityContentDownloader shellEntityContentDownloader,
            IShellUploader shellUploader,
            IShellEntityRetriever shellEntityRetriever,
            ISearchingService searchingService, 
            IShellContentEntityRetriever shellContentEntityRetriever)
        {
            this.webErrorLogger = webErrorLogger;
            this.shellEntityContentDownloader = shellEntityContentDownloader;
            this.shellUploader = shellUploader;
            this.shellEntityRetriever = shellEntityRetriever;
            this.searchingService = searchingService;
            this.shellContentEntityRetriever = shellContentEntityRetriever;
        }

        [AcceptVerbs("GET")]
        [HttpGet]
        [Route("api/shells/versions/{shellName}")]
        public IHttpActionResult GetShellVersions(string shellName)
        {
            try
            {
                var shellEntities = shellContentEntityRetriever.GetShellContentDtos(shellName);

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
        [Route("api/shells")]
        public IHttpActionResult GetShellEntitiesFirstPage()
        {
            return GetShellEntitiesPage(0);
        }

        [AcceptVerbs("GET")]
        [HttpGet]
        [Route("api/shells/page/{pageNumber}")]
        public IHttpActionResult GetShellEntitiesPage(int pageNumber)
        {
            try
            {
                var shellEntities = shellEntityRetriever.GetShellEntitiesWithPaging(pageNumber);

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
        [Route("api/shells/search/{text}")]
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
        [Route("api/shells")]
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
        [Route("api/shells/download/{shellName}")]
        public HttpResponseMessage DownloadLatestVersion(string shellName)
        {
            return DownloadSpecificVersion(shellName, string.Empty);
        }

        [HttpGet]
        [Route("api/shells/download/{shellName}/{version}")]
        public HttpResponseMessage DownloadSpecificVersion(string shellName, string version)
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
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using ShellRepo.Common;
using ShellRepo.Engine;
using ShellRepo.Models;
using Toscana;
using Toscana.Common;
using Toscana.Exceptions;

namespace ShellRepo.Controllers
{
    public class ShellApiController : ApiController
    {
        private readonly IShellEntityRepository shellEntityRepository;
        private readonly IWebErrorLogger webErrorLogger;

        public ShellApiController(IShellEntityRepository shellEntityRepository, IWebErrorLogger webErrorLogger)
        {
            this.shellEntityRepository = shellEntityRepository;
            this.webErrorLogger = webErrorLogger;
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
                    return BadRequest($"Invalid version provided '{version}'");
                }

                return Json(shellEntityRepository.Find(shellName, versionObject).Select(s=>new ShellEntity
                {
                    CreatedBy = s.CreatedBy,
                    Description = s.Description,
                    Name = s.Name,
                    Version = s.Version
                }));
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

            var fileStreamKeyValue = provider.FileStreams.Single();
            var content = fileStreamKeyValue.Value.StreamToByteArray();

            ToscaCloudServiceArchive toscaCloudServiceArchive;
            try
            {
                toscaCloudServiceArchive = ToscaCloudServiceArchive.Load(fileStreamKeyValue.Value);
            }
            catch (ToscaBaseException toscaBaseException)
            {
                webErrorLogger.LogError(toscaBaseException.Message);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, toscaBaseException.Message);
            }

            try
            {
                await shellEntityRepository.Add(new ShellContentEntity
                {
                    Name = Path.GetFileNameWithoutExtension(fileStreamKeyValue.Key),
                    Version = toscaCloudServiceArchive.ToscaMetadata.CsarVersion,
                    CreatedBy = toscaCloudServiceArchive.ToscaMetadata.CreatedBy,
                    Description = toscaCloudServiceArchive.EntryPointServiceTemplate.Description,
                    Content = content
                });
            }
            catch (Exception exception)
            {
                webErrorLogger.LogError(exception.Message);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, exception.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Successfully published: " + fileStreamKeyValue.Key);
        }

        [HttpGet]
        [Route("api/shell/download/{shellName}/{version?}")]
        public HttpResponseMessage Download(string shellName, string version = null)
        {
            Version versionObject = null;
            if (!string.IsNullOrEmpty(version) && !Version.TryParse(version, out versionObject))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            var shellContentEntities = shellEntityRepository.Find(shellName, versionObject);
            if (!shellContentEntities.Any())
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            var latestVersion = shellContentEntities.Max(s => s.Version);
            var shellContentEntity = shellContentEntities.Single(c => c.Version == latestVersion);

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
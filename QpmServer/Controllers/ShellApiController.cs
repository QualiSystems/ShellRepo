using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ShellRepo.Engine;
using ShellRepo.Models;
using Toscana;
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
        [Route("api/shell/list/{shellName}")]
        public IHttpActionResult Get(string shellName)
        {
            try
            {
                return Json(shellEntityRepository.Find(shellName).Select(s=>new ShellEntity
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

            try
            {
                var toscaCloudServiceArchive = ToscaCloudServiceArchive.Load(fileStreamKeyValue.Value);

                shellEntityRepository.Add(new ShellContentEntity
                {
                    Name = Path.GetFileName(fileStreamKeyValue.Key),
                    Version = toscaCloudServiceArchive.ToscaMetadata.CsarVersion,
                    CreatedBy = toscaCloudServiceArchive.ToscaMetadata.CreatedBy,
                    Description = toscaCloudServiceArchive.EntryPointServiceTemplate.Description
                });
            }
            catch (ToscaBaseException toscaBaseException)
            {
                webErrorLogger.LogError(toscaBaseException.Message);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, toscaBaseException.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Successfully published: " + fileStreamKeyValue.Key);
        }
    }
}
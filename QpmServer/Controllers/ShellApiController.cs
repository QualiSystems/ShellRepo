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

        public ShellApiController(IShellEntityRepository shellEntityRepository)
        {
            this.shellEntityRepository = shellEntityRepository;
        }

        [AcceptVerbs("GET")]
        [HttpGet]
        [Route("api/shell/list/{shellName}")]
        public IHttpActionResult Get(string shellName)
        {
            return Json(shellEntityRepository.Find(shellName));
        }

        [HttpPost]
        [Route("api/shell/upload")]
        public async Task<HttpResponseMessage> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

            // Read the file and form data.
            var provider = new MultipartFormDataMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Check if files are on the request.
            if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No shell in request.");
            }

            if (provider.FileStreams.Count > 1)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Single shell publishing allowed.");
            }

            var fileStreamKeyValue = provider.FileStreams.Single();

            try
            {
                var toscaCloudServiceArchive = ToscaCloudServiceArchive.Load(fileStreamKeyValue.Value);
                
                shellEntityRepository.Add(new ShellEntity
                {
                    Name = fileStreamKeyValue.Key,
                    Version = toscaCloudServiceArchive.ToscaMetadata.CsarVersion,
                    CreatedBy = toscaCloudServiceArchive.ToscaMetadata.CreatedBy,
                    Description = toscaCloudServiceArchive.EntryPointServiceTemplate.Description
                });
            }
            catch (ToscaBaseException toscaBaseException)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, toscaBaseException.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Successfully published: " + fileStreamKeyValue.Key);
        }
    }
}
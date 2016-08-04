using System.IO;
using System.Threading.Tasks;
using ShellRepo.Common;
using ShellRepo.Models;
using Toscana;

namespace ShellRepo.Engine
{
    public interface IShellContentEntityCreator
    {
        Task CreateShellContentEntity(string filename, Stream archiveStream);
    }

    public class ShellContentEntityCreator : IShellContentEntityCreator
    {
        private readonly IShellEntityRepository shellEntityRepository;

        public ShellContentEntityCreator(IShellEntityRepository shellEntityRepository)
        {
            this.shellEntityRepository = shellEntityRepository;
        }

        public async Task CreateShellContentEntity(string filename, Stream archiveStream)
        {
            var content = archiveStream.StreamToByteArray();

            var toscaCloudServiceArchive = ToscaCloudServiceArchive.Load(archiveStream);

            await shellEntityRepository.Add(new ShellContentEntity
            {
                Name = Path.GetFileNameWithoutExtension(filename),
                Version = toscaCloudServiceArchive.ToscaMetadata.CsarVersion,
                CreatedBy = toscaCloudServiceArchive.ToscaMetadata.CreatedBy,
                Description = toscaCloudServiceArchive.EntryPointServiceTemplate.Description,
                Content = content
            });
        }
    }
}
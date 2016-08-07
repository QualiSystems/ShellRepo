using System;
using System.IO;
using System.Threading.Tasks;
using ShellRepo.Common;
using ShellRepo.Models;
using Toscana;

namespace ShellRepo.Engine
{
    public interface IShellUploader
    {
        Task UploadShell(string filename, Stream archiveStream);
    }

    public class ShellUploader : IShellUploader
    {
        private readonly ISearchingService searchingService;
        private readonly IShellContentEntityRepository shellContentEntityRepository;
        private readonly IShellEntityRepository shellEntityRepository;

        public ShellUploader(IShellContentEntityRepository shellContentEntityRepository,
            IShellEntityRepository shellEntityRepository, ISearchingService searchingService)
        {
            this.shellContentEntityRepository = shellContentEntityRepository;
            this.shellEntityRepository = shellEntityRepository;
            this.searchingService = searchingService;
        }

        public async Task UploadShell(string filename, Stream archiveStream)
        {
            var shellName = Path.GetFileNameWithoutExtension(filename);
            var content = archiveStream.StreamToByteArray();

            var toscaCloudServiceArchive = ToscaCloudServiceArchive.Load(archiveStream);

            var shellEntity = shellEntityRepository.Find(shellName);

            if (shellEntity == null)
            {
                shellEntity = new ShellEntity
                {
                    Name = shellName,
                    Version = toscaCloudServiceArchive.ToscaMetadata.CsarVersion,
                    CreatedBy = toscaCloudServiceArchive.ToscaMetadata.CreatedBy,
                    Description = toscaCloudServiceArchive.EntryPointServiceTemplate.Description,
                    LastPublished = DateTime.UtcNow,
                    DownloadCount = 0,
                    LicenseType = "Apache 2.0",
                    LicenseUrl = @"http://www.apache.org/licenses/LICENSE-2.0"
                };
                shellEntityRepository.Insert(shellEntity);
            }
            else
            {
                shellEntity.LastPublished = DateTime.UtcNow;
                shellEntityRepository.Update(shellEntity);
            }

            searchingService.Index(shellEntity);

            await shellContentEntityRepository.Add(new ShellContentEntity
            {
                Name = shellName,
                Version = toscaCloudServiceArchive.ToscaMetadata.CsarVersion,
                CreatedBy = toscaCloudServiceArchive.ToscaMetadata.CreatedBy,
                Description = toscaCloudServiceArchive.EntryPointServiceTemplate.Description,
                Content = content,
                DownloadCount = 0,
                PublishedAt = DateTime.UtcNow,
                ShellEntityId = shellEntity.Id
            });
        }
    }
}
using System.Linq;
using ShellRepo.Models;

namespace ShellRepo.Engine
{
    public interface IShellContentEntityRetriever
    {
        ShellContentDto[] GetShellContentDtos(string shellName);
    }

    public class ShellContentEntityRetriever : IShellContentEntityRetriever
    {
        private readonly IShellContentEntityRepository shellContentEntityRepository;

        public ShellContentEntityRetriever(IShellContentEntityRepository shellContentEntityRepository)
        {
            this.shellContentEntityRepository = shellContentEntityRepository;
        }

        public ShellContentDto[] GetShellContentDtos(string shellName)
        {
            var shellEntities = shellContentEntityRepository.Find(shellName)
                .Select(ConvertToShellEntity)
                .ToArray();
            return shellEntities;
        }

        private static ShellContentDto ConvertToShellEntity(ShellContentEntity shellContentEntity)
        {
            return new ShellContentDto
            {
                CreatedBy = shellContentEntity.CreatedBy,
                Description = shellContentEntity.Description,
                Name = shellContentEntity.Name,
                Version = shellContentEntity.Version,
                DownloadCount = shellContentEntity.DownloadCount,
                PublishedAt = shellContentEntity.PublishedAt
            };
        }

    }
}
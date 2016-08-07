using System;
using System.Linq;
using ShellRepo.Exceptions;
using ShellRepo.Models;

namespace ShellRepo.Engine
{
    public interface IShellEntityContentDownloader
    {
        ShellContentEntity DownloadShellContentEntity(string shellName, Version versionObject);
    }

    public class ShellEntityContentDownloader : IShellEntityContentDownloader
    {
        private readonly IShellContentEntityRepository shellContentEntityRepository;
        private readonly IShellEntityRepository shellEntityRepository;

        public ShellEntityContentDownloader(IShellContentEntityRepository shellContentEntityRepository, IShellEntityRepository shellEntityRepository)
        {
            this.shellContentEntityRepository = shellContentEntityRepository;
            this.shellEntityRepository = shellEntityRepository;
        }

        public ShellContentEntity DownloadShellContentEntity(string shellName, Version versionObject)
        {
            var shellContentEntities = shellContentEntityRepository.Find(shellName, versionObject);
            if (!shellContentEntities.Any())
            {
                throw new ShellNotFoundException(string.Format("Shell not found by name '{0}' and version'{1}'", shellName, versionObject));
            }
            var latestVersion = shellContentEntities.Max(s => s.Version);
            var shellContentEntity = shellContentEntities.Single(c => c.Version == latestVersion);
            shellContentEntityRepository.IncrementDownloadCount(shellContentEntity.Id);
            shellEntityRepository.IncrementDownloadCount(shellContentEntity.ShellEntityId);
            return shellContentEntity;
        }
    }
}
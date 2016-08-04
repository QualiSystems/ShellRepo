using System;
using System.Linq;
using ShellRepo.Exceptions;
using ShellRepo.Models;

namespace ShellRepo.Engine
{
    public interface IShellEntityContentRetriever
    {
        ShellContentEntity GetShellContentEntity(string shellName, Version versionObject);
    }

    public class ShellEntityContentRetriever : IShellEntityContentRetriever
    {
        private readonly IShellEntityRepository shellEntityRepository;

        public ShellEntityContentRetriever(IShellEntityRepository shellEntityRepository)
        {
            this.shellEntityRepository = shellEntityRepository;
        }

        public ShellContentEntity GetShellContentEntity(string shellName, Version versionObject)
        {
            var shellContentEntities = shellEntityRepository.Find(shellName, versionObject);
            if (!shellContentEntities.Any())
            {
                throw new ShellNotFoundException(string.Format("Shell not found by name '{0}' and version'{1}'", shellName, versionObject));
            }
            var latestVersion = shellContentEntities.Max(s => s.Version);
            var shellContentEntity = shellContentEntities.Single(c => c.Version == latestVersion);
            return shellContentEntity;
        }
    }
}
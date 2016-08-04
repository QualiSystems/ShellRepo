using System;
using System.Linq;
using ShellRepo.Models;

namespace ShellRepo.Engine
{
    public interface IShellEntityFinder
    {
        ShellEntity[] FindShellEntities(string shellName, Version version);
    }

    public class ShellEntityFinder : IShellEntityFinder
    {
        private readonly IShellEntityRepository shellEntityRepository;

        public ShellEntityFinder(IShellEntityRepository shellEntityRepository)
        {
            this.shellEntityRepository = shellEntityRepository;
        }

        public ShellEntity[] FindShellEntities(string shellName, Version version)
        {
            var shellEntities = shellEntityRepository.Find(shellName, version)
                .Select(s => new ShellEntity
                {
                    CreatedBy = s.CreatedBy,
                    Description = s.Description,
                    Name = s.Name,
                    Version = s.Version
                })
                .ToArray();
            return shellEntities;
        }
    }
}
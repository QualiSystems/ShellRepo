using System;
using System.Collections.Generic;
using System.Linq;
using ShellRepo.Models;

namespace ShellRepo.Engine
{
    public interface IShellEntityFinder
    {
        ShellEntity[] FindShellEntities(string shellName, Version version);
        List<ShellEntity> GetAll();
    }

    public class ShellEntityFinder : IShellEntityFinder
    {
        private readonly IShellContentEntityRepository shellContentEntityRepository;
        private readonly IShellEntityRepository shellEntityRepository;

        public ShellEntityFinder(IShellContentEntityRepository shellContentEntityRepository,
            IShellEntityRepository shellEntityRepository)
        {
            this.shellContentEntityRepository = shellContentEntityRepository;
            this.shellEntityRepository = shellEntityRepository;
        }

        public ShellEntity[] FindShellEntities(string shellName, Version version)
        {
            var shellEntities = shellContentEntityRepository.Find(shellName, version)
                .Select(ConvertToShellEntity)
                .ToArray();
            return shellEntities;
        }

        public List<ShellEntity> GetAll()
        {
            return shellEntityRepository.GetAll();
        }

        private static ShellEntity ConvertToShellEntity(ShellContentEntity s)
        {
            return new ShellEntity
            {
                CreatedBy = s.CreatedBy,
                Description = s.Description,
                Name = s.Name,
                Version = s.Version
            };
        }
    }
}
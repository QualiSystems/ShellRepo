using System.Collections.Generic;
using ShellRepo.Models;

namespace ShellRepo.Engine
{
    public interface IShellEntityRetriever
    {
        List<ShellEntity> GetShellEntitiesWithPaging(int? pageNumber);
    }

    public class ShellEntityRetriever : IShellEntityRetriever
    {
        private readonly IShellEntityRepository shellEntityRepository;

        public ShellEntityRetriever(IShellEntityRepository shellEntityRepository)
        {
            this.shellEntityRepository = shellEntityRepository;
        }

        public List<ShellEntity> GetShellEntitiesWithPaging(int? pageNumber)
        {
            return shellEntityRepository.GetAll(pageNumber);
        }
    }
}
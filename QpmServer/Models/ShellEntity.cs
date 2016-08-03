using System;

namespace ShellRepo.Models
{
    public class ShellEntity
    {
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public Version Version { get; set; }
        public string Description { get; set; }
    }
}
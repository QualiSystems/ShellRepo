using System;

namespace ShellRepo.Models
{
    public class ShellContentEntity
    {
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public Version Version { get; set; }
        public string Description { get; set; }
        public byte[] Content { get; set; }

    }
}
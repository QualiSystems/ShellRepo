using System;

namespace ShellRepo.Models
{
    public class ShellContentDto
    {
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public Version Version { get; set; }
        public string Description { get; set; }
        public DateTime PublishedAt { get; set; }
        public int DownloadCount { get; set; }
    }
}
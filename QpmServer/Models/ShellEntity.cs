using System;
using MongoDB.Bson;

namespace ShellRepo.Models
{
    public class ShellEntity
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public Version Version { get; set; }
        public string Description { get; set; }
        public int DownloadCount { get; set; }
        public DateTime LastPublished { get; set; }
        public string LicenseType { get; set; }
        public string LicenseUrl { get; set; }
    }
}
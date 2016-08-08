using System;
using MongoDB.Bson;

namespace ShellRepo.Models
{
    public class ShellContentEntity
    {
        public ObjectId Id { get; set; }
        public ObjectId ShellEntityId { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public Version Version { get; set; }
        public string Description { get; set; }
        public byte[] Content { get; set; }
        public DateTime PublishedAt { get; set; }
        public int DownloadCount { get; set; }
    }
}
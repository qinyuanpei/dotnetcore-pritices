using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Serialization;

namespace hello_webapi.Models
{
    [Serializable]
    public class FileEntry
    {
        public string FileId { get; set; }

        public long FileSize { get; set; }

        public string FileName { get; set; }

        public string AssetURL { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
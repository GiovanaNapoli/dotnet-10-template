using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities.Common
{
    public class ImageFile : BaseAuditableEntity
    {
        public required string FileName { get; set; }
        public required string FilePath { get; set; }
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
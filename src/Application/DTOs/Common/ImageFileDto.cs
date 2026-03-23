using System;

namespace Application.DTOs.Common
{
    public class ImageFileDto
    {
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

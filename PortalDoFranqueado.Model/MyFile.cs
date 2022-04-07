using System;

namespace PortalDoFranqueado.Model
{
    public class MyFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string ContentType { get; set; }
        public DateTime CreatedDate { get; set; }
        public long Size { get; set; }
        public string CompressionType { get; set; } = "None";

        public MyFile Clone()
            => new()
            {
                Id = Id,
                Name = Name,
                Extension = Extension,
                ContentType = ContentType,
                CreatedDate = CreatedDate,
                Size = Size,
                CompressionType = CompressionType
            };
    }
}

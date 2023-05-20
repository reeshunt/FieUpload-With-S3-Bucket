using System;
namespace AnimalDrawing.Models
{
    public class S3Config
    {
        public string Accesskey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }
        public string Resources { get; set; }
        public string BucketURL { get; set; }
    }
}


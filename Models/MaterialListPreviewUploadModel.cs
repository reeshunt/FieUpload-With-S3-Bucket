using System.ComponentModel.DataAnnotations;

namespace AnimalDrawing.Models
{
    public class FileUploadModel
    {
        [Required]
        public string? AnimalName { get; set; }
        [Required]
        public IFormFile? MaterialPreviewImageURL { get; set; }
        [Required]
        public IFormFile? ArtBoardImgURL { get; set; }
        [Required]
        public List<IFormFile>? GIFImageList { get; set; }
        [Required]
        public IFormFile? VideoPreviewScreenImageURL { get; set; }
        [Required]
        public IFormFile? VideoPreviewScreenImgURL { get; set; }
    }
}

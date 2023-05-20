using System.Collections.Generic;

namespace AnimalDrawing.Models
{
    public class ResponseDataModel
    {
        public List<MaterialListPreviewScreen> MaterialListPreviewScreen { get; set; }
        public List<GIFScreen> GIFScreen { get; set; }
        public List<VideoPreviewScreen> VideoPreviewScreen { get; set; }
    }
}

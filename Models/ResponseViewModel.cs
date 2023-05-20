using System;
namespace AnimalDrawing.Models
{
    public class ResponseViewModel
    {
        public string status { get; set; }
        public string message { get; set; }
        public dynamic data { get; set; }
    }
}


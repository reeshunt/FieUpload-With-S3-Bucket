using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AnimalDrawing.Controllers
{
    //Controller
    public class UploadController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult success()
        {
            return View();
        }
        public IActionResult deleted()
        {
            return View();
        }
    }
}
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AnimalDrawing.Models;
using System.Linq;
using System.Collections;

namespace AnimalDrawing.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAWSS3BucketHelper _aWSS3BucketHelper;
    public HomeController(ILogger<HomeController> logger, IAWSS3BucketHelper aWSS3BucketHelper)
    {
        _logger = logger;
        _aWSS3BucketHelper = aWSS3BucketHelper;
    }

    [HttpGet]
    public async Task<JsonResult> List()
    {
        List<MaterialListPreviewScreen> materialResponse = await _aWSS3BucketHelper.GetMaterialListPreviewScreen();
        List<GIFScreen> gifResponse = await _aWSS3BucketHelper.GetGIFScreen();
        List<VideoPreviewScreen> videoPreviewScreenResponse = await _aWSS3BucketHelper.GetVideoPreviewScreen();

        return Json(new { MaterialListPreviewScreen = materialResponse,
            GIFScreen = gifResponse,VideoPreviewScreen = videoPreviewScreenResponse});
    }
    [HttpPost]
    public async Task<IActionResult> MaterialListUpload(FileUploadModel data)
    {
        if (!string.IsNullOrEmpty(data.AnimalName) && data.MaterialPreviewImageURL != null && data.ArtBoardImgURL != null)
        {
            var result = await _aWSS3BucketHelper.UploadFileForMaterialList(data.AnimalName,data.MaterialPreviewImageURL,data.ArtBoardImgURL);
        }
        else
        {
            TempData["Error"] = "All fields are mandatory";
        }

        return RedirectToAction("success", "Upload");
    }
    [HttpPost]
    public async Task<IActionResult> GIFUpload(FileUploadModel data)
    {
        if (!string.IsNullOrEmpty(data.AnimalName) && data.GIFImageList!=null)
        {
            if (data.GIFImageList.Count > 0)
            {
                var result = await _aWSS3BucketHelper.UploadFileForGIF(data.AnimalName, data.GIFImageList);
            }
        }
        else
        {
            TempData["Error"] = "All fields are mandatory";
        }

        return RedirectToAction("success", "Upload");
    }

    [HttpPost]
    public async Task<IActionResult> VideoPreviewScreenUpload(FileUploadModel data)
    {
        if (!string.IsNullOrEmpty(data.AnimalName) && data.VideoPreviewScreenImageURL != null && data.VideoPreviewScreenImgURL != null)
        {
            var result = await _aWSS3BucketHelper.UploadFileForVideoPreviewScreen(data.AnimalName, data.VideoPreviewScreenImageURL, data.VideoPreviewScreenImgURL);
        }
        else
        {
            TempData["Error"] = "All fields are mandatory";
        }

        return RedirectToAction("success", "Upload");
    }
    [HttpGet]
    public async Task<IActionResult> DeleteAll()
    {
        await _aWSS3BucketHelper.EmptyS3Bucket();
        return RedirectToAction("deleted", "Upload");
    }

}


using Amazon.S3.Model;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using AnimalDrawing.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AnimalDrawing.Models
{
    public interface IAWSS3BucketHelper
    {
        Task<ResponseViewModel> UploadFile(IFormFile file);
        Task<ResponseViewModel> GetFilesList();
        Task<ResponseViewModel> GetFoldersList();
        Task<ResponseViewModel> GetFile(string key);
        Task<ResponseViewModel> DeleteFile(string key);
        Task EmptyS3Bucket();
        ResponseViewModel GetFileUrl(string key);
        Task<List<string>> GetFolderFiles(string FolderName);
        Task<ResponseViewModel> UploadFileForMaterialList(string animalName, IFormFile previewImage, IFormFile artBoardImage);
        Task<ResponseViewModel> UploadFileForGIF(string animalName, List<IFormFile> artBoardImage);
        Task<ResponseViewModel> UploadFileForVideoPreviewScreen(string animalName, IFormFile previewImage, IFormFile artBoardImage);
        Task<List<MaterialListPreviewScreen>> GetMaterialListPreviewScreen();
        Task<List<GIFScreen>> GetGIFScreen();
        Task<List<VideoPreviewScreen>> GetVideoPreviewScreen();
        string PathToPresignedUrl(string file);



    }
    public class AWSS3BucketHelper : IAWSS3BucketHelper
    {
        private readonly IAmazonS3 _awsS3Client;
        private readonly S3Config _aWSS3Bucket;
        public AWSS3BucketHelper(IOptions<S3Config> aWSS3Bucket, IAmazonS3 awsS3Client)
        {
            _aWSS3Bucket = aWSS3Bucket.Value;
            _awsS3Client = awsS3Client;
        }
        public async Task<ResponseViewModel> UploadFile(IFormFile file)
        {
            var response = new ResponseViewModel();
            try
            {
                using (var newMemoryStream = new MemoryStream())
                {
                    file.CopyTo(newMemoryStream);

                    PutObjectRequest request = new PutObjectRequest()
                    {
                        InputStream = newMemoryStream,
                        BucketName = _aWSS3Bucket.BucketName,
                        Key = file.FileName
                    };
                    PutObjectResponse res = await _awsS3Client.PutObjectAsync(request);
                    if (res.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        response = new ResponseViewModel
                        {
                            status = "Success",
                            message = "Files uploaded successfully.",
                        };
                    }
                    else
                    {
                        response = new ResponseViewModel
                        {
                            status = "Error",
                            message = "Files not uploaded.",
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Access Denied")
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = "Please provide correct aws credentials.",
                    };
                }
                else
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = ex.Message,
                    };
                }
            }

            return response;
        }
        public async Task<ResponseViewModel> GetFilesList()
        {
            var response = new ResponseViewModel();
            try
            {
                var listVersionResponse = await _awsS3Client.ListVersionsAsync(_aWSS3Bucket.BucketName);
                var fileList = listVersionResponse.Versions.Select(c => c.Key).ToList();
                response = new ResponseViewModel
                {
                    status = "Success",
                    message = "Files retrieved successfully.",
                    data = fileList,
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == "Access Denied")
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = "Please provide correct aws credentials.",
                    };
                }
                else
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = ex.Message,
                    };
                }
            }
            return response;
        }
        [HttpGet]
        public async Task<ResponseViewModel> GetFoldersList()
        {
            var response = new ResponseViewModel();
            try
            {
                var s3Apiresponse = await _awsS3Client.ListObjectsAsync(_aWSS3Bucket.BucketName);
                var reponseData = s3Apiresponse.S3Objects.Where(x => x.Key.EndsWith("/")).Select(x => x.Key).ToList();
                response = new ResponseViewModel
                {
                    status = "Success",
                    message = "Files retrieved successfully.",
                    data = reponseData,
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == "Access Denied")
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = "Please provide correct aws credentials.",
                    };
                }
                else
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = ex.Message,
                    };
                }
            }
            return response;
        }
        [HttpGet]
        public async Task<List<MaterialListPreviewScreen>> GetMaterialListPreviewScreen()
        {
            List<MaterialListPreviewScreen> responseList = new List<MaterialListPreviewScreen>();
            try
            {
                var response = await GetFolderFiles("MaterialListPreviewScreen");
                var distinctFolders = response.ToList().Select(x => x.Split("/")[1]).ToList().Distinct();
                foreach (var animalName in distinctFolders)
                {

                    var folderSpecificData = await GetFolderFiles("MaterialListPreviewScreen/" + animalName);
                        MaterialListPreviewScreen mlps = new MaterialListPreviewScreen();
                        var folderDataThumb = await GetFolderFiles("MaterialListPreviewScreen/" + animalName + "/thumbnail").ConfigureAwait(false);
                        var folderDataImage = await GetFolderFiles("MaterialListPreviewScreen/" + animalName + "/Image").ConfigureAwait(false);
                        mlps.AnimalName = animalName;
                        mlps.ArtBoardImgURL = PathToPresignedUrl(folderDataThumb.First());
                        mlps.MaterialPreviewImageURL = PathToPresignedUrl(folderDataImage.First());
                        responseList.Add(mlps);

                }
                return responseList;
            }
            catch (Exception ex)
            {
                return null;
            }
            
        }
        [HttpGet]
        public string PathToPresignedUrl(string file)
        {
            var urlRequest = new GetPreSignedUrlRequest()
            {
                BucketName = _aWSS3Bucket.BucketName,
                Key = file,
                Expires = DateTime.UtcNow.AddMinutes(1440) //24hrs
            };

            return _awsS3Client.GetPreSignedURL(urlRequest);
        }
        [HttpGet]
        public async Task<List<GIFScreen>> GetGIFScreen()
        {
            List<GIFScreen> responseList = new List<GIFScreen>();

            try
            {
                var response = await GetFolderFiles("GIFScreen").ConfigureAwait(false);
                var distinctFolders = response.ToList().Select(x => x.Split("/")[1]).ToList().Distinct();

                foreach (var folderName in distinctFolders)
                {
                    List<AnimalList> animalLists = new List<AnimalList>();

                    var folderSpecificData = await GetFolderFiles("GIFScreen/" + folderName).ConfigureAwait(false);
                    var sortedFolderSpecificData = folderSpecificData.OrderBy(x => int.Parse(x.Split("/")[2].Split(".")[0])).ToList();
                    foreach (var folderData in sortedFolderSpecificData)
                    {
                        AnimalList gifData = new AnimalList();
                        gifData.GIF = PathToPresignedUrl(folderData);
                        animalLists.Add(gifData);
                    }

                    GIFScreen gifScreenData = new GIFScreen();
                    gifScreenData.AnimalName = folderName;
                    gifScreenData.AnimalList = animalLists;
                    responseList.Add(gifScreenData);
                }
                return responseList;
            }
            catch (Exception ex)
            {
                return null;
            }
            
        }

        [HttpGet]
        public async Task<List<VideoPreviewScreen>> GetVideoPreviewScreen()
        {
            List<VideoPreviewScreen> responseList = new List<VideoPreviewScreen>();
            try
            {
                var response = await GetFolderFiles("VideoPreviewScreen");
                var distinctFolders = response.ToList().Select(x => x.Split("/")[1]).ToList().Distinct();
                foreach (var animalName in distinctFolders)
                {

                    var folderSpecificData = await GetFolderFiles("VideoPreviewScreen/" + animalName);
                    foreach (var folderData in folderSpecificData)
                    {
                        VideoPreviewScreen vps = new VideoPreviewScreen();
                        var folderDataThumb = await GetFolderFiles("VideoPreviewScreen/" + animalName + "/Thumbnail").ConfigureAwait(false);
                        var folderDataVideo = await GetFolderFiles("VideoPreviewScreen/" + animalName + "/Video").ConfigureAwait(false);
                        vps.AnimalName = animalName;
                        vps.PreviewImageURL = PathToPresignedUrl(folderDataThumb.First());
                        vps.YTB_VideoURL = PathToPresignedUrl(folderDataVideo.First());

                        responseList.Add(vps);
                    }

                }
                return responseList;
            }
            catch (Exception ex)
            {
                return null;
            }
            
        }
        [HttpGet]
        public async Task<List<string>> GetFolderFiles(string FolderName)
        {
            List<string> reponseData = new List<string>();
            var response = new ResponseViewModel();
            try
            {
                ListObjectsV2Request request = new ListObjectsV2Request();
                request.BucketName = _aWSS3Bucket.BucketName; //Amazon Bucket Name
                request.Prefix = FolderName; //Amazon S3 Folder path
                request.StartAfter = FolderName;

                ListObjectsV2Response s3Apiresponse = await _awsS3Client.ListObjectsV2Async(request).ConfigureAwait(false);//_client - AmazonS3Client
                reponseData = s3Apiresponse.S3Objects.Select(x => x.Key).ToList()
                    .Where(x=>!x.Equals(FolderName+"/")) //excluding the self folder
                    .Where(x=>!x.EndsWith("/")).ToList(); //excluding other folders
            }
            catch (Exception ex)
            {
                if (ex.Message == "Access Denied")
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = "Please provide correct aws credentials.",
                    };
                }
                else
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = ex.Message,
                    };
                }
            }
            return reponseData;
        }
        public async Task<ResponseViewModel> GetFile(string key)
        {
            var response = new ResponseViewModel();
            try
            {
                GetObjectResponse res = await _awsS3Client.GetObjectAsync(_aWSS3Bucket.BucketName, key);


                if (res.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    response = new ResponseViewModel
                    {
                        status = "Success",
                        message = "File retrieved successfully.",
                        data = res.ResponseStream,
                    };
                }
                else
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = "Files not found.",
                    };
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Access Denied")
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = "Please provide correct aws credentials.",
                    };
                }
                else
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = ex.Message,
                    };
                }
            }

            return response;
        }

        
        public ResponseViewModel GetFileUrl(string key)
        {
            var response = new ResponseViewModel();
            try
            {
                var expiryUrlRequest = new GetPreSignedUrlRequest
                {
                    BucketName = _aWSS3Bucket.BucketName,
                    Key = key,
                    Expires = DateTime.Now.AddDays(7)
                };

                var fileUrl = _awsS3Client.GetPreSignedURL(expiryUrlRequest);

                if (!string.IsNullOrEmpty(fileUrl))
                {
                    response = new ResponseViewModel
                    {
                        status = "Success",
                        message = "File url generated successfully.",
                        data = fileUrl,
                    };
                }
                else
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = "Files not found.",
                    };
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Access Denied")
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = "Please provide correct aws credentials.",
                    };
                }
                else
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = ex.Message,
                    };
                }
            }

            return response;
        }

        public async Task<ResponseViewModel> UploadFileForMaterialList(string animalName, IFormFile previewImage, IFormFile artBoardImage)
        {
            var response = new ResponseViewModel();
            try
            {
                string fullFolderPathForThumb = string.Concat(_aWSS3Bucket.BucketName, "/", "MaterialListPreviewScreen/",animalName, "/thumbnail");
                using (var newMemoryStream = new MemoryStream())
                {
                    previewImage.CopyTo(newMemoryStream);

                    PutObjectRequest previewImageuploadReq = new PutObjectRequest()
                    {
                        InputStream = newMemoryStream,
                        BucketName = fullFolderPathForThumb,
                        Key = previewImage.FileName,
                    };
                    PutObjectResponse previewImageuploadRes = await _awsS3Client.PutObjectAsync(previewImageuploadReq);
                }

                using (var newMemoryStream2 = new MemoryStream())
                {
                    string fullFolderPathForImage = string.Concat(_aWSS3Bucket.BucketName, "/", "MaterialListPreviewScreen/", animalName, "/Image");
                    artBoardImage.CopyTo(newMemoryStream2);

                    PutObjectRequest ArtBoardImgageUploadReq = new PutObjectRequest()
                    {
                        InputStream = newMemoryStream2,
                        BucketName = fullFolderPathForImage,
                        Key = artBoardImage.FileName,
                    };
                    PutObjectResponse ArtBoardImgageUploadRes = await _awsS3Client.PutObjectAsync(ArtBoardImgageUploadReq);
                }
                //if (previewImageuploadRes.HttpStatusCode == System.Net.HttpStatusCode.OK)
                //{
                //    response = new ResponseViewModel
                //    {
                //        status = "Success",
                //        message = "Files uploaded successfully.",
                //    };
                //}
                //else
                //{
                //    response = new ResponseViewModel
                //    {
                //        status = "Error",
                //        message = "Files not uploaded.",
                //    };
                //}
            }
            catch (Exception ex)
            {
                if (ex.Message == "Access Denied")
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = "Please provide correct aws credentials.",
                    };
                }
                else
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = ex.Message,
                    };
                }
            }

            return response;
        }
        public async Task<ResponseViewModel> UploadFileForVideoPreviewScreen(string animalName, IFormFile previewImage, IFormFile artBoardImage)
        {
            var response = new ResponseViewModel();
            try
            {
                string fullFolderPathForThumb = string.Concat(_aWSS3Bucket.BucketName, "/", "VideoPreviewScreen/", animalName, "/Thumbnail");
                using (var newMemoryStream = new MemoryStream())
                {
                    previewImage.CopyTo(newMemoryStream);

                    PutObjectRequest previewImageuploadReq = new PutObjectRequest()
                    {
                        InputStream = newMemoryStream,
                        BucketName = fullFolderPathForThumb,
                        Key = previewImage.FileName,
                    };
                    PutObjectResponse previewImageuploadRes = await _awsS3Client.PutObjectAsync(previewImageuploadReq);
                }

                using (var newMemoryStream2 = new MemoryStream())
                {
                    string fullFolderPathForImage = string.Concat(_aWSS3Bucket.BucketName, "/", "VideoPreviewScreen/", animalName, "/Video");
                    artBoardImage.CopyTo(newMemoryStream2);

                    PutObjectRequest ArtBoardImgageUploadReq = new PutObjectRequest()
                    {
                        InputStream = newMemoryStream2,
                        BucketName = fullFolderPathForImage,
                        Key = artBoardImage.FileName,
                    };
                    PutObjectResponse ArtBoardImgageUploadRes = await _awsS3Client.PutObjectAsync(ArtBoardImgageUploadReq);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Access Denied")
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = "Please provide correct aws credentials.",
                    };
                }
                else
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = ex.Message,
                    };
                }
            }

            return response;
        }
        public async Task<ResponseViewModel> UploadFileForGIF(string animalName, List<IFormFile> artBoardImage)
        {
            var response = new ResponseViewModel();
            try
            {
                int i = 0;
                foreach (var fileObject in artBoardImage)
                {
                    i += 1;
                    string fullFolderPathForGIF = string.Concat(_aWSS3Bucket.BucketName, "/", "GIFScreen/", animalName);
                    using (var newMemoryStreamGIF = new MemoryStream())
                    {
                        fileObject.CopyTo(newMemoryStreamGIF);
                        PutObjectRequest previewImageuploadReq = new PutObjectRequest()
                        {
                            InputStream = newMemoryStreamGIF,
                            BucketName = fullFolderPathForGIF,
                            Key = string.Concat((i).ToString(),".png"),
                        };
                        PutObjectResponse previewImageuploadRes = await _awsS3Client.PutObjectAsync(previewImageuploadReq);
                    }
                }




                
            }
            catch (Exception ex)
            {
                if (ex.Message == "Access Denied")
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = "Please provide correct aws credentials.",
                    };
                }
                else
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = ex.Message,
                    };
                }
            }

            return response;
        }
        public async Task<ResponseViewModel> DeleteFile(string key)
        {
            var response = new ResponseViewModel();
            try
            {
                DeleteObjectResponse res = await _awsS3Client.DeleteObjectAsync(_aWSS3Bucket.BucketName, key);
                if (res.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    response = new ResponseViewModel
                    {
                        status = "Success",
                        message = "File deleted successfully.",
                    };
                }
                else
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = "Files not found.",
                    };
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Access Denied")
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = "Please provide correct aws credentials.",
                    };
                }
                else
                {
                    response = new ResponseViewModel
                    {
                        status = "Error",
                        message = ex.Message,
                    };
                }
            }

            return response;
        }

        public async Task EmptyS3Bucket()
        {
            try
            {
                ListObjectsV2Response listResponse;
                do
                {
                    listResponse = await _awsS3Client.ListObjectsV2Async(new ListObjectsV2Request
                    {
                        BucketName = _aWSS3Bucket.BucketName
                    });

                    var deleteRequest = new DeleteObjectsRequest
                    {
                        BucketName = _aWSS3Bucket.BucketName,
                        Objects = listResponse.S3Objects.Select(obj => new KeyVersion { Key = obj.Key }).ToList()
                    };

                    await _awsS3Client.DeleteObjectsAsync(deleteRequest);
                } while (listResponse.IsTruncated);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }
    }
}

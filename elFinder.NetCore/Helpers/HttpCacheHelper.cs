//using System;
//using System.Threading.Tasks;
//using elFinder.NetCore.Drivers;
//using Microsoft.AspNetCore.Http;

//namespace elFinder.NetCore.Helpers
//{
//    internal static class HttpCacheHelper
//    {
//        public static async Task<bool> IsFileFromCacheAsync(IFile file, HttpRequest request, HttpResponse response)
//        {
//            DateTime updated = await file.LastWriteTimeUtcAsync;
//            string filename = file.Name;

//            if (!DateTime.TryParse(request.Headers["If-Modified-Since"], out DateTime modifyDate))
//            {
//                modifyDate = DateTime.UtcNow;
//            }

//            string eTag = await GetFileETag(file);
//            if (!IsFileModified(updated, eTag, request))
//            {
//                response.StatusCode = (int)System.Net.HttpStatusCode.NotModified;
//                response.Headers.Add("Content-Length", "0");
//                //response.Cache.SetCacheability(HttpCacheability.Public);
//                //response.Cache.SetLastModified(updated);
//                response.Headers.Add("ETag", new[] { eTag });
//                return true;
//            }
//            else
//            {
//                //response.Cache.SetAllowResponseInBrowserHistory(true);
//                //response.Cache.SetCacheability(HttpCacheability.Public);
//                //response.Cache.SetLastModified(updated);
//                response.Headers.Add("ETag", new[] { eTag });
//                return false;
//            }
//        }

//        private static async Task<string> GetFileETag(IFile file)
//        {
//            string md5 = await file.GetFileMd5Async();
//            return $"\"{md5}\"";
//        }

//        private static bool IsFileModified(DateTime modifyDate, string eTag, HttpRequest request)
//        {
//            bool fileDateModified = true;

//            //Check If-Modified-Since request header, if it exists
//            if (!string.IsNullOrEmpty(request.Headers["If-Modified-Since"]) && DateTime.TryParse(request.Headers["If-Modified-Since"], out DateTime modifiedSince))
//            {
//                fileDateModified = false;
//                if (modifyDate > modifiedSince)
//                {
//                    TimeSpan modifyDiff = modifyDate - modifiedSince;
//                    //ignore time difference of up to one seconds to compensate for date encoding
//                    fileDateModified = modifyDiff > TimeSpan.FromSeconds(1);
//                }
//            }

//            //check the If-None-Match header, if it exists, this header is used by FireFox to validate entities based on the etag response header
//            bool eTagChanged = false;
//            if (!string.IsNullOrEmpty(request.Headers["If-None-Match"]))
//            {
//                eTagChanged = request.Headers["If-None-Match"] != eTag;
//            }
//            return (eTagChanged || fileDateModified);
//        }
//    }
//}
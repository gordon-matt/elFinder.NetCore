using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace elFinder.NetCore.Helpers
{
    internal static class HttpCacheHelper
    {
        // TODO: Needs testing due to porting over to .NET Core
        public static bool IsFileFromCache(FileInfo info, HttpRequest request, HttpResponse response)
        {
            DateTime updated = info.LastWriteTimeUtc;
            string filename = info.Name;
            DateTime modifyDate;

            if (!DateTime.TryParse(request.Headers["If-Modified-Since"], out modifyDate))
            {
                modifyDate = DateTime.UtcNow;
            }

            string eTag = GetFileETag(filename, updated);
            if (!IsFileModified(updated, eTag, request))
            {
                response.StatusCode = (int)System.Net.HttpStatusCode.NotModified;
                response.Headers.Add("Content-Length", "0");
                //response.Cache.SetCacheability(HttpCacheability.Public);
                //response.Cache.SetLastModified(updated);
                response.Headers.Add("ETag", new[] { eTag });
                return true;
            }
            else
            {
                //response.Cache.SetAllowResponseInBrowserHistory(true);
                //response.Cache.SetCacheability(HttpCacheability.Public);
                //response.Cache.SetLastModified(updated);
                response.Headers.Add("ETag", new[] { eTag });
                return false;
            }
        }

        private static string GetFileETag(string fileName, DateTime modified)
        {
            return "\"" + Utils.GetFileMd5(fileName, modified) + "\"";
        }

        private static bool IsFileModified(DateTime modifyDate, string eTag, HttpRequest request)
        {
            DateTime modifiedSince;
            bool fileDateModified = true;

            //Check If-Modified-Since request header, if it exists
            if (!string.IsNullOrEmpty(request.Headers["If-Modified-Since"]) && DateTime.TryParse(request.Headers["If-Modified-Since"], out modifiedSince))
            {
                fileDateModified = false;
                if (modifyDate > modifiedSince)
                {
                    TimeSpan modifyDiff = modifyDate - modifiedSince;
                    //ignore time difference of up to one seconds to compensate for date encoding
                    fileDateModified = modifyDiff > TimeSpan.FromSeconds(1);
                }
            }

            //check the If-None-Match header, if it exists, this header is used by FireFox to validate entities based on the etag response header
            bool eTagChanged = false;
            if (!string.IsNullOrEmpty(request.Headers["If-None-Match"]))
            {
                eTagChanged = request.Headers["If-None-Match"] != eTag;
            }
            return (eTagChanged || fileDateModified);
        }
    }
}
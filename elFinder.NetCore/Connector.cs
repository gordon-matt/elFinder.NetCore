using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using elFinder.NetCore.Drawing;
using elFinder.NetCore.Extensions;
using elFinder.NetCore.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore
{
    /// <summary>
    /// Represents a connector which processes elFinder requests
    /// </summary>
    public class Connector
    {
        private IDriver driver;

        public Connector(IDriver driver)
        {
            this.driver = driver;
        }

        public async Task<IActionResult> Process(HttpRequest request)
        {
            IDictionary<string, string> parameters = request.Query.Count > 0
                ? request.Query.ToDictionary(k => k.Key, v => string.Join(";", v.Value))
                : request.Form.ToDictionary(k => k.Key, v => string.Join(";", v.Value));

            string cmd = parameters["cmd"];
            if (string.IsNullOrEmpty(cmd))
            {
                return Error.CommandNotFound();
            }

            string target = parameters.GetValueOrDefault("target");
            if (target != null && target.ToLower() == "null")
            {
                target = null;
            }

            switch (cmd)
            {
                case "open":
                    if (parameters.GetValueOrDefault("init") == "1")
                    {
                        return await driver.InitAsync(target);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(target))
                        {
                            return Error.MissedParameter(cmd);
                        }
                        return await driver.OpenAsync(target, parameters.GetValueOrDefault("tree") == "1");
                    }
                case "file":
                    if (string.IsNullOrEmpty(target))
                    {
                        return Error.MissedParameter(cmd);
                    }
                    return await driver.FileAsync(target, parameters.GetValueOrDefault("download") == "1");

                case "tree":
                    if (string.IsNullOrEmpty(target))
                    {
                        return Error.MissedParameter(cmd);
                    }
                    return await driver.TreeAsync(target);

                case "parents":
                    if (string.IsNullOrEmpty(target))
                    {
                        return Error.MissedParameter(cmd);
                    }
                    return await driver.ParentsAsync(target);

                case "mkdir":
                    {
                        if (string.IsNullOrEmpty(target))
                        {
                            return Error.MissedParameter(cmd);
                        }

                        string name = parameters.GetValueOrDefault("name");
                        if (string.IsNullOrEmpty(name))
                        {
                            return Error.MissedParameter("name");
                        }

                        return await driver.MakeDirAsync(target, name);
                    }
                case "mkfile":
                    {
                        if (string.IsNullOrEmpty(target))
                        {
                            return Error.MissedParameter(cmd);
                        }

                        string name = parameters.GetValueOrDefault("name");
                        if (string.IsNullOrEmpty(name))
                        {
                            return Error.MissedParameter("name");
                        }

                        return await driver.MakeFileAsync(target, name);
                    }
                case "rename":
                    {
                        if (string.IsNullOrEmpty(target))
                        {
                            return Error.MissedParameter(cmd);
                        }

                        string name = parameters.GetValueOrDefault("name");
                        if (string.IsNullOrEmpty(name))
                        {
                            return Error.MissedParameter("name");
                        }

                        return await driver.RenameAsync(target, name);
                    }
                case "rm":
                    {
                        IEnumerable<string> targets = GetTargetsArray(request);
                        if (targets == null)
                        {
                            return Error.MissedParameter("targets");
                        }
                        return await driver.RemoveAsync(targets);
                    }
                case "ls":
                    if (string.IsNullOrEmpty(target))
                    {
                        return Error.MissedParameter(cmd);
                    }
                    return await driver.ListAsync(target);

                case "get":
                    if (string.IsNullOrEmpty(target))
                    {
                        return Error.MissedParameter(cmd);
                    }
                    return await driver.GetAsync(target);

                case "put":
                    if (string.IsNullOrEmpty(target))
                    {
                        return Error.MissedParameter(cmd);
                    }

                    string content = parameters.GetValueOrDefault("content");
                    if (string.IsNullOrEmpty(target))
                    {
                        return Error.MissedParameter("content");
                    }

                    return await driver.PutAsync(target, content);

                case "paste":
                    {
                        IEnumerable<string> targets = GetTargetsArray(request);
                        if (targets == null)
                        {
                            Error.MissedParameter("targets");
                        }

                        string src = parameters.GetValueOrDefault("src");
                        if (string.IsNullOrEmpty(src))
                        {
                            return Error.MissedParameter("src");
                        }

                        string dst = parameters.GetValueOrDefault("dst");
                        if (string.IsNullOrEmpty(src))
                        {
                            return Error.MissedParameter("dst");
                        }

                        return await driver.PasteAsync(src, dst, targets, parameters.GetValueOrDefault("cut") == "1");
                    }
                case "upload":
                    if (string.IsNullOrEmpty(target))
                    {
                        return Error.MissedParameter(cmd);
                    }
                    return await driver.UploadAsync(target, request.Form.Files);

                case "duplicate":
                    {
                        IEnumerable<string> targets = GetTargetsArray(request);
                        if (targets == null)
                        {
                            Error.MissedParameter("targets");
                        }
                        return await driver.DuplicateAsync(targets);
                    }
                case "tmb":
                    {
                        IEnumerable<string> targets = GetTargetsArray(request);
                        if (targets == null)
                        {
                            Error.MissedParameter("targets");
                        }
                        return await driver.ThumbsAsync(targets);
                    }
                case "dim":
                    {
                        if (string.IsNullOrEmpty(target))
                        {
                            return Error.MissedParameter(cmd);
                        }
                        return await driver.DimAsync(target);
                    }
                case "resize":
                    {
                        if (string.IsNullOrEmpty(target))
                        {
                            return Error.MissedParameter(cmd);
                        }
                        switch (parameters.GetValueOrDefault("mode"))
                        {
                            case "resize":
                                {
                                    return await driver.ResizeAsync(
                                        target,
                                        int.Parse(parameters.GetValueOrDefault("width")),
                                        int.Parse(parameters.GetValueOrDefault("height")));
                                }
                            case "crop":
                                {
                                    return await driver.CropAsync(
                                        target,
                                        int.Parse(parameters.GetValueOrDefault("x")),
                                        int.Parse(parameters.GetValueOrDefault("y")),
                                        int.Parse(parameters.GetValueOrDefault("width")),
                                        int.Parse(parameters.GetValueOrDefault("height")));
                                }
                            case "rotate":
                                {
                                    return await driver.RotateAsync(
                                        target,
                                        int.Parse(parameters.GetValueOrDefault("degree")));
                                }
                            default: break;
                        }
                        return Error.CommandNotFound();
                    }
                default: return Error.CommandNotFound();
            }
        }

        /// <summary>
        /// Get actual filesystem path by hash
        /// </summary>
        /// <param name="hash">Hash of file or directory</param>
        public FileSystemInfo GetFileByHash(string hash)
        {
            var path = driver.ParsePath(hash);
            return !path.IsDirectory ? (FileSystemInfo)path.File : (FileSystemInfo)path.Directory;
        }

        public IActionResult GetThumbnail(HttpRequest request, HttpResponse response, string hash)
        {
            if (hash != null)
            {
                var path = driver.ParsePath(hash);
                if (!path.IsDirectory && path.Root.CanCreateThumbnail(path.File))
                {
                    if (!HttpCacheHelper.IsFileFromCache(path.File, request, response))
                    {
                        var thumb = path.Root.GenerateThumbnail(path);
                        return new FileStreamResult(thumb.ImageStream, thumb.MimeType);
                    }
                    else
                    {
                        response.ContentType = Utils.GetMimeType(path.Root.PicturesEditor.ConvertThumbnailExtension(path.File.Extension));
                        //response.End();
                    }
                }
            }
            return new EmptyResult();
        }

        private IEnumerable<string> GetTargetsArray(HttpRequest request)
        {
            IEnumerable<string> targets = null;
            // At the moment, request.Form is throwing an InvalidOperationException...
            //if (request.Form.ContainsKey("targets"))
            //{
            //    targets = request.Form["targets"];
            //}

            IDictionary<string, string> parameters = request.Query.Count > 0
                ? request.Query.ToDictionary(k => k.Key, v => string.Join(";", v.Value))
                : request.Form.ToDictionary(k => k.Key, v => string.Join(";", v.Value));

            if (targets == null)
            {
                string t = parameters.GetValueOrDefault("targets[]");
                if (string.IsNullOrEmpty(t))
                {
                    t = parameters.GetValueOrDefault("targets");
                }
                if (string.IsNullOrEmpty(t))
                {
                    return null;
                }
                targets = t.Split(';'); // 2018.02.23: Bug Fix Issue #3
            }
            return targets;
        }
    }
}
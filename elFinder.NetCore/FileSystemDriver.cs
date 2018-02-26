using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using elFinder.NetCore.Helpers;
using elFinder.NetCore.Models;
using elFinder.NetCore.Models.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore
{
    /// <summary>
    /// Represents a driver for local file system
    /// </summary>
    public class FileSystemDriver : IDriver
    {
        #region Private

        private const string _volumePrefix = "v";
        private List<Root> _roots;

        private Task<JsonResult> Json(object data)
        {
            return Task.FromResult(new JsonResult(data) { ContentType = "text/html" });
        }

        private void DirectoryCopy(DirectoryInfo sourceDir, string destDirName, bool copySubDirs)
        {
            DirectoryInfo[] dirs = sourceDir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!sourceDir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDir.FullName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the file contents of the directory to copy.
            FileInfo[] files = sourceDir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir, temppath, copySubDirs);
                }
            }
        }

        private void RemoveThumbs(FullPath path)
        {
            if (path.Directory != null)
            {
                string thumbPath = path.Root.GetExistingThumbPath(path.Directory);
                if (thumbPath != null)
                {
                    Directory.Delete(thumbPath, true);
                }
            }
            else
            {
                string thumbPath = path.Root.GetExistingThumbPath(path.File);
                if (thumbPath != null)
                {
                    System.IO.File.Delete(thumbPath);
                }
            }
        }

        #endregion Private

        #region Public

        public FullPath ParsePath(string target)
        {
            string volumePrefix = null;
            string pathHash = null;
            for (int i = 0; i < target.Length; i++)
            {
                if (target[i] == '_')
                {
                    pathHash = target.Substring(i + 1);
                    volumePrefix = target.Substring(0, i + 1);
                    break;
                }
            }
            var root = _roots.First(r => r.VolumeId == volumePrefix);
            string path = Utils.DecodePath(pathHash);
            string dirUrl = path != root.Directory.Name ? path : string.Empty;
            var dir = new DirectoryInfo(root.Directory.FullName + dirUrl);

            if (dir.Exists)
            {
                return new FullPath(root, dir);
            }
            else
            {
                var file = new FileInfo(root.Directory.FullName + dirUrl);
                return new FullPath(root, file);
            }
        }

        /// <summary>
        /// Initialize new instance of class ElFinder.FileSystemDriver
        /// </summary>
        public FileSystemDriver()
        {
            _roots = new List<Root>();
        }

        /// <summary>
        /// Adds an object to the end of the roots.
        /// </summary>
        /// <param name="item"></param>
        public void AddRoot(Root item)
        {
            _roots.Add(item);
            item.VolumeId = _volumePrefix + _roots.Count + "_";
        }

        /// <summary>
        /// Gets collection of roots
        /// </summary>
        public IEnumerable<Root> Roots => _roots;

        #endregion Public

        #region IDriver

        public async Task<JsonResult> OpenAsync(string target, bool tree)
        {
            var fullPath = ParsePath(target);
            var response = new OpenResponse(BaseModel.Create(fullPath.Directory, fullPath.Root), fullPath);
            foreach (var item in fullPath.Directory.GetFiles())
            {
                if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    response.Files.Add(BaseModel.Create(item, fullPath.Root));
                }
            }
            foreach (var item in fullPath.Directory.GetDirectories())
            {
                if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    response.Files.Add(BaseModel.Create(item, fullPath.Root));
                }
            }
            return await Json(response);
        }

        public async Task<JsonResult> InitAsync(string target)
        {
            FullPath fullPath;
            if (string.IsNullOrEmpty(target))
            {
                var root = _roots.FirstOrDefault(r => r.StartPath != null);
                if (root == null)
                {
                    root = _roots.First();
                }

                fullPath = new FullPath(root, root.StartPath ?? root.Directory);
            }
            else
            {
                fullPath = ParsePath(target);
            }
            var response = new InitResponseModel(BaseModel.Create(fullPath.Directory, fullPath.Root), new Options(fullPath));

            foreach (var item in fullPath.Directory.GetFiles())
            {
                if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    response.Files.Add(BaseModel.Create(item, fullPath.Root));
                }
            }
            foreach (var item in fullPath.Directory.GetDirectories())
            {
                if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    response.Files.Add(BaseModel.Create(item, fullPath.Root));
                }
            }
            foreach (var item in _roots)
            {
                response.Files.Add(BaseModel.Create(item.Directory, item));
            }
            if (fullPath.Root.Directory.FullName != fullPath.Directory.FullName)
            {
                foreach (var item in fullPath.Root.Directory.GetDirectories())
                {
                    if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    {
                        response.Files.Add(BaseModel.Create(item, fullPath.Root));
                    }
                }
            }
            if (fullPath.Root.MaxUploadSize.HasValue)
            {
                response.UploadMaxSize = fullPath.Root.MaxUploadSizeInKb.Value + "K";
            }
            return await Json(response);
        }

        public async Task<IActionResult> FileAsync(string target, bool download)
        {
            IActionResult result;

            var fullPath = ParsePath(target);
            if (fullPath.IsDirectory)
            {
                result = new ForbidResult();
            }
            if (!fullPath.File.Exists)
            {
                result = new NotFoundResult();
            }
            if (fullPath.Root.IsShowOnly)
            {
                result = new ForbidResult();
            }
            //result = new DownloadFileResult(fullPath.File, download);
            string contentType = download ? "application/octet-stream" : Utils.GetMimeType(fullPath.File);
            result = new PhysicalFileResult(fullPath.File.FullName, contentType);

            return await Task.FromResult(result);
        }

        public async Task<JsonResult> ParentsAsync(string target)
        {
            var fullPath = ParsePath(target);
            var response = new TreeResponseModel();
            if (fullPath.Directory.FullName == fullPath.Root.Directory.FullName)
            {
                response.Tree.Add(BaseModel.Create(fullPath.Directory, fullPath.Root));
            }
            else
            {
                var parent = fullPath.Directory;
                foreach (var item in parent.Parent.GetDirectories())
                {
                    response.Tree.Add(BaseModel.Create(item, fullPath.Root));
                }
                while (parent.FullName != fullPath.Root.Directory.FullName)
                {
                    parent = parent.Parent;
                    response.Tree.Add(BaseModel.Create(parent, fullPath.Root));
                }
            }
            return await Json(response);
        }

        public async Task<JsonResult> TreeAsync(string target)
        {
            var fullPath = ParsePath(target);
            var response = new TreeResponseModel();
            foreach (var item in fullPath.Directory.GetDirectories())
            {
                if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    response.Tree.Add(BaseModel.Create(item, fullPath.Root));
                }
            }
            return await Json(response);
        }

        public async Task<JsonResult> ListAsync(string target)
        {
            var fullPath = ParsePath(target);
            var response = new ListResponseModel();
            foreach (var item in fullPath.Directory.GetFileSystemInfos())
            {
                response.List.Add(item.Name);
            }
            return await Json(response);
        }

        public async Task<JsonResult> MakeDirAsync(string target, string name)
        {
            var fullPath = ParsePath(target);
            var newDir = Directory.CreateDirectory(Path.Combine(fullPath.Directory.FullName, name));
            return await Json(new AddResponseModel(newDir, fullPath.Root));
        }

        public async Task<JsonResult> MakeFileAsync(string target, string name)
        {
            var fullPath = ParsePath(target);
            var newFile = new FileInfo(Path.Combine(fullPath.Directory.FullName, name));
            //newFile.Create().Close();
            newFile.Create();
            return await Json(new AddResponseModel(newFile, fullPath.Root));
        }

        public async Task<JsonResult> RenameAsync(string target, string name)
        {
            var fullPath = ParsePath(target);
            var response = new ReplaceResponseModel();
            response.Removed.Add(target);
            RemoveThumbs(fullPath);
            if (fullPath.Directory != null)
            {
                string newPath = Path.Combine(fullPath.Directory.Parent.FullName, name);
                System.IO.Directory.Move(fullPath.Directory.FullName, newPath);
                response.Added.Add(BaseModel.Create(new DirectoryInfo(newPath), fullPath.Root));
            }
            else
            {
                string newPath = Path.Combine(fullPath.File.DirectoryName, name);
                System.IO.File.Move(fullPath.File.FullName, newPath);
                response.Added.Add(BaseModel.Create(new FileInfo(newPath), fullPath.Root));
            }
            return await Json(response);
        }

        public async Task<JsonResult> RemoveAsync(IEnumerable<string> targets)
        {
            var response = new RemoveResponseModel();
            foreach (string item in targets)
            {
                var fullPath = ParsePath(item);
                RemoveThumbs(fullPath);
                if (fullPath.Directory != null)
                {
                    Directory.Delete(fullPath.Directory.FullName, true);
                }
                else
                {
                    System.IO.File.Delete(fullPath.File.FullName);
                }
                response.Removed.Add(item);
            }
            return await Json(response);
        }

        public async Task<JsonResult> GetAsync(string target)
        {
            var fullPath = ParsePath(target);
            var response = new GetResponseModel();
            using (var reader = new StreamReader(fullPath.File.OpenRead()))
            {
                response.Content = reader.ReadToEnd();
            }
            return await Json(response);
        }

        public async Task<JsonResult> PutAsync(string target, string content)
        {
            var fullPath = ParsePath(target);
            var response = new ChangedResponseModel();
            using (var fileStream = new FileStream(fullPath.File.FullName, FileMode.Create))
            using (var writer = new StreamWriter(fileStream))
            {
                writer.Write(content);
            }
            response.Changed.Add((FileModel)BaseModel.Create(fullPath.File, fullPath.Root));
            return await Json(response);
        }

        public async Task<JsonResult> PasteAsync(string source, string dest, IEnumerable<string> targets, bool isCut)
        {
            var destPath = ParsePath(dest);
            var response = new ReplaceResponseModel();
            foreach (var item in targets)
            {
                var src = ParsePath(item);
                if (src.Directory != null)
                {
                    var newDir = new DirectoryInfo(Path.Combine(destPath.Directory.FullName, src.Directory.Name));
                    if (newDir.Exists)
                    {
                        Directory.Delete(newDir.FullName, true);
                    }
                    if (isCut)
                    {
                        RemoveThumbs(src);
                        src.Directory.MoveTo(newDir.FullName);
                        response.Removed.Add(item);
                    }
                    else
                    {
                        DirectoryCopy(src.Directory, newDir.FullName, true);
                    }
                    response.Added.Add(BaseModel.Create(newDir, destPath.Root));
                }
                else
                {
                    string newFilePath = Path.Combine(destPath.Directory.FullName, src.File.Name);
                    if (System.IO.File.Exists(newFilePath))
                        System.IO.File.Delete(newFilePath);
                    if (isCut)
                    {
                        RemoveThumbs(src);
                        src.File.MoveTo(newFilePath);
                        response.Removed.Add(item);
                    }
                    else
                    {
                        System.IO.File.Copy(src.File.FullName, newFilePath);
                    }
                    response.Added.Add(BaseModel.Create(new FileInfo(newFilePath), destPath.Root));
                }
            }
            return await Json(response);
        }

        public async Task<JsonResult> UploadAsync(string target, IEnumerable<IFormFile> targets)
        {
            int fileCount = targets.Count();

            var dest = ParsePath(target);
            var response = new AddResponseModel();
            if (dest.Root.MaxUploadSize.HasValue)
            {
                for (int i = 0; i < fileCount; i++)
                {
                    IFormFile file = targets.ElementAt(i);
                    if (file.Length > dest.Root.MaxUploadSize.Value)
                    {
                        return Error.MaxUploadFileSize();
                    }
                }
            }
            for (int i = 0; i < fileCount; i++)
            {
                IFormFile file = targets.ElementAt(i);
                var path = new FileInfo(Path.Combine(dest.Directory.FullName, Path.GetFileName(file.FileName)));

                if (path.Exists)
                {
                    if (dest.Root.UploadOverwrite)
                    {
                        //if file already exist we rename the current file,
                        //and if upload is succesfully delete temp file, in otherwise we restore old file
                        string tmpPath = path.FullName + Guid.NewGuid();
                        bool uploaded = false;
                        try
                        {
                            using (var fileStream = new FileStream(tmpPath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            uploaded = true;
                        }
                        catch { }
                        finally
                        {
                            if (uploaded)
                            {
                                System.IO.File.Delete(path.FullName);
                                System.IO.File.Move(tmpPath, path.FullName);
                            }
                            else
                            {
                                System.IO.File.Delete(tmpPath);
                            }
                        }
                    }
                    else
                    {
                        using (var fileStream = new FileStream(Path.Combine(path.DirectoryName, Utils.GetDuplicatedName(path)), FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                    }
                }
                else
                {
                    using (var fileStream = new FileStream(path.FullName, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
                response.Added.Add((FileModel)BaseModel.Create(new FileInfo(path.FullName), dest.Root));
            }
            return await Json(response);
        }

        public async Task<JsonResult> DuplicateAsync(IEnumerable<string> targets)
        {
            var response = new AddResponseModel();
            foreach (var target in targets)
            {
                var fullPath = ParsePath(target);
                if (fullPath.Directory != null)
                {
                    var parentPath = fullPath.Directory.Parent.FullName;
                    var name = fullPath.Directory.Name;
                    var newName = string.Format(@"{0}\{1} copy", parentPath, name);
                    if (!Directory.Exists(newName))
                    {
                        DirectoryCopy(fullPath.Directory, newName, true);
                    }
                    else
                    {
                        for (int i = 1; i < 100; i++)
                        {
                            newName = string.Format(@"{0}\{1} copy {2}", parentPath, name, i);
                            if (!Directory.Exists(newName))
                            {
                                DirectoryCopy(fullPath.Directory, newName, true);
                                break;
                            }
                        }
                    }
                    response.Added.Add(BaseModel.Create(new DirectoryInfo(newName), fullPath.Root));
                }
                else
                {
                    var parentPath = fullPath.File.Directory.FullName;
                    var name = fullPath.File.Name.Substring(0, fullPath.File.Name.Length - fullPath.File.Extension.Length);
                    var ext = fullPath.File.Extension;

                    var newName = string.Format(@"{0}\{1} copy{2}", parentPath, name, ext);

                    if (!System.IO.File.Exists(newName))
                    {
                        fullPath.File.CopyTo(newName);
                    }
                    else
                    {
                        for (int i = 1; i < 100; i++)
                        {
                            newName = string.Format(@"{0}\{1} copy {2}{3}", parentPath, name, i, ext);
                            if (!System.IO.File.Exists(newName))
                            {
                                fullPath.File.CopyTo(newName);
                                break;
                            }
                        }
                    }
                    response.Added.Add(BaseModel.Create(new FileInfo(newName), fullPath.Root));
                }
            }
            return await Json(response);
        }

        public async Task<JsonResult> ThumbsAsync(IEnumerable<string> targets)
        {
            var response = new ThumbsResponseModel();
            foreach (string target in targets)
            {
                var path = ParsePath(target);
                response.Images.Add(target, path.Root.GenerateThumbHash(path.File));
                //response.Images.Add(target, path.Root.GenerateThumbHash(path.File) + path.File.Extension); // 2018.02.23: Fix
            }
            return await Json(response);
        }

        public async Task<JsonResult> DimAsync(string target)
        {
            var path = ParsePath(target);
            var response = new DimResponseModel(path.Root.GetImageDimension(path.File));
            return await Json(response);
        }

        public async Task<JsonResult> ResizeAsync(string target, int width, int height)
        {
            var path = ParsePath(target);
            RemoveThumbs(path);
            path.Root.PicturesEditor.Resize(path.File.FullName, width, height);
            var output = new ChangedResponseModel();
            output.Changed.Add((FileModel)BaseModel.Create(path.File, path.Root));
            return await Json(output);
        }

        public async Task<JsonResult> CropAsync(string target, int x, int y, int width, int height)
        {
            var path = ParsePath(target);
            RemoveThumbs(path);
            path.Root.PicturesEditor.Crop(path.File.FullName, x, y, width, height);
            var output = new ChangedResponseModel();
            output.Changed.Add((FileModel)BaseModel.Create(path.File, path.Root));
            return await Json(output);
        }

        public async Task<JsonResult> RotateAsync(string target, int degree)
        {
            var path = ParsePath(target);
            RemoveThumbs(path);
            path.Root.PicturesEditor.Rotate(path.File.FullName, degree);
            var output = new ChangedResponseModel();
            output.Changed.Add((FileModel)BaseModel.Create(path.File, path.Root));
            return await Json(output);
        }

        #endregion IDriver
    }
}
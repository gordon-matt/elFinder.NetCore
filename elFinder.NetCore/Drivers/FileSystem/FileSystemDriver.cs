using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using elFinder.NetCore.Drawing;
using elFinder.NetCore.Helpers;
using elFinder.NetCore.Models;
using elFinder.NetCore.Models.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore.Drivers.FileSystem
{
    /// <summary>
    /// Represents a driver for local file system
    /// </summary>
    public class FileSystemDriver : BaseDriver, IDriver
    {
        private const string _volumePrefix = "v";
        
        #region Constructor

        /// <summary>
        /// Initialize new instance of class ElFinder.FileSystemDriver
        /// </summary>
        public FileSystemDriver()
        {
            VolumePrefix = _volumePrefix;
            Roots = new List<RootVolume>();
        }

        #endregion Constructor

        #region IDriver Members

        public async Task<FullPath> GetFullPathAsync(string target)
        {
            if (string.IsNullOrEmpty(target)) return null;

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
            var root = Roots.First(r => r.VolumeId == volumePrefix);
            var rootDirectory = new DirectoryInfo(root.RootDirectory);
            string path = Utils.DecodePath(pathHash);
            string dirUrl = path != rootDirectory.Name ? path : string.Empty;
            var dir = new FileSystemDirectory(root.RootDirectory + dirUrl);

            if (await dir.ExistsAsync)
            {
                return new FullPath(root, dir, target);
            }
            else
            {
                var file = new FileSystemFile(root.RootDirectory + dirUrl);
                return new FullPath(root, file, target);
            }
        }

        public async Task<JsonResult> CropAsync(FullPath path, int x, int y, int width, int height)
        {
            await RemoveThumbs(path);

            // Crop Image
            ImageWithMimeType image;
            using (var stream = new FileStream(path.File.FullName, FileMode.Open))
            {
                image = path.RootVolume.PictureEditor.Crop(stream, x, y, width, height);
            }

            using (var fileStream = File.Create(path.File.FullName))
            {
                await image.ImageStream.CopyToAsync(fileStream);
            }

            var output = new ChangedResponseModel();
            output.Changed.Add((FileModel)await BaseModel.Create(this, path.File, path.RootVolume));
            return await Json(output);
        }

        public async Task<JsonResult> DimAsync(FullPath path)
        {
            using (var stream = new FileStream(path.File.FullName, FileMode.Open))
            {
                var response = new DimResponseModel(path.RootVolume.PictureEditor.ImageSize(stream));
                return await Json(response);
            }
        }

        public async Task<JsonResult> DuplicateAsync(IEnumerable<FullPath> paths)
        {
            var response = new AddResponseModel();
            foreach (var path in paths)
            {
                if (path.IsDirectory)
                {
                    var parentPath = path.Directory.Parent.FullName;
                    var name = path.Directory.Name;
                    string newName = $"{parentPath}{Path.DirectorySeparatorChar}{name} copy";
                    if (!Directory.Exists(newName))
                    {
                        DirectoryCopy(path.Directory.FullName, newName, true);
                    }
                    else
                    {
                        for (int i = 1; i < 100; i++)
                        {
                            newName = $"{parentPath}{Path.DirectorySeparatorChar}{name} copy {i}";
                            if (!Directory.Exists(newName))
                            {
                                DirectoryCopy(path.Directory.FullName, newName, true);
                                break;
                            }
                        }
                    }
                    response.Added.Add(await BaseModel.Create(this, new FileSystemDirectory(newName), path.RootVolume));
                }
                else
                {
                    var parentPath = path.File.Directory.FullName;
                    var name = path.File.Name.Substring(0, path.File.Name.Length - path.File.Extension.Length);
                    var ext = path.File.Extension;

                    string newName = $"{parentPath}{Path.DirectorySeparatorChar}{name} copy{ext}";

                    if (!File.Exists(newName))
                        File.Copy(path.File.FullName, newName);
                    else
                    {
                        for (int i = 1; i < 100; i++)
                        {
                            newName = $"{parentPath}{Path.DirectorySeparatorChar}{name} copy {i}{ext}";
                            if (!File.Exists(newName))
                            {
                                File.Copy(path.File.FullName, newName);
                                break;
                            }
                        }
                    }
                    response.Added.Add(await BaseModel.Create(this, new FileSystemFile(newName), path.RootVolume));
                }
            }
            return await Json(response);
        }

        public async Task<IActionResult> FileAsync(FullPath path, bool download)
        {
            IActionResult result;

            if (path.IsDirectory)
            {
                result = new ForbidResult();
            }
            if (!await path.File.ExistsAsync)
            {
                result = new NotFoundResult();
            }
            if (path.RootVolume.IsShowOnly)
            {
                result = new ForbidResult();
            }
            //result = new DownloadFileResult(fullPath.File, download);
            string contentType = download ? "application/octet-stream" : Utils.GetMimeType(path.File);
            result = new PhysicalFileResult(path.File.FullName, contentType);

            return await Task.FromResult(result);
        }

        public async Task<JsonResult> GetAsync(FullPath path)
        {
            var response = new GetResponseModel();
            using (var reader = new StreamReader(await path.File.OpenReadAsync()))
            {
                response.Content = reader.ReadToEnd();
            }
            return await Json(response);
        }

        public async Task<JsonResult> InitAsync(FullPath path)
        {
            if (path == null)
            {
                var root = Roots.FirstOrDefault(r => r.StartDirectory != null);
                if (root == null)
                {
                    root = Roots.First();
                }

                path = new FullPath(root, new FileSystemDirectory(root.StartDirectory ?? root.RootDirectory), null);
            }

            var response = new InitResponseModel(await BaseModel.Create(this, path.Directory, path.RootVolume), new Options(path));

            foreach (var item in await path.Directory.GetFilesAsync())
            {
                if (!item.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.Files.Add(await BaseModel.Create(this, item, path.RootVolume));
                }
            }
            foreach (var item in await path.Directory.GetDirectoriesAsync())
            {
                if (!item.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.Files.Add(await BaseModel.Create(this, item, path.RootVolume));
                }
            }

            foreach (var item in Roots)
            {
                response.Files.Add(await BaseModel.Create(this, new FileSystemDirectory(item.RootDirectory), item));
            }

            if (path.RootVolume.RootDirectory != path.Directory.FullName)
            {
                var dirInfo = new DirectoryInfo(path.RootVolume.RootDirectory);
                foreach (var item in dirInfo.GetDirectories())
                {
                    var attributes = item.Attributes;
                    if (!attributes.HasFlag(FileAttributes.Hidden))
                    {
                        response.Files.Add(await BaseModel.Create(this, new FileSystemDirectory(item), path.RootVolume));
                    }
                }
            }

            if (path.RootVolume.MaxUploadSize.HasValue)
            {
                response.UploadMaxSize = path.RootVolume.MaxUploadSizeInKb.Value + "K";
            }
            return await Json(response);
        }

        public async Task<JsonResult> ListAsync(FullPath path)
        {
            var response = new ListResponseModel();

            foreach (var item in await path.Directory.GetFilesAsync())
            {
                if (!item.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.List.Add(item.Name);
                }
            }
            foreach (var item in await path.Directory.GetDirectoriesAsync())
            {
                if (!item.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.List.Add(item.Name);
                }
            }
            return await Json(response);
        }

        public async Task<JsonResult> MakeDirAsync(FullPath path, string name)
        {
            var newDir = new FileSystemDirectory(Path.Combine(path.Directory.FullName, name));
            await newDir.CreateAsync();

            var response = new AddResponseModel();
            response.Added.Add(await BaseModel.Create(this, newDir, path.RootVolume));
            return await Json(response);
        }

        public async Task<JsonResult> MakeFileAsync(FullPath path, string name)
        {
            var newFile = new FileSystemFile(Path.Combine(path.Directory.FullName, name));
            await newFile.CreateAsync();

            var response = new AddResponseModel();
            response.Added.Add(await BaseModel.Create(this, newFile, path.RootVolume));
            return await Json(response);
        }

        public async Task<JsonResult> OpenAsync(FullPath path, bool tree)
        {
            var response = new OpenResponse(await BaseModel.Create(this, path.Directory, path.RootVolume), path);
            foreach (var item in await path.Directory.GetFilesAsync())
            {
                if (!item.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.Files.Add(await BaseModel.Create(this, item, path.RootVolume));
                }
            }
            foreach (var item in await path.Directory.GetDirectoriesAsync())
            {
                if (!item.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.Files.Add(await BaseModel.Create(this, item, path.RootVolume));
                }
            }

            // Add parents
            if (tree)
            {
                var parent = path.Directory;

                var rootDirectory = new DirectoryInfo(path.RootVolume.RootDirectory);
                while (parent != null && parent.Name != rootDirectory.Name)
                {
                    // Update parent
                    parent = parent.Parent;

                    // Ensure it's a child of the root
                    if (parent != null && path.RootVolume.RootDirectory.Contains(parent.Name))
                    {
                        response.Files.Insert(0, await BaseModel.Create(this, parent, path.RootVolume));
                    }
                }
            }

            return await Json(response);
        }

        public async Task<JsonResult> ParentsAsync(FullPath path)
        {
            var response = new TreeResponseModel();
            if (path.Directory.FullName == path.RootVolume.RootDirectory)
            {
                response.Tree.Add(await BaseModel.Create(this, path.Directory, path.RootVolume));
            }
            else
            {
                var parent = path.Directory;
                foreach (var item in await parent.Parent.GetDirectoriesAsync())
                {
                    response.Tree.Add(await BaseModel.Create(this, item, path.RootVolume));
                }
                while (parent.FullName != path.RootVolume.RootDirectory)
                {
                    parent = parent.Parent;
                    response.Tree.Add(await BaseModel.Create(this, parent, path.RootVolume));
                }
            }
            return await Json(response);
        }

        public async Task<JsonResult> PasteAsync(FullPath dest, IEnumerable<FullPath> paths, bool isCut)
        {
            var response = new ReplaceResponseModel();
            foreach (var src in paths)
            {
                if (src.IsDirectory)
                {
                    var newDir = new FileSystemDirectory(Path.Combine(dest.Directory.FullName, src.Directory.Name));
                    if (await newDir.ExistsAsync)
                        Directory.Delete(newDir.FullName, true);

                    if (isCut)
                    {
                        await RemoveThumbs(src);
                        Directory.Move(src.Directory.FullName, newDir.FullName);
                        response.Removed.Add(src.HashedTarget);
                    }
                    else
                        DirectoryCopy(src.Directory.FullName, newDir.FullName, true);

                    response.Added.Add(await BaseModel.Create(this, newDir, dest.RootVolume));
                }
                else
                {
                    var newFile = new FileSystemFile(Path.Combine(dest.Directory.FullName, src.File.Name));
                    if (await newFile.ExistsAsync)
                        await newFile.DeleteAsync();

                    if (isCut)
                    {
                        await RemoveThumbs(src);
                        File.Move(src.File.FullName, newFile.FullName);
                        response.Removed.Add(src.HashedTarget);
                    }
                    else
                    {
                        File.Copy(src.File.FullName, newFile.FullName);
                    }
                    response.Added.Add(await BaseModel.Create(this, newFile, dest.RootVolume));
                }
            }
            return await Json(response);
        }

        public async Task<JsonResult> PutAsync(FullPath path, string content)
        {
            var response = new ChangedResponseModel();
            using (var fileStream = new FileStream(path.File.FullName, FileMode.Create))
            using (var writer = new StreamWriter(fileStream))
            {
                writer.Write(content);
            }
            response.Changed.Add((FileModel)await BaseModel.Create(this, path.File, path.RootVolume));
            return await Json(response);
        }

        public async Task<JsonResult> RemoveAsync(IEnumerable<FullPath> paths)
        {
            var response = new RemoveResponseModel();
            foreach (var path in paths)
            {
                await RemoveThumbs(path);
                if (path.IsDirectory)
                {
                    Directory.Delete(path.Directory.FullName, true);
                }
                else
                {
                    File.Delete(path.File.FullName);
                }
                response.Removed.Add(path.HashedTarget);
            }
            return await Json(response);
        }

        public async Task<JsonResult> RenameAsync(FullPath path, string name)
        {
            var response = new ReplaceResponseModel();
            response.Removed.Add(path.HashedTarget);
            await RemoveThumbs(path);
            if (path.IsDirectory)
            {
                var newPath = new FileSystemDirectory(Path.Combine(path.Directory.Parent.FullName, name));
                Directory.Move(path.Directory.FullName, newPath.FullName);
                response.Added.Add(await BaseModel.Create(this, newPath, path.RootVolume));
            }
            else
            {
                var newPath = new FileSystemFile(Path.Combine(path.File.DirectoryName, name));
                File.Move(path.File.FullName, newPath.FullName);
                response.Added.Add(await BaseModel.Create(this, newPath, path.RootVolume));
            }
            return await Json(response);
        }

        public async Task<JsonResult> ResizeAsync(FullPath path, int width, int height)
        {
            await RemoveThumbs(path);

            // Resize Image
            ImageWithMimeType image;
            using (var stream = new FileStream(path.File.FullName, FileMode.Open))
            {
                image = path.RootVolume.PictureEditor.Resize(stream, width, height);
            }

            using (var fileStream = File.Create(path.File.FullName))
            {
                await image.ImageStream.CopyToAsync(fileStream);
            }

            var output = new ChangedResponseModel();
            output.Changed.Add((FileModel)await BaseModel.Create(this, path.File, path.RootVolume));
            return await Json(output);
        }

        public async Task<JsonResult> RotateAsync(FullPath path, int degree)
        {
            await RemoveThumbs(path);

            // Rotate Image
            ImageWithMimeType image;
            using (var stream = new FileStream(path.File.FullName, FileMode.Open))
            {
                image = path.RootVolume.PictureEditor.Rotate(stream, degree);
            }

            using (var fileStream = File.Create(path.File.FullName))
            {
                await image.ImageStream.CopyToAsync(fileStream);
            }

            var output = new ChangedResponseModel();
            output.Changed.Add((FileModel)await BaseModel.Create(this, path.File, path.RootVolume));
            return await Json(output);
        }

        public async Task<JsonResult> ThumbsAsync(IEnumerable<FullPath> paths)
        {
            var response = new ThumbsResponseModel();
            foreach (var path in paths)
            {
                response.Images.Add(path.HashedTarget, await path.RootVolume.GenerateThumbHash(path.File));
                //response.Images.Add(target, path.Root.GenerateThumbHash(path.File) + path.File.Extension); // 2018.02.23: Fix
            }
            return await Json(response);
        }

        public async Task<JsonResult> TreeAsync(FullPath path)
        {
            var response = new TreeResponseModel();
            foreach (var item in await path.Directory.GetDirectoriesAsync())
            {
                if (!item.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.Tree.Add(await BaseModel.Create(this, item, path.RootVolume));
                }
            }
            return await Json(response);
        }

        public async Task<JsonResult> UploadAsync(FullPath path, IEnumerable<IFormFile> files)
        {
            int fileCount = files.Count();

            var response = new AddResponseModel();
            if (path.RootVolume.MaxUploadSize.HasValue)
            {
                for (int i = 0; i < fileCount; i++)
                {
                    IFormFile file = files.ElementAt(i);
                    if (file.Length > path.RootVolume.MaxUploadSize.Value)
                    {
                        return Error.MaxUploadFileSize();
                    }
                }
            }
            foreach (var file in files)
            {
                var p = new FileInfo(Path.Combine(path.Directory.FullName, Path.GetFileName(file.FileName)));

                if (p.Exists)
                {
                    if (path.RootVolume.UploadOverwrite)
                    {
                        //if file already exist we rename the current file,
                        //and if upload is succesfully delete temp file, in otherwise we restore old file
                        string tmpPath = p.FullName + Guid.NewGuid();
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
                                File.Delete(p.FullName);
                                File.Move(tmpPath, p.FullName);
                            }
                            else
                            {
                                File.Delete(tmpPath);
                            }
                        }
                    }
                    else
                    {
                        using (var fileStream = new FileStream(Path.Combine(p.DirectoryName, Utils.GetDuplicatedName(p)), FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                    }
                }
                else
                {
                    using (var fileStream = new FileStream(p.FullName, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
                response.Added.Add((FileModel)await BaseModel.Create(this, new FileSystemFile(p.FullName), path.RootVolume));
            }
            return await Json(response);
        }

        #endregion IDriver Members

        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(sourceDirName);
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
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private async Task RemoveThumbs(FullPath path)
        {
            if (path.IsDirectory)
            {
                if (await path.Directory.ExistsAsync)
                    await path.Directory.DeleteAsync();
            }
            else
            {
                if (await path.File.ExistsAsync)
                    await path.File.DeleteAsync();
            }
        }
    }
}
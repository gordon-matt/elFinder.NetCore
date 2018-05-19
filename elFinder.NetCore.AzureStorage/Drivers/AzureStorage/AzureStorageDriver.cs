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
using Microsoft.WindowsAzure.Storage.File;

namespace elFinder.NetCore.Drivers.AzureStorage
{
    /// <summary>
    /// Represents a driver for AzureStorage system
    /// </summary>
    public class AzureStorageDriver : BaseDriver, IDriver
    {
        private const string _volumePrefix = "a";

        #region Constructor

        /// <summary>
        /// Initialize new instance of class ElFinder.AzureStorageDriver
        /// </summary>
        public AzureStorageDriver()
        {
            VolumePrefix = _volumePrefix;
            Roots = new List<RootVolume>();
        }

        #endregion Constructor

        #region IDriver Members

        public async Task<FullPath> GetFullPathAsync(string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                return null;
            }

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
            string path = Utils.DecodePath(pathHash);
            string dirUrl = path != root.RootDirectory ? path : string.Empty;
            var dir = new AzureStorageDirectory(root.RootDirectory + dirUrl);

            if (await dir.ExistsAsync)
            {
                return new FullPath(root, dir, target);
            }
            else
            {
                var file = new AzureStorageFile(root.RootDirectory + dirUrl);
                return new FullPath(root, file, target);
            }
        }

        public async Task<JsonResult> OpenAsync(FullPath path, bool tree)
        {
            var response = new OpenResponse(await BaseModel.Create(this, path.Directory, path.RootVolume), path);

            // Get all files and directories
            var items = await AzureStorageAPI.ListFilesAndDirectories(path.Directory.FullName);

            // Add visible files
            foreach (var file in items.Where(i => i is CloudFile))
            {
                var f = new AzureStorageFile(file as CloudFile);
                if (!f.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.Files.Add(await BaseModel.Create(this, f, path.RootVolume));
                }
            }

            // Add visible directories
            foreach (var dir in items.Where(i => i is CloudFileDirectory))
            {
                var d = new AzureStorageDirectory(dir as CloudFileDirectory);
                if (!d.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.Files.Add(await BaseModel.Create(this, d, path.RootVolume));
                }
            }

            // Add parents
            if (tree)
            {
                var parent = path.Directory;

                while (parent != null && parent.FullName != path.RootVolume.RootDirectory)
                {
                    // Update parent
                    parent = parent.Parent;

                    // Ensure it's a child of the root
                    if (parent != null && path.RootVolume.RootDirectory.Contains(parent.FullName))
                    {
                        response.Files.Insert(0, await BaseModel.Create(this, parent, path.RootVolume));
                    }
                }
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

                path = new FullPath(root, new AzureStorageDirectory(root.StartDirectory ?? root.RootDirectory), null);
            }

            var response = new InitResponseModel(await BaseModel.Create(this, path.Directory, path.RootVolume), new Options(path));

            // Get all files and directories
            var items = await AzureStorageAPI.ListFilesAndDirectories(path.Directory.FullName);

            // Add visible files
            foreach (var file in items.Where(i => i is CloudFile))
            {
                var f = new AzureStorageFile(file as CloudFile);
                if (!f.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.Files.Add(await BaseModel.Create(this, f, path.RootVolume));
                }
            }

            // Add visible directories
            foreach (var dir in items.Where(i => i is CloudFileDirectory))
            {
                var d = new AzureStorageDirectory(dir as CloudFileDirectory);
                if (!d.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.Files.Add(await BaseModel.Create(this, d, path.RootVolume));
                }
            }

            // Add roots
            foreach (var root in Roots)
            {
                response.Files.Add(await BaseModel.Create(this, new AzureStorageDirectory(root.RootDirectory), root));
            }

            if (path.RootVolume.RootDirectory != path.Directory.FullName)
            {
                // Get all files and directories
                var entries = await AzureStorageAPI.ListFilesAndDirectories(path.RootVolume.RootDirectory);

                // Add visible directories
                foreach (var dir in entries.Where(i => i is CloudFileDirectory))
                {
                    var d = new AzureStorageDirectory(dir as CloudFileDirectory);
                    if (!d.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        response.Files.Add(await BaseModel.Create(this, d, path.RootVolume));
                    }
                }
            }

            if (path.RootVolume.MaxUploadSize.HasValue)
            {
                response.UploadMaxSize = $"{path.RootVolume.MaxUploadSizeInKb.Value}K";
            }

            return await Json(response);
        }

        public async Task<IActionResult> FileAsync(FullPath path, bool download)
        {
            // Check if path is directory
            if (path.IsDirectory)
            {
                return new ForbidResult();
            }

            // Check if path exists
            if (!await AzureStorageAPI.FileExists(path.File.FullName))
            {
                return new NotFoundResult();
            }

            // Check if access is allowed
            if (path.RootVolume.IsShowOnly)
            {
                return new ForbidResult();
            }

            //result = new DownloadFileResult(fullPath.File, download);
            string contentType = download ? "application/octet-stream" : Utils.GetMimeType(path.File);

            var stream = new MemoryStream();
            await AzureStorageAPI.Get(path.File.FullName, stream);
            stream.Position = 0;
            return new FileStreamResult(stream, contentType);
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
                // Not root level

                // Go back to root
                var parent = path.Directory;

                while (parent != null && parent.Name != path.RootVolume.RootDirectory)
                {
                    // Update parent
                    parent = parent.Parent;

                    // Ensure it's a child of the root
                    if (parent != null && path.RootVolume.RootDirectory.Contains(parent.Name))
                    {
                        response.Tree.Insert(0, await BaseModel.Create(this, parent, path.RootVolume));
                    }
                }

                // Check that directory has a parent
                if (path.Directory.Parent != null)
                {
                    var items = await AzureStorageAPI.ListFilesAndDirectories(path.Directory.Parent.FullName);

                    // Add all visible directories except the target
                    foreach (var dir in items.Where(i => i is CloudFileDirectory && ((CloudFileDirectory)i).Name != path.Directory.Name))
                    {
                        var d = new AzureStorageDirectory(dir as CloudFileDirectory);
                        if (!d.Attributes.HasFlag(FileAttributes.Hidden))
                            response.Tree.Add(await BaseModel.Create(this, d, path.RootVolume));
                    }
                }
            }
            return await Json(response);
        }

        public async Task<JsonResult> TreeAsync(FullPath path)
        {
            var response = new TreeResponseModel();

            var items = await AzureStorageAPI.ListFilesAndDirectories(path.Directory.FullName);

            // Add visible directories
            foreach (var dir in items.Where(i => i is CloudFileDirectory))
            {
                var d = new AzureStorageDirectory(dir as CloudFileDirectory);
                if (!d.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.Tree.Add(await BaseModel.Create(this, d, path.RootVolume));
                }
            }

            return await Json(response);
        }

        public async Task<JsonResult> ListAsync(FullPath path)
        {
            var response = new ListResponseModel();

            // Get all files and directories
            var items = await AzureStorageAPI.ListFilesAndDirectories(path.Directory.FullName);

            // Add visible files
            foreach (var file in items.Where(i => i is CloudFile))
            {
                var f = new AzureStorageFile(file as CloudFile);
                if (!f.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.List.Add(f.Name);
                }
            }

            // Add visible directories
            foreach (var dir in items.Where(i => i is CloudFileDirectory))
            {
                var d = new AzureStorageDirectory(dir as CloudFileDirectory);
                if (!d.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.List.Add(d.Name);
                }
            }

            return await Json(response);
        }

        public async Task<JsonResult> MakeDirAsync(FullPath path, string name)
        {
            // Create directory
            var newDir = new AzureStorageDirectory(AzureStorageAPI.UrlCombine(path.Directory.FullName, name));
            await newDir.CreateAsync();

            var response = new AddResponseModel();
            response.Added.Add(await BaseModel.Create(this, newDir, path.RootVolume));

            return await Json(response);
        }

        public async Task<JsonResult> MakeFileAsync(FullPath path, string name)
        {
            var newFile = new AzureStorageFile(AzureStorageAPI.UrlCombine(path.Directory.FullName, name));
            await newFile.CreateAsync();

            var response = new AddResponseModel();
            response.Added.Add(await BaseModel.Create(this, newFile, path.RootVolume));
            return await Json(response);
        }

        public async Task<JsonResult> RenameAsync(FullPath path, string name)
        {
            var response = new ReplaceResponseModel();

            response.Removed.Add(path.HashedTarget);

            RemoveThumbs(path);

            if (path.IsDirectory)
            {
                // Get new path
                var newPath = AzureStorageAPI.UrlCombine(path.Directory.Parent.FullName ?? string.Empty, name);

                // Move file
                await AzureStorageAPI.MoveDirectory(path.Directory.FullName, newPath);

                // Add it to added entries list
                response.Added.Add(await BaseModel.Create(this, new AzureStorageDirectory(newPath), path.RootVolume));
            }
            else
            {
                // Get new path
                var newPath = AzureStorageAPI.UrlCombine(path.File.DirectoryName ?? string.Empty, name);

                // Move file
                await AzureStorageAPI.MoveFile(path.File.FullName, newPath);

                // Add it to added entries list
                response.Added.Add(await BaseModel.Create(this, new AzureStorageFile(newPath), path.RootVolume));
            }

            return await Json(response);
        }

        public async Task<JsonResult> RemoveAsync(IEnumerable<FullPath> paths)
        {
            var response = new RemoveResponseModel();

            foreach (FullPath path in paths)
            {
                RemoveThumbs(path);

                if (path.IsDirectory)
                {
                    await AzureStorageAPI.DeleteDirectory(path.Directory.FullName);
                }
                else
                {
                    await AzureStorageAPI.DeleteFile(path.File.FullName);
                }

                response.Removed.Add(path.HashedTarget);
            }
            return await Json(response);
        }

        public async Task<JsonResult> GetAsync(FullPath path)
        {
            var response = new GetResponseModel();

            // Get content
            using (var stream = new MemoryStream())
            {
                await AzureStorageAPI.Get(path.File.FullName, stream);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    response.Content = reader.ReadToEnd();
                }
            }

            return await Json(response);
        }

        public async Task<JsonResult> PutAsync(FullPath path, string content)
        {
            var response = new ChangedResponseModel();

            // Write content
            await AzureStorageAPI.Put(path.File.FullName, content);

            response.Changed.Add((FileModel)await BaseModel.Create(this, path.File, path.RootVolume));
            return await Json(response);
        }

        public async Task<JsonResult> PasteAsync(FullPath dest, IEnumerable<FullPath> paths, bool isCut)
        {
            var response = new ReplaceResponseModel();

            foreach (var src in paths)
            {
                if (src.IsDirectory)
                {
                    var newDir = new AzureStorageDirectory(AzureStorageAPI.UrlCombine(dest.Directory.FullName, src.Directory.Name));

                    // Check if it already exists
                    if (await newDir.ExistsAsync)
                    {
                        // Exists
                        await newDir.DeleteAsync();
                    }

                    if (isCut)
                    {
                        RemoveThumbs(src);
                        await AzureStorageAPI.MoveDirectory(src.Directory.FullName, newDir.FullName);
                        response.Removed.Add(src.HashedTarget);
                    }
                    else
                    {
                        // Copy directory
                        await AzureStorageAPI.CopyDirectory(src.Directory.FullName, newDir.FullName);
                    }
                    response.Added.Add(await BaseModel.Create(this, newDir, dest.RootVolume));
                }
                else
                {
                    string newFilePath = AzureStorageAPI.UrlCombine(dest.Directory.FullName, src.File.Name);
                    await AzureStorageAPI.DeleteFileIfExists(newFilePath);

                    if (isCut)
                    {
                        RemoveThumbs(src);

                        // Move file
                        await AzureStorageAPI.MoveFile(src.File.FullName, newFilePath);

                        response.Removed.Add(src.HashedTarget);
                    }
                    else
                    {
                        // Copy file
                        await AzureStorageAPI.CopyFile(src.File.FullName, newFilePath);
                    }

                    response.Added.Add(await BaseModel.Create(this, new AzureStorageFile(newFilePath), dest.RootVolume));
                }
            }

            return await Json(response);
        }

        public async Task<JsonResult> UploadAsync(FullPath path, IEnumerable<IFormFile> files)
        {
            int fileCount = files.Count();

            var response = new AddResponseModel();

            // Check if max upload size is set and that no files exceeds it
            if (path.RootVolume.MaxUploadSize.HasValue && files.Any(x => x.Length > path.RootVolume.MaxUploadSize))
            {
                // Max upload size exceeded
                return Error.MaxUploadFileSize();
            }

            // Loop files
            foreach (var file in files)
            {
                // Validate file name
                if (string.IsNullOrWhiteSpace(file.FileName)) throw new ArgumentNullException(nameof(IFormFile.FileName));

                // Get path
                var p = new AzureStorageFile(AzureStorageAPI.UrlCombine(path.Directory.FullName, Path.GetFileName(file.FileName)));

                // Check if it already exists
                if (await p.ExistsAsync)
                {
                    // Check if overwrite on upload is supported
                    if (path.RootVolume.UploadOverwrite)
                    {
                        // If file already exist we rename the current file.
                        // If upload is successful, delete temp file. Otherwise, we restore old file.
                        var tmpPath = p.FullName + Guid.NewGuid();

                        // Save file
                        var uploaded = false;

                        try
                        {
                            await AzureStorageAPI.Upload(file, tmpPath);
                            uploaded = true;
                        }
                        catch (Exception) { }
                        finally
                        {
                            // Check that file was saved correctly
                            if (uploaded)
                            {
                                // Delete file
                                await p.DeleteAsync();

                                // Move file
                                await AzureStorageAPI.MoveFile(tmpPath, p.FullName);
                            }
                            else
                            {
                                // Delete temporary file
                                await AzureStorageAPI.DeleteFile(tmpPath);
                            }
                        }
                    }
                    else
                    {
                        // Ensure directy name is set
                        if (string.IsNullOrEmpty(p.Directory.Name)) throw new ArgumentNullException("Directory");

                        // Save file
                        await AzureStorageAPI.Upload(file, AzureStorageAPI.UrlCombine(p.Directory.FullName, await AzureStorageAPI.GetDuplicatedName(p)));
                    }
                }
                else
                {
                    // Save file
                    await AzureStorageAPI.Upload(file, p.FullName);
                }

                response.Added.Add((FileModel)await BaseModel.Create(this, new AzureStorageFile(p.FullName), path.RootVolume));
            }
            return await Json(response);
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
                    string newName = $"{parentPath}/{name} copy";

                    // Check if directory already exists
                    if (!await AzureStorageAPI.DirectoryExists(newName))
                    {
                        // Doesn't exist
                        await AzureStorageAPI.CopyDirectory(path.Directory.FullName, newName);
                    }
                    else
                    {
                        // Already exists, create numbered copy
                        var newNameFound = false;
                        for (int i = 1; i < 100; i++)
                        {
                            newName = $"{parentPath}/{name} copy {i}";

                            // Test that it doesn't exist
                            if (!await AzureStorageAPI.DirectoryExists(newName))
                            {
                                await AzureStorageAPI.CopyDirectory(path.Directory.FullName, newName);
                                newNameFound = true;
                                break;
                            }
                        }

                        // Check if new name was found
                        if (!newNameFound) return Error.NewNameSelectionException($@"{parentPath}/{name} copy");
                    }

                    response.Added.Add(await BaseModel.Create(this, new AzureStorageDirectory(newName), path.RootVolume));
                }
                else // File
                {
                    var parentPath = path.File.Directory.FullName;
                    var name = path.File.Name.Substring(0, path.File.Name.Length - path.File.Extension.Length);
                    var ext = path.File.Extension;

                    string newName = $"{parentPath}/{name} copy{ext}";

                    // Check if file already exists
                    if (!await AzureStorageAPI.FileExists(newName))
                    {
                        // Doesn't exist
                        await AzureStorageAPI.CopyFile(path.File.FullName, newName);
                    }
                    else
                    {
                        // Already exists, create numbered copy
                        var newNameFound = false;
                        for (var i = 1; i < 100; i++)
                        {
                            // Compute new name
                            newName = $@"{parentPath}/{name} copy {i}{ext}";

                            // Test that it doesn't exist
                            if (!await AzureStorageAPI.FileExists(newName))
                            {
                                await AzureStorageAPI.CopyFile(path.File.FullName, newName);
                                newNameFound = true;
                                break;
                            }
                        }

                        // Check if new name was found
                        if (!newNameFound) return Error.NewNameSelectionException($@"{parentPath}/{name} copy");
                    }

                    response.Added.Add(await BaseModel.Create(this, new AzureStorageFile(newName), path.RootVolume));
                }
            }
            return await Json(response);
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

        public async Task<JsonResult> DimAsync(FullPath path)
        {
            using (var stream = await AzureStorageAPI.FileStream(path.File.FullName))
            {
                var response = new DimResponseModel(path.RootVolume.PictureEditor.ImageSize(stream));
                return await Json(response);
            }
        }

        public async Task<JsonResult> ResizeAsync(FullPath path, int width, int height)
        {
            RemoveThumbs(path);

            // Resize Image
            ImageWithMimeType image;
            using (var stream = await path.File.OpenReadAsync())
            {
                image = path.RootVolume.PictureEditor.Resize(stream, width, height);
            }

            await AzureStorageAPI.Put(path.File.FullName, image.ImageStream);

            var output = new ChangedResponseModel();
            output.Changed.Add((FileModel)await BaseModel.Create(this, path.File, path.RootVolume));
            return await Json(output);
        }

        public async Task<JsonResult> CropAsync(FullPath path, int x, int y, int width, int height)
        {
            RemoveThumbs(path);

            // Crop Image
            ImageWithMimeType image;
            using (var stream = await path.File.OpenReadAsync())
            {
                image = path.RootVolume.PictureEditor.Crop(stream, x, y, width, height);
            }

            await AzureStorageAPI.Put(path.File.FullName, image.ImageStream);

            var output = new ChangedResponseModel();
            output.Changed.Add((FileModel)await BaseModel.Create(this, path.File, path.RootVolume));
            return await Json(output);
        }

        public async Task<JsonResult> RotateAsync(FullPath path, int degree)
        {
            RemoveThumbs(path);

            // Crop Image
            ImageWithMimeType image;
            using (var stream = await path.File.OpenReadAsync())
            {
                image = path.RootVolume.PictureEditor.Rotate(stream, degree);
            }

            await AzureStorageAPI.Put(path.File.FullName, image.ImageStream);

            var output = new ChangedResponseModel();
            output.Changed.Add((FileModel)await BaseModel.Create(this, path.File, path.RootVolume));
            return await Json(output);
        }

        #endregion IDriver Members

        private async void RemoveThumbs(FullPath path)
        {
            if (path.IsDirectory)
            {
                var thumbPath = path.RootVolume.GenerateThumbPath(path.Directory);
                if (thumbPath == null) return;

                var thumbDir = new AzureStorageDirectory(thumbPath);
                if (await thumbDir.ExistsAsync)
                {
                    await thumbDir.DeleteAsync();
                }
            }
            else
            {
                var thumbPath = await path.RootVolume.GenerateThumbPath(path.File);
                if (thumbPath == null)
                {
                    return;
                }

                var thumbFile = new AzureStorageFile(thumbPath);
                if (await thumbFile.ExistsAsync)
                {
                    await thumbFile.DeleteAsync();
                }
            }
        }
    }
}
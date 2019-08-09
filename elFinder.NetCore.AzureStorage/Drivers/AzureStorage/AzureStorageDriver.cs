using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        public async Task<JsonResult> ArchiveAsync(FullPath parentPath, IEnumerable<FullPath> paths, string filename, string mimeType)
        {
            var response = new AddResponseModel();

            if (paths == null)
            {
                throw new NotSupportedException();
            }

            if (mimeType != "application/zip")
            {
                throw new NotSupportedException("Only .zip files are currently supported.");
            }

            // Parse target path

            var directoryInfo = parentPath.Directory;

            if (directoryInfo != null)
            {
                filename = filename ?? "newfile";

                if (filename.EndsWith(".zip"))
                {
                    filename = filename.Replace(".zip", "");
                }

                var newPath = AzureStorageAPI.PathCombine(directoryInfo.FullName, filename + ".zip");
                await AzureStorageAPI.DeleteFileIfExistsAsync(newPath);

                var archivePath = Path.GetTempFileName();
                using (var newFile = ZipFile.Open(archivePath, ZipArchiveMode.Update))
                {
                    foreach (var tg in paths)
                    {
                        if (tg.IsDirectory)
                        {
                            await AddDirectoryToArchiveAsync(newFile, tg.Directory, "");
                        }
                        else
                        {
                            var filePath = Path.GetTempFileName();
                            File.WriteAllBytes(filePath, await AzureStorageAPI.FileBytesAsync(tg.File.FullName));
                            newFile.CreateEntryFromFile(filePath, tg.File.Name);
                        }
                    }
                }

                using (var stream = new FileStream(archivePath, FileMode.Open))
                {
                    await AzureStorageAPI.PutAsync(newPath, stream);
                }

                // Cleanup
                File.Delete(archivePath);

                response.Added.Add((FileModel)await BaseModel.CreateAsync(this, new AzureStorageFile(newPath), parentPath.RootVolume));
            }

            return await Json(response);
        }

        public async Task<JsonResult> CropAsync(FullPath path, int x, int y, int width, int height)
        {
            await RemoveThumbsAsync(path);

            // Crop Image
            ImageWithMimeType image;
            using (var stream = await path.File.OpenReadAsync())
            {
                image = path.RootVolume.PictureEditor.Crop(stream, x, y, width, height);
            }

            await AzureStorageAPI.PutAsync(path.File.FullName, image.ImageStream);

            var output = new ChangedResponseModel();
            output.Changed.Add((FileModel)await BaseModel.CreateAsync(this, path.File, path.RootVolume));
            return await Json(output);
        }

        public async Task<JsonResult> DimAsync(FullPath path)
        {
            using (var stream = await AzureStorageAPI.FileStreamAsync(path.File.FullName))
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
                    string newName = $"{parentPath}/{name} copy";

                    // Check if directory already exists
                    if (!await AzureStorageAPI.DirectoryExistsAsync(newName))
                    {
                        // Doesn't exist
                        await AzureStorageAPI.CopyDirectoryAsync(path.Directory.FullName, newName);
                    }
                    else
                    {
                        // Already exists, create numbered copy
                        var newNameFound = false;
                        for (int i = 1; i < 100; i++)
                        {
                            newName = $"{parentPath}/{name} copy {i}";

                            // Test that it doesn't exist
                            if (!await AzureStorageAPI.DirectoryExistsAsync(newName))
                            {
                                await AzureStorageAPI.CopyDirectoryAsync(path.Directory.FullName, newName);
                                newNameFound = true;
                                break;
                            }
                        }

                        // Check if new name was found
                        if (!newNameFound) return Error.NewNameSelectionException($@"{parentPath}/{name} copy");
                    }

                    response.Added.Add(await BaseModel.CreateAsync(this, new AzureStorageDirectory(newName), path.RootVolume));
                }
                else // File
                {
                    var parentPath = path.File.Directory.FullName;
                    var name = path.File.Name.Substring(0, path.File.Name.Length - path.File.Extension.Length);
                    var ext = path.File.Extension;

                    string newName = $"{parentPath}/{name} copy{ext}";

                    // Check if file already exists
                    if (!await AzureStorageAPI.FileExistsAsync(newName))
                    {
                        // Doesn't exist
                        await AzureStorageAPI.CopyFileAsync(path.File.FullName, newName);
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
                            if (!await AzureStorageAPI.FileExistsAsync(newName))
                            {
                                await AzureStorageAPI.CopyFileAsync(path.File.FullName, newName);
                                newNameFound = true;
                                break;
                            }
                        }

                        // Check if new name was found
                        if (!newNameFound) return Error.NewNameSelectionException($@"{parentPath}/{name} copy");
                    }

                    response.Added.Add(await BaseModel.CreateAsync(this, new AzureStorageFile(newName), path.RootVolume));
                }
            }
            return await Json(response);
        }

        public async Task<JsonResult> ExtractAsync(FullPath fullPath, bool newFolder)
        {
            var response = new AddResponseModel();

            if (fullPath.IsDirectory || fullPath.File.Extension.ToLower() != ".zip")
            {
                throw new NotSupportedException("Only .zip files are currently supported.");
            }

            var rootPath = fullPath.File.Directory.FullName;

            if (newFolder)
            {
                // Azure doesn't like directory names that look like a file name i.e. blah.png
                // So iterate through the names until there's no more extension
                var path = Path.GetFileNameWithoutExtension(fullPath.File.Name);
                while (Path.HasExtension(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                }

                rootPath = AzureStorageAPI.PathCombine(rootPath, path);
                var rootDir = new AzureStorageDirectory(rootPath);
                if (!await rootDir.ExistsAsync)
                {
                    await rootDir.CreateAsync();
                }
                response.Added.Add(await BaseModel.CreateAsync(this, rootDir, fullPath.RootVolume));
            }

            // Create temp file
            var archivePath = Path.GetTempFileName();
            File.WriteAllBytes(archivePath, await AzureStorageAPI.FileBytesAsync(fullPath.File.FullName));

            using (var archive = ZipFile.OpenRead(archivePath))
            {
                var separator = Path.DirectorySeparatorChar.ToString();
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    try
                    {
                        var file = AzureStorageAPI.PathCombine(rootPath, entry.FullName);

                        if (file.EndsWith(separator)) //directory
                        {
                            var dir = new AzureStorageDirectory(file);

                            if (!await dir.ExistsAsync)
                            {
                                await dir.CreateAsync();
                            }
                            if (!newFolder)
                            {
                                response.Added.Add(await BaseModel.CreateAsync(this, dir, fullPath.RootVolume));
                            }
                        }
                        else
                        {
                            var filePath = Path.GetTempFileName();
                            entry.ExtractToFile(filePath, true);

                            using (var stream = new FileStream(filePath, FileMode.Open))
                            {
                                await AzureStorageAPI.PutAsync(file, stream);
                            }

                            File.Delete(filePath);

                            if (!newFolder)
                            {
                                response.Added.Add(await BaseModel.CreateAsync(this, new AzureStorageFile(file), fullPath.RootVolume));
                            }
                        }
                    }
                    catch //(Exception ex)
                    {
                        //throw new Exception(entry.FullName, ex);
                    }
                }
            }

            File.Delete(archivePath);

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
            if (!await AzureStorageAPI.FileExistsAsync(path.File.FullName))
            {
                return new NotFoundResult();
            }

            // Check if access is allowed
            if (path.RootVolume.IsShowOnly)
            {
                return new ForbidResult();
            }

            string contentType = download ? "application/octet-stream" : MimeHelper.GetMimeType(path.File.Extension);

            var stream = new MemoryStream();
            await AzureStorageAPI.GetAsync(path.File.FullName, stream);
            stream.Position = 0;
            return new FileStreamResult(stream, contentType);
        }

        public async Task<JsonResult> GetAsync(FullPath path)
        {
            var response = new GetResponseModel();

            // Get content
            using (var stream = new MemoryStream())
            {
                await AzureStorageAPI.GetAsync(path.File.FullName, stream);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    response.Content = reader.ReadToEnd();
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

            var response = new InitResponseModel(await BaseModel.CreateAsync(this, path.Directory, path.RootVolume), new Options(path));

            // Get all files and directories
            var items = await AzureStorageAPI.ListFilesAndDirectoriesAsync(path.Directory.FullName);

            // Add visible files
            foreach (var file in items.Where(i => i is CloudFile))
            {
                var f = new AzureStorageFile(file as CloudFile);
                if (!f.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.Files.Add(await BaseModel.CreateAsync(this, f, path.RootVolume));
                }
            }

            // Add visible directories
            foreach (var dir in items.Where(i => i is CloudFileDirectory))
            {
                var d = new AzureStorageDirectory(dir as CloudFileDirectory);
                if (!d.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.Files.Add(await BaseModel.CreateAsync(this, d, path.RootVolume));
                }
            }

            // Add roots
            foreach (var root in Roots)
            {
                response.Files.Add(await BaseModel.CreateAsync(this, new AzureStorageDirectory(root.RootDirectory), root));
            }

            if (path.RootVolume.RootDirectory != path.Directory.FullName)
            {
                // Get all files and directories
                var entries = await AzureStorageAPI.ListFilesAndDirectoriesAsync(path.RootVolume.RootDirectory);

                // Add visible directories
                foreach (var dir in entries.Where(i => i is CloudFileDirectory))
                {
                    var d = new AzureStorageDirectory(dir as CloudFileDirectory);
                    if (!d.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        response.Files.Add(await BaseModel.CreateAsync(this, d, path.RootVolume));
                    }
                }
            }

            if (path.RootVolume.MaxUploadSize.HasValue)
            {
                response.Options.UploadMaxSize = $"{path.RootVolume.MaxUploadSizeInKb.Value}K";
            }

            return await Json(response);
        }

        public async Task<JsonResult> ListAsync(FullPath path, IEnumerable<string> intersect)
        {
            var response = new ListResponseModel();

            // Get all files and directories
            var items = await AzureStorageAPI.ListFilesAndDirectoriesAsync(path.Directory.FullName);

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

            if (intersect.Any())
            {
                response.List.RemoveAll(l => !intersect.Contains(l));
            }

            return await Json(response);
        }

        public async Task<JsonResult> MakeDirAsync(FullPath path, string name, IEnumerable<string> dirs)
        {
            var response = new AddResponseModel();

            if (!string.IsNullOrEmpty(name))
            {
                // Create directory
                var newDir = new AzureStorageDirectory(AzureStorageAPI.PathCombine(path.Directory.FullName, name));
                await newDir.CreateAsync();
                response.Added.Add(await BaseModel.CreateAsync(this, newDir, path.RootVolume));
            }

            if (dirs.Any())
            {
                foreach (string dir in dirs)
                {
                    string dirName = dir.StartsWith("/") ? dir.Substring(1) : dir;
                    var newDir = new AzureStorageDirectory(AzureStorageAPI.PathCombine(path.Directory.FullName, dirName));
                    await newDir.CreateAsync();

                    response.Added.Add(await BaseModel.CreateAsync(this, newDir, path.RootVolume));

                    string relativePath = newDir.FullName.Substring(path.RootVolume.RootDirectory.Length);
                    response.Hashes.Add($"/{dirName}", path.RootVolume.VolumeId + HttpEncoder.EncodePath(relativePath));
                }
            }

            return await Json(response);
        }

        public async Task<JsonResult> MakeFileAsync(FullPath path, string name)
        {
            var newFile = new AzureStorageFile(AzureStorageAPI.PathCombine(path.Directory.FullName, name));
            await newFile.CreateAsync();

            var response = new AddResponseModel();
            response.Added.Add(await BaseModel.CreateAsync(this, newFile, path.RootVolume));
            return await Json(response);
        }

        public async Task<JsonResult> OpenAsync(FullPath path, bool tree)
        {
            var response = new OpenResponse(await BaseModel.CreateAsync(this, path.Directory, path.RootVolume), path);

            // Get all files and directories
            var items = await AzureStorageAPI.ListFilesAndDirectoriesAsync(path.Directory.FullName);

            // Add visible files
            foreach (var file in items.Where(i => i is CloudFile))
            {
                var f = new AzureStorageFile(file as CloudFile);
                if (!f.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.Files.Add(await BaseModel.CreateAsync(this, f, path.RootVolume));
                }
            }

            // Add visible directories
            foreach (var dir in items.Where(i => i is CloudFileDirectory))
            {
                var d = new AzureStorageDirectory(dir as CloudFileDirectory);
                if (!d.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.Files.Add(await BaseModel.CreateAsync(this, d, path.RootVolume));
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
                        response.Files.Insert(0, await BaseModel.CreateAsync(this, parent, path.RootVolume));
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
                response.Tree.Add(await BaseModel.CreateAsync(this, path.Directory, path.RootVolume));
            }
            else
            {
                var parent = path.Directory;
                foreach (var item in await parent.Parent.GetDirectoriesAsync())
                {
                    response.Tree.Add(await BaseModel.CreateAsync(this, item, path.RootVolume));
                }

                while (parent.FullName != path.RootVolume.RootDirectory)
                {
                    parent = parent.Parent;
                    response.Tree.Add(await BaseModel.CreateAsync(this, parent, path.RootVolume));
                }
            }
            return await Json(response);
        }

        public async Task<FullPath> ParsePathAsync(string target)
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
            string path = HttpEncoder.DecodePath(pathHash);
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

        public async Task<JsonResult> PasteAsync(FullPath dest, IEnumerable<FullPath> paths, bool isCut, IEnumerable<string> renames, string suffix)
        {
            var response = new ReplaceResponseModel();

            foreach (var src in paths)
            {
                if (src.IsDirectory)
                {
                    var newDir = new AzureStorageDirectory(AzureStorageAPI.PathCombine(dest.Directory.FullName, src.Directory.Name));

                    // Check if it already exists
                    if (await newDir.ExistsAsync)
                    {
                        await newDir.DeleteAsync();
                    }

                    if (isCut)
                    {
                        await RemoveThumbsAsync(src);
                        await AzureStorageAPI.MoveDirectoryAsync(src.Directory.FullName, newDir.FullName);
                        response.Removed.Add(src.HashedTarget);
                    }
                    else
                    {
                        // Copy directory
                        await AzureStorageAPI.CopyDirectoryAsync(src.Directory.FullName, newDir.FullName);
                    }
                    response.Added.Add(await BaseModel.CreateAsync(this, newDir, dest.RootVolume));
                }
                else
                {
                    string newFilePath = AzureStorageAPI.PathCombine(dest.Directory.FullName, src.File.Name);
                    await AzureStorageAPI.DeleteFileIfExistsAsync(newFilePath);

                    if (isCut)
                    {
                        await RemoveThumbsAsync(src);

                        // Move file
                        await AzureStorageAPI.MoveFileAsync(src.File.FullName, newFilePath);

                        response.Removed.Add(src.HashedTarget);
                    }
                    else
                    {
                        // Copy file
                        await AzureStorageAPI.CopyFileAsync(src.File.FullName, newFilePath);
                    }

                    response.Added.Add(await BaseModel.CreateAsync(this, new AzureStorageFile(newFilePath), dest.RootVolume));
                }
            }

            return await Json(response);
        }

        public async Task<JsonResult> PutAsync(FullPath path, string content)
        {
            var response = new ChangedResponseModel();

            // Write content
            await AzureStorageAPI.PutAsync(path.File.FullName, content);

            response.Changed.Add((FileModel)await BaseModel.CreateAsync(this, path.File, path.RootVolume));
            return await Json(response);
        }

        public async Task<JsonResult> RemoveAsync(IEnumerable<FullPath> paths)
        {
            var response = new RemoveResponseModel();

            foreach (FullPath path in paths)
            {
                await RemoveThumbsAsync(path);

                if (path.IsDirectory && await path.Directory.ExistsAsync)
                {
                    await AzureStorageAPI.DeleteDirectoryAsync(path.Directory.FullName);
                }
                else if (await path.File.ExistsAsync)
                {
                    await AzureStorageAPI.DeleteFileAsync(path.File.FullName);
                }

                response.Removed.Add(path.HashedTarget);
            }
            return await Json(response);
        }

        public async Task<JsonResult> RenameAsync(FullPath path, string name)
        {
            var response = new ReplaceResponseModel();
            response.Removed.Add(path.HashedTarget);
            await RemoveThumbsAsync(path);

            if (path.IsDirectory)
            {
                // Get new path
                var newPath = AzureStorageAPI.PathCombine(path.Directory.Parent.FullName, name);

                // Move file
                await AzureStorageAPI.MoveDirectoryAsync(path.Directory.FullName, newPath);

                // Add it to added entries list
                response.Added.Add(await BaseModel.CreateAsync(this, new AzureStorageDirectory(newPath), path.RootVolume));
            }
            else
            {
                // Get new path
                var newPath = AzureStorageAPI.PathCombine(path.File.DirectoryName ?? string.Empty, name);

                // Move file
                await AzureStorageAPI.MoveFileAsync(path.File.FullName, newPath);

                // Add it to added entries list
                response.Added.Add(await BaseModel.CreateAsync(this, new AzureStorageFile(newPath), path.RootVolume));
            }

            return await Json(response);
        }

        public async Task<JsonResult> ResizeAsync(FullPath path, int width, int height)
        {
            await RemoveThumbsAsync(path);

            // Resize Image
            ImageWithMimeType image;
            using (var stream = await path.File.OpenReadAsync())
            {
                image = path.RootVolume.PictureEditor.Resize(stream, width, height);
            }

            await AzureStorageAPI.PutAsync(path.File.FullName, image.ImageStream);

            var output = new ChangedResponseModel();
            output.Changed.Add((FileModel)await BaseModel.CreateAsync(this, path.File, path.RootVolume));
            return await Json(output);
        }

        public async Task<JsonResult> RotateAsync(FullPath path, int degree)
        {
            await RemoveThumbsAsync(path);

            // Crop Image
            ImageWithMimeType image;
            using (var stream = await path.File.OpenReadAsync())
            {
                image = path.RootVolume.PictureEditor.Rotate(stream, degree);
            }

            await AzureStorageAPI.PutAsync(path.File.FullName, image.ImageStream);

            var output = new ChangedResponseModel();
            output.Changed.Add((FileModel)await BaseModel.CreateAsync(this, path.File, path.RootVolume));
            return await Json(output);
        }

        public async Task<JsonResult> SizeAsync(IEnumerable<FullPath> paths)
        {
            var response = new SizeResponseModel();

            foreach (var path in paths)
            {
                if (path.IsDirectory)
                {
                    response.DirectoryCount++; // API counts the current directory in the total

                    var sizeAndCount = await DirectorySizeAndCount(new AzureStorageDirectory(path.Directory.FullName));

                    response.DirectoryCount += sizeAndCount.DirectoryCount;
                    response.FileCount += sizeAndCount.FileCount;
                    response.Size += sizeAndCount.Size;
                }
                else
                {
                    response.FileCount++;
                    response.Size += await path.File.LengthAsync;
                }
            }

            return await Json(response);
        }

        public async Task<JsonResult> ThumbsAsync(IEnumerable<FullPath> paths)
        {
            var response = new ThumbsResponseModel();
            foreach (var path in paths)
            {
                response.Images.Add(path.HashedTarget, await path.RootVolume.GenerateThumbHashAsync(path.File));
                //response.Images.Add(target, path.Root.GenerateThumbHash(path.File) + path.File.Extension); // 2018.02.23: Fix
            }
            return await Json(response);
        }

        public async Task<JsonResult> TreeAsync(FullPath path)
        {
            var response = new TreeResponseModel();

            var items = await AzureStorageAPI.ListFilesAndDirectoriesAsync(path.Directory.FullName);

            // Add visible directories
            foreach (var dir in items.Where(i => i is CloudFileDirectory))
            {
                var d = new AzureStorageDirectory(dir as CloudFileDirectory);
                if (!d.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    response.Tree.Add(await BaseModel.CreateAsync(this, d, path.RootVolume));
                }
            }

            return await Json(response);
        }

        public async Task<JsonResult> UploadAsync(FullPath path, IEnumerable<IFormFile> files, bool? overwrite, IEnumerable<FullPath> uploadPaths, IEnumerable<string> renames, string suffix)
        {
            var response = new AddResponseModel();

            // Check if max upload size is set and that no files exceeds it
            if (path.RootVolume.MaxUploadSize.HasValue && files.Any(x => x.Length > path.RootVolume.MaxUploadSize))
            {
                // Max upload size exceeded
                return Error.MaxUploadFileSize();
            }

            foreach (string rename in renames)
            {
                var fileInfo = new FileInfo(Path.Combine(path.Directory.FullName, rename));
                string destination = Path.Combine(path.Directory.FullName, $"{Path.GetFileNameWithoutExtension(rename)}{suffix}{Path.GetExtension(rename)}");
                fileInfo.MoveTo(destination);
                response.Added.Add((FileModel)await BaseModel.CreateAsync(this, new AzureStorageFile(destination), path.RootVolume));
            }

            foreach (var uploadPath in uploadPaths)
            {
                var dir = uploadPath.Directory;
                while (dir.FullName != path.RootVolume.RootDirectory)
                {
                    response.Added.Add(await BaseModel.CreateAsync(this, new AzureStorageDirectory(dir.FullName), path.RootVolume));
                    dir = dir.Parent;
                }
            }

            var i = 0;
            foreach (var file in files)
            {
                string destination = uploadPaths.Count() > i ? uploadPaths.ElementAt(i).Directory.FullName : path.Directory.FullName;
                var azureFile = new AzureStorageFile(AzureStorageAPI.PathCombine(destination, Path.GetFileName(file.FileName)));

                if (await azureFile.ExistsAsync)
                {
                    if (overwrite ?? path.RootVolume.UploadOverwrite)
                    {
                        await azureFile.DeleteAsync();
                        await AzureStorageAPI.UploadAsync(file, azureFile.FullName);
                        response.Added.Add((FileModel)await BaseModel.CreateAsync(this, new AzureStorageFile(azureFile.FullName), path.RootVolume));
                    }
                    else
                    {
                        var newName = await CreateNameForCopy(azureFile, suffix);
                        await AzureStorageAPI.UploadAsync(file, AzureStorageAPI.PathCombine(azureFile.DirectoryName, newName));
                        response.Added.Add((FileModel)await BaseModel.CreateAsync(this, new AzureStorageFile(newName), path.RootVolume));
                    }
                }
                else
                {
                    await AzureStorageAPI.UploadAsync(file, azureFile.FullName);
                    response.Added.Add((FileModel)await BaseModel.CreateAsync(this, new AzureStorageFile(azureFile.FullName), path.RootVolume));
                }

                i++;
            }
            return await Json(response);
        }

        #endregion IDriver Members

        private async Task<string> CreateNameForCopy(IFile file, string suffix)
        {
            string parentPath = file.DirectoryName;
            string name = Path.GetFileNameWithoutExtension(file.Name);
            string extension = file.Extension;

            for (int i = 1; i < 10; i++)
            {
                var newName = $"{parentPath}/{name}{suffix ?? "-"}{i}{extension}";
                if (!await AzureStorageAPI.FileExistsAsync(newName))
                {
                    return newName;
                }
            }

            return $"{parentPath}/{name}{suffix ?? "-"}{Guid.NewGuid()}{extension}";
        }

        private async Task<SizeResponseModel> DirectorySizeAndCount(IDirectory d)
        {
            var response = new SizeResponseModel();

            // Add file sizes.
            foreach (var file in await d.GetFilesAsync())
            {
                response.FileCount++;
                response.Size += await file.LengthAsync;
            }

            // Add subdirectory sizes.
            foreach (var directory in await d.GetDirectoriesAsync())
            {
                response.DirectoryCount++;

                var subdir = await DirectorySizeAndCount(directory);
                response.DirectoryCount += subdir.DirectoryCount;
                response.FileCount += subdir.FileCount;
                response.Size += subdir.Size;
            }

            return response;
        }

        private async Task RemoveThumbsAsync(FullPath path)
        {
            if (path.IsDirectory)
            {
                string thumbPath = path.RootVolume.GenerateThumbPath(path.Directory);

                if (thumbPath == null)
                {
                    return;
                }

                await AzureStorageAPI.DeleteDirectoryIfExistsAsync(thumbPath);
            }
            else
            {
                string thumbPath = await path.RootVolume.GenerateThumbPathAsync(path.File);

                if (thumbPath == null)
                {
                    return;
                }

                await AzureStorageAPI.DeleteFileIfExistsAsync(thumbPath);
            }
        }
    }
}
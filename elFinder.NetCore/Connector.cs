using elFinder.NetCore.Drawing;
using elFinder.NetCore.Drivers;
using elFinder.NetCore.Extensions;
using elFinder.NetCore.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

		public async Task<IActionResult> ProcessAsync(HttpRequest request)
		{
			var parameters = request.Query.Any() ? request.Query.ToDictionary(k => k.Key, v => v.Value) : request.Form.ToDictionary(k => k.Key, v => v.Value);

			string cmd = parameters.GetValueOrDefault("cmd");
			if (string.IsNullOrEmpty(cmd)) return Error.CommandNotFound();

			switch (cmd)
			{
				case "dim":
				{
					var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
					return await driver.DimAsync(path);
				}
				case "duplicate":
				{
					var targets = await GetFullPathArrayAsync(parameters.GetValueOrDefault("targets[]"));
					return await driver.DuplicateAsync(targets);
				}
				case "file":
				{
					var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
					return await driver.FileAsync(path, parameters.GetValueOrDefault("download") == "1");
				}
				case "get":
				{
					var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
					return await driver.GetAsync(path);
				}
				case "ls":
				{
					var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
					return await driver.ListAsync(path, parameters.GetValueOrDefault("intersect[]"));
				}
				case "mkdir":
				{
					var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
					var name = parameters.GetValueOrDefault("name");
					var dirs = parameters.GetValueOrDefault("dirs[]");
					return await driver.MakeDirAsync(path, name, dirs);
				}
				case "mkfile":
				{
					var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
					var name = parameters.GetValueOrDefault("name");
					return await driver.MakeFileAsync(path, name);
				}
				case "open":
				{
					var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));

					if (parameters.GetValueOrDefault("init") == "1")
					{
						return await driver.InitAsync(path);
					}
					else
					{
						return await driver.OpenAsync(path, parameters.GetValueOrDefault("tree") == "1");
					}
				}
				case "parents":
				{
					var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
					//var until = parameters.GetValueOrDefault("until");
					return await driver.ParentsAsync(path);
				}
				case "paste":
				{
					var paths = await GetFullPathArrayAsync(parameters.GetValueOrDefault("targets[]"));
					var dst = parameters.GetValueOrDefault("dst");
					var cut = parameters.GetValueOrDefault("cut") == "1";
					var renames = parameters.GetValueOrDefault("ranames[]");
					var suffix = parameters.GetValueOrDefault("suffix");
					return await driver.PasteAsync(await driver.ParsePathAsync(dst), paths, cut, renames, suffix);
				}
				case "put":
				{
					var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
					var content = parameters.GetValueOrDefault("content");
					return await driver.PutAsync(path, content);
				}
				case "rename":
				{
					var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
					var name = parameters.GetValueOrDefault("name");
					return await driver.RenameAsync(path, name);
				}
				case "resize":
				{
					var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
					switch (parameters.GetValueOrDefault("mode"))
					{
						case "resize":
						return await driver.ResizeAsync(
							path,
							int.Parse(parameters.GetValueOrDefault("width")),
							int.Parse(parameters.GetValueOrDefault("height")));

						case "crop":
						return await driver.CropAsync(
							path,
							int.Parse(parameters.GetValueOrDefault("x")),
							int.Parse(parameters.GetValueOrDefault("y")),
							int.Parse(parameters.GetValueOrDefault("width")),
							int.Parse(parameters.GetValueOrDefault("height")));

						case "rotate":
						return await driver.RotateAsync(path, int.Parse(parameters.GetValueOrDefault("degree")));

						default:
						return Error.CommandNotFound();
					}
				}
				case "rm":
				{
					var paths = await GetFullPathArrayAsync(parameters.GetValueOrDefault("targets[]"));
					return await driver.RemoveAsync(paths);
				}
				case "size":
				{
					var paths = new StringValues(parameters.Where(p => p.Key.StartsWith("target")).Select(p => (string)p.Value).ToArray());
					return await driver.SizeAsync(await GetFullPathArrayAsync(paths));
				}
				case "tmb":
				{
					var targets = await GetFullPathArrayAsync(parameters.GetValueOrDefault("targets[]"));
					return await driver.ThumbsAsync(targets);
				}
				case "tree":
				{
					var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
					return await driver.TreeAsync(path);
				}
				case "upload":
				{
					var path = await driver.ParsePathAsync(parameters.GetValueOrDefault("target"));
					var uploadPath = await GetFullPathArrayAsync(parameters.GetValueOrDefault("upload_path[]"));
					var overwrite = parameters.GetValueOrDefault("overwrite") == "1";
					var renames = parameters.GetValueOrDefault("renames[]");
					var suffix = parameters.GetValueOrDefault("suffix");
					return await driver.UploadAsync(path, request.Form.Files, overwrite, uploadPath, renames, suffix);
				}

				default: return Error.CommandNotFound();
			}
		}

		/// <summary>
		/// Get actual filesystem path by hash
		/// </summary>
		/// <param name="hash">Hash of file or directory</param>
		public async Task<IFile> GetFileByHashAsync(string hash)
		{
			var path = await driver.ParsePathAsync(hash);
			return !path.IsDirectory ? path.File : null;
		}

		public async Task<IActionResult> GetThumbnailAsync(HttpRequest request, HttpResponse response, string hash)
		{
			if (hash != null)
			{
				var path = await driver.ParsePathAsync(hash);
				if (!path.IsDirectory && CanCreateThumbnail(path, path.RootVolume.PictureEditor))
				{
					//if (!await HttpCacheHelper.IsFileFromCache(path.File, request, response))
					//{
					var thumb = await path.GenerateThumbnailAsync();
					return new FileStreamResult(thumb.ImageStream, thumb.MimeType);
					//}
					//else
					//{
					//	response.ContentType = Utils.GetMimeType(path.RootVolume.PictureEditor.ConvertThumbnailExtension(path.File.Extension));
					//response.End();
					//}
				}
			}
			return new EmptyResult();
		}

		private bool CanCreateThumbnail(FullPath path, IPictureEditor pictureEditor)
		{
			return !string.IsNullOrEmpty(path.RootVolume.ThumbnailUrl) && pictureEditor.CanProcessFile(path.File.Extension);
		}

		private async Task<IEnumerable<FullPath>> GetFullPathArrayAsync(StringValues targets)
		{
			var tasks = targets.Select(async t => await driver.ParsePathAsync(t));
			return await Task.WhenAll(tasks);
		}
	}
}
using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore.Helpers
{
    // For a list of all possible error messages: https://github.com/Studio-42/elFinder/blob/master/js/i18n/elfinder.en.js

    public static class Error
    {
        /// <summary>
        /// Access denied.
        /// </summary>
        /// <returns></returns>
        public static JsonResult AccessDenied()
        {
            return FormatSimpleError("errAccess");
        }

        /// <summary>
        /// File not found.
        /// </summary>
        /// <returns></returns>
        public static JsonResult FileNotFound()
        {
            return FormatSimpleError("errFileNotFound");
        }

        /// <summary>
        /// File type not allowed.
        /// </summary>
        /// <returns></returns>
        public static JsonResult FileTypeNotAllowed()
        {
            return FormatSimpleError("errUploadMime");
        }

        /// <summary>
        /// Folder not found.
        /// </summary>
        /// <returns></returns>
        public static JsonResult FolderNotFound()
        {
            return FormatSimpleError("errFolderNotFound");
        }

        /// <summary>
        /// Invalid parameters for command "$1".
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static JsonResult InvalidCommandParams(string command)
        {
            return new JsonResult(new { error = new string[] { "errCmdParams", command } });
        }

        // NOTE: This is a custom error (not out-of-the-box) defined on client side.
        /// <summary>
        /// Unable to create new file with name "$1"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static JsonResult NewNameSelectionException(string name)
        {
            return new JsonResult(new
            {
                error = new[] { "errNewNameSelection", name }
            });
        }

        /// <summary>
        /// Unable to upload "$1".
        /// </summary>
        /// <returns></returns>
        public static JsonResult UnableToUpload()
        {
            return FormatSimpleError("errUploadFile");
        }

        /// <summary>
        /// Unknown command.
        /// </summary>
        /// <returns></returns>
        public static JsonResult UnknownCommand()
        {
            return FormatSimpleError("errUnknownCmd");
        }

        /// <summary>
        /// File exceeds maximum allowed size.
        /// </summary>
        /// <returns></returns>
        public static JsonResult UploadFileTooLarge()
        {
            return FormatSimpleError("errUploadFileSize"); // old name - errFileMaxSize
        }

        private static JsonResult FormatSimpleError(string message)
        {
            return new JsonResult(new { error = message });
        }
    }
}
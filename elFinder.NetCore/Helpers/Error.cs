using Microsoft.AspNetCore.Mvc;

namespace elFinder.NetCore.Helpers
{
    public static class Error
    {
        public static JsonResult AccessDenied()
        {
            return FormatSimpleError("errAccess");
        }

        public static JsonResult CannotUploadFile()
        {
            return FormatSimpleError("errUploadFile");
        }

        public static JsonResult CommandNotFound()
        {
            return FormatSimpleError("errUnknownCmd");
        }

        public static JsonResult FileNotFound()
        {
            return FormatSimpleError("errFileNotFound");
        }

        public static JsonResult FolderNotFound()
        {
            return FormatSimpleError("errFolderNotFound");
        }

        public static JsonResult MaxUploadFileSize()
        {
            return FormatSimpleError("errFileMaxSize");
        }

        public static JsonResult MissedParameter(string command)
        {
            return new JsonResult(new { error = new string[] { "errCmdParams", command } });
        }

        public static JsonResult NewNameSelectionException(string name)
        {
            return new JsonResult(new
            {
                error = new[] { "errNewNameSelection", name }
            });
        }

        private static JsonResult FormatSimpleError(string message)
        {
            return new JsonResult(new { error = message });
        }
    }
}
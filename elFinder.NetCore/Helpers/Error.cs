using System;

namespace elFinder.NetCore.Helpers
{
    public static class Error
    {
        public static object AccessDenied()
        {
            return FormatSimpleError("errAccess");
        }

        public static object CannotUploadFile()
        {
            return FormatSimpleError("errUploadFile");
        }

        public static object CommandNotFound()
        {
            return FormatSimpleError("errUnknownCmd");
        }

        public static object MaxUploadFileSize()
        {
            return FormatSimpleError("errFileMaxSize");
        }

        public static object MissedParameter(string command)
        {
            return new { error = new string[] { "errCmdParams", command } };
        }

        public static object NewNameSelectionException(string name)
        {
            return new { error = $"Unable to create new file with name {name}" };
        }

        private static object FormatSimpleError(string message)
        {
            return new { error = message };
        }
    }
}
namespace elFinder.NetCore.Helpers;

// For a list of all possible error messages: https://github.com/Studio-42/elFinder/blob/master/js/i18n/elfinder.en.js

public static class Error
{
    /// <summary>
    /// Access denied.
    /// </summary>
    /// <returns></returns>
    public static JsonResult AccessDenied() => FormatSimpleError("errAccess");

    /// <summary>
    /// File not found.
    /// </summary>
    /// <returns></returns>
    public static JsonResult FileNotFound() => FormatSimpleError("errFileNotFound");

    /// <summary>
    /// File type not allowed.
    /// </summary>
    /// <returns></returns>
    public static JsonResult FileTypeNotAllowed() => FormatSimpleError("errUploadMime");

    /// <summary>
    /// Folder not found.
    /// </summary>
    /// <returns></returns>
    public static JsonResult FolderNotFound() => FormatSimpleError("errFolderNotFound");

    /// <summary>
    /// Invalid parameters for command "$1".
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public static JsonResult InvalidCommandParams(string command) => new(new
    {
        error = new string[] { "errCmdParams", command }
    });

    // NOTE: This is a custom error (not out-of-the-box) defined on client side.
    /// <summary>
    /// Unable to create new file with name "$1"
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static JsonResult NewNameSelectionException(string name) => new(new
    {
        error = new[] { "errNewNameSelection", name }
    });

    /// <summary>
    /// Unable to upload "$1".
    /// </summary>
    /// <returns></returns>
    public static JsonResult UnableToUpload() => FormatSimpleError("errUploadFile");

    /// <summary>
    /// Unknown command.
    /// </summary>
    /// <returns></returns>
    public static JsonResult UnknownCommand() => FormatSimpleError("errUnknownCmd");

    /// <summary>
    /// File exceeds maximum allowed size.
    /// </summary>
    /// <returns></returns>
    public static JsonResult UploadFileTooLarge() => FormatSimpleError("errUploadFileSize"); // old name - errFileMaxSize

    private static JsonResult FormatSimpleError(string message) => new(new { error = message });
}
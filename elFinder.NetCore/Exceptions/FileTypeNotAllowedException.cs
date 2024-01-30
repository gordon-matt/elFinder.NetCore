namespace elFinder.NetCore.Exceptions;

public class FileTypeNotAllowedException : ApplicationException
{
    public FileTypeNotAllowedException()
    {
    }

    public FileTypeNotAllowedException(string message)
        : base(message)
    {
    }

    public FileTypeNotAllowedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
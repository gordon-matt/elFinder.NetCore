﻿namespace elFinder.NetCore.Exceptions;

public class InvalidPathException : ApplicationException
{
    public InvalidPathException()
    {
    }

    public InvalidPathException(string message)
        : base(message)
    {
    }

    public InvalidPathException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
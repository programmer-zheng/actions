namespace System;

public class CustomException : Exception
{
    public CustomException(int code, string message) : base(message)
    {
        Code = code;
    }

    public int Code { get; set; }
}
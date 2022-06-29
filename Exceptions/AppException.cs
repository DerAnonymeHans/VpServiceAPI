using System;

namespace VpServiceAPI.Exceptions
{
    public class AppException : Exception
    {
        public AppException(string text) : base(text)
        {

        }
    }
}

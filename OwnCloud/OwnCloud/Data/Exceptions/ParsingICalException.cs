using System;

namespace OwnCloud.Data.Exceptions
{
    public class ParsingICalException : Exception
    {
        public ParsingICalException() : base("Unexpected  ICal value")
        {
            
        }
    }
}

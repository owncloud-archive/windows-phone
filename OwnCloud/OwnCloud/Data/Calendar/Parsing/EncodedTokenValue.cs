using System;

namespace OwnCloud.Data.Calendar.Parsing
{
    /// <summary>
    /// Represents a value, which can be accessed as Encoded ICAL Value or as decoded readable text.
    /// </summary>
    public class EncodedTokenValue
    {
        private string _encodedValue = "";
        private string _decodedValue = "";

        /// <summary>
        /// The encoded ical value
        /// </summary>
        public string EncodedValue
        {
            get { return _encodedValue; }
            set { _encodedValue = value;
                _decodedValue = DecodeValue(value);
            }
        }

        /// <summary>
        /// The readable clear Text
        /// </summary>
        public string DecodedValue
        {
            get { return _decodedValue; }
            set { _decodedValue = value;
                _encodedValue = EncodeValue(value);
            }
        }


        private static string DecodeValue(string value)
        {
            return value.Replace("\\n", Environment.NewLine)
                    .Replace("\\,", ",")
                    .Replace("\\;", ";")
                    .Replace("\\\\", "\\");
        }
        private static string EncodeValue(string value)
        {
            return value.Replace(Environment.NewLine, "\\n")
                   .Replace(",", "\\,")
                   .Replace(";", "\\;")
                   .Replace("\\", "\\\\");
        }

    }
}

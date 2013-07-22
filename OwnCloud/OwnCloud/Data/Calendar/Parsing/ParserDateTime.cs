using System;
using System.Text.RegularExpressions;

namespace OwnCloud.Data.Calendar.Parsing
{
    static class ParserDateTime
    {
        public static DateTime? Parse(string value, out bool isFullDayTime)
        {
            var dateRegex = new Regex(@"(\d{4})(\d{2})(\d{2})T(\d{2})(\d{2})(\d{2})(.*)");

            var match = dateRegex.Match(value);

            if (match.Success)
            {
                var result = new DateTime
                    (
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value),
                    int.Parse(match.Groups[4].Value),
                    int.Parse(match.Groups[5].Value),
                    int.Parse(match.Groups[6].Value)
                    );
                isFullDayTime = false;
                return result;
            }

            dateRegex = new Regex(@"(\d{4})(\d{2})(\d{2})");
            match = dateRegex.Match(value);
            if (match.Success)
            {
                var result = new DateTime
                    (
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value)
                    );
                isFullDayTime = true;
                return result;
            }

            isFullDayTime = true;
            return null;
        }
    }
}

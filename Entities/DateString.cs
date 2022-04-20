using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archimedes.Entities
{
    /// <summary>
    /// Object representing a date created from an 8 byte string
    /// </summary>
    public class DateString
    {
        string format = "yyyyMMdd";

        public DateTime DateTime;

        public DateString(string value)
        {
            int year = 1990;
            int month = 1;
            int day = 1;

            if (!int.TryParse(value.Substring(0, 4), out year))
                throw new Exception("Failed to parse the year from the provided date string");

            if (!int.TryParse(value.Substring(4, 2), out month))
                throw new Exception("Failed to parse the month from the provided date string");

            if (!int.TryParse(value.Substring(6, 2), out day))
                throw new Exception("Failed to parse the day from the provided date string");

            DateTime = new DateTime(year, month, day);
        }

        public string ToString(string format)
        {
            if (format == null)
                return null;

            this.format = format;

            return ToString();
        }

        public override string ToString() => DateTime.ToString(format);
    }

}

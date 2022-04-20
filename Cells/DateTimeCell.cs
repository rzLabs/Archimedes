using Archimedes.Abstractions;
using Archimedes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archimedes.Cells
{
    /// <summary>
    /// Cell representing a 4 byte <b>Datetime</b> value by calculating seconds from epoch
    /// </summary>
    public class DateTimeCell : CellBase, ICellIO, ICloneable
    {
        public DateTimeCell(string name) : base(name, typeof(DateTime), ArcType.NONE) { }

        public object Read()
        {
            byte[] buffer = new byte[4];

            Stream.Read(buffer, 0, buffer.Length);

            var secFromEpoch = BitConverter.ToInt32(buffer, 0);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return epoch.AddSeconds(secFromEpoch);
        }

        public void Write(object value)
        {
            DateTime dt = (DateTime)value;
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var seconds = Convert.ToInt32((dt - epoch).TotalSeconds);

            byte[] buffer = BitConverter.GetBytes(seconds);

            Stream.Write(buffer, 0, buffer.Length);
        }

        public object Clone()
        {
            DateTimeCell cell = new DateTimeCell(Name);

            cell.Index = Index;
            cell.Flags = Flags;
            cell.Offset = Offset;
            cell.PrimaryType = PrimaryType;
            cell.SecondaryType = SecondaryType;

            return cell;
        }
    }
}

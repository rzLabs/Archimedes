using Archimedes.Abstractions;
using Archimedes.Enums;
using Archimedes.Utilities;
using Archimedes.Entities;
using System;

namespace Archimedes.Cells
{
    /// <summary>
    /// Cell representing an 8 byte <b>DateString</b> value
    /// </summary>
    public class DateStringCell : CellBase, ICellIO, ICloneable
    {
        public DateStringCell(string name) : base(name, typeof(DateString), ArcType.TYPE_DATESTRING)
        {
            base.Length = 8;
        }

        public DateStringCell(string name, CellFlags flags) : base(name, typeof(DateString), ArcType.TYPE_DATESTRING)
        {
            base.Length = 8;
            base.Flags = flags;
        }

        public object Read()
        {
            if (Length != 8)
                throw new ArgumentOutOfRangeException("DateStringObject Length must be 8!");

            byte[] buffer = new byte[Length];

            Stream.Read(buffer, 0, buffer.Length);

            string dateStr = ByteUtility.ToString(buffer);

            return new DateString(dateStr);
        }

        public void Write(object value)
        {
            DateString val = value as DateString;

            string valStr = val.ToString("yyyyMMdd");

            byte[] buffer = ByteUtility.ToBytes(valStr);

            Stream.Write(buffer, 0, buffer.Length);
        }

        public object Clone()
        {
            DateStringCell cell = new DateStringCell(Name);

            cell.Index = Index;
            cell.Flags = Flags;
            cell.Offset = Offset;
            cell.PrimaryType = PrimaryType;
            cell.SecondaryType = SecondaryType;

            return cell;
        }
    }
}

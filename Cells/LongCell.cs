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
    /// Cell representing an 8 byte <b>long</b> or <b>Int64</b> value
    /// </summary>
    public class LongCell : CellBase, ICellIO, ICloneable
    {
        public LongCell(string name) : base(name, typeof(long), ArcType.TYPE_LONG) { }

        public LongCell(string name, CellFlags flags) : base(name, typeof(long), ArcType.TYPE_LONG)
        {
            Flags = flags;
        }

        public object Read()
        {
            byte[] buffer = new byte[8];

            Stream.Read(buffer, 0, buffer.Length);

            return BitConverter.ToInt64(buffer, 0);
        }

        public void Write(object value)
        {
            long val = (long)value;

            byte[] buffer = BitConverter.GetBytes(val);

            Stream.Write(buffer, 0, buffer.Length);
        }

        public object Clone()
        {
            LongCell cell = new LongCell(Name);

            cell.Index = Index;
            cell.Flags = Flags;
            cell.Offset = Offset;
            cell.PrimaryType = PrimaryType;
            cell.SecondaryType = SecondaryType;

            return cell;
        }
    }
}

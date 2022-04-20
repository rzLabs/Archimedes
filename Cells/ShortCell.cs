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
    /// Cell representing a short or int16 value
    /// </summary>
    public class ShortCell : CellBase, ICellIO, ICloneable
    {
        public ShortCell(string name) : base(name, typeof(short), ArcType.TYPE_SHORT) { }

        public ShortCell(string name, CellFlags flags) : base(name, typeof(short), ArcType.TYPE_SHORT)
        {
            Flags = flags;
        }

        public object Read()
        {
            byte[] buffer = new byte[2];

            Stream.Read(buffer, 0, buffer.Length);

            return BitConverter.ToInt16(buffer, 0);
        }

        public void Write(object value)
        {
            short val = Convert.ToInt16(value);

            byte[] buffer = BitConverter.GetBytes(val);

            Stream.Write(buffer, 0, buffer.Length);
        }

        public object Clone()
        {
            ShortCell cell = new ShortCell(Name);

            cell.Index = Index;
            cell.Flags = Flags;
            cell.Offset = Offset;
            cell.PrimaryType = PrimaryType;
            cell.SecondaryType = SecondaryType;

            return cell;
        }
    }
}

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
    /// Cell representing a byte value
    /// </summary>
    public class ByteCell : CellBase, ICellIO, ICloneable
    {
        public ByteCell(string name) : base(name, typeof(byte), ArcType.TYPE_BYTE)
        {
        }

        public ByteCell(string name, CellFlags flags) : base(name, typeof(byte), ArcType.TYPE_BYTE)
        {
            base.Flags = flags;
        }

        public object Read() => (byte)Stream.ReadByte();

        public void Write(object value)
        {
            byte val = Convert.ToByte(value);

            byte[] buffer = new byte[1] { val };

            Stream.Write(buffer, 0, buffer.Length);
        }

        public object Clone()
        {
            ByteCell cell = new ByteCell(Name);

            cell.Index = Index;
            cell.Flags = Flags;
            cell.Length = Length;
            cell.Offset = Offset;
            cell.PrimaryType = PrimaryType;
            cell.SecondaryType = SecondaryType;

            return cell;
        }
    }
}

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
    /// Cell representing a collection of bytes
    /// </summary>
    public class ByteArrayCell : CellBase, ICellIO, ICloneable
    {
        public ByteArrayCell(string name, int length) : base(name, typeof(byte[]), ArcType.TYPE_BYTE_ARRAY)
        {
            if (length == -1)
                throw new ArgumentOutOfRangeException("Cannot construct ByteArrayCell, length is invalid!");

            Length = length;
        }

        public ByteArrayCell(string name, int length, CellFlags flags) : base(name, typeof(byte[]), ArcType.TYPE_BYTE_ARRAY)
        {
            Flags = flags;

            if (length == -1)
                throw new ArgumentOutOfRangeException("Cannot construct ByteArrayCell, length is invalid!");

            Length = length;
        }

        public object Read()
        {
            if (Length == -1)
                throw new ArgumentOutOfRangeException("Cannot read ByteArrayCell, length is invalid!");

            byte[] value = new byte[Length];

            Stream.Read(value, 0, value.Length);

            return value;
        }

        public void Write(object value)
        {
            byte[] buffer = (byte[])value;

            Stream.Write(buffer, 0, buffer.Length);
        }

        public object Clone()
        {
            ByteArrayCell cell = new ByteArrayCell(Name, Length);

            cell.Index = Index;
            cell.Flags = Flags;
            cell.Offset = Offset;
            cell.PrimaryType = PrimaryType;
            cell.SecondaryType = SecondaryType;

            return cell;
        }
    }
}

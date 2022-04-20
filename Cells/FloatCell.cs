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
    /// Cell representing a 4 byte <b>single</b> / <b>float</b> value
    /// </summary>
    public class FloatCell : CellBase, ICellIO, ICloneable
    {
        public FloatCell(string name) : base(name, typeof(float), ArcType.TYPE_FLOAT) { }

        public object Read()
        {
            byte[] buffer = new byte[4];

            Stream.Read(buffer, 0, buffer.Length);

            return BitConverter.ToSingle(buffer, 0);
        }

        public void Write(object value)
        {
            float val = Convert.ToSingle(value);

            byte[] buffer = BitConverter.GetBytes(val);

            Stream.Write(buffer, 0, buffer.Length);
        }

        public object Clone()
        {
            FloatCell cell = new FloatCell(Name);

            cell.Index = Index;
            cell.Flags = Flags;
            cell.Offset = Offset;
            cell.PrimaryType = PrimaryType;
            cell.SecondaryType = SecondaryType;

            return cell;
        }
    }
}

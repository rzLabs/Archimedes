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
    /// Cell representing an 8 byte <b>double</b> value
    /// </summary>
    public class DoubleCell : CellBase, ICellIO, ICloneable
    {
        public DoubleCell(string name) : base(name, typeof(double), ArcType.TYPE_DOUBLE) { }

        public DoubleCell(string name, CellFlags flags) : base(name, typeof(double), ArcType.TYPE_DOUBLE)
        {
            Flags = flags;
        }

        public object Read()
        {
            byte[] buffer = new byte[8];

            Stream.Read(buffer, 0, buffer.Length);

            return BitConverter.ToDouble(buffer, 0);
        }

        public void Write(object value)
        {
            double val = Convert.ToDouble(value);

            byte[] buffer = BitConverter.GetBytes(val);

            Stream.Write(buffer, 0, buffer.Length);
        }

        public object Clone()
        {
            DoubleCell cell = new DoubleCell(Name);

            cell.Index = Index;
            cell.Flags = Flags;
            cell.Offset = Offset;
            cell.PrimaryType = PrimaryType;
            cell.SecondaryType = SecondaryType;

            return cell;
        }
    }
}

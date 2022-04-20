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
    /// Cell representing a <b>4</b> byte <b>int</b> / <b>Int32</b> value
    /// </summary>
    public class IntCell : CellBase, ICellIO, ICloneable
    {
        public IntCell(string name) : base(name, typeof(int), ArcType.TYPE_INT32)
        {
        }

        public IntCell(string name, CellFlags flags) : base(name, typeof(int), ArcType.TYPE_INT32)
        {
            base.Flags = flags;
        }

        public object Read()
        {
            byte[] buffer = new byte[4];

            Stream.Read(buffer, 0, buffer.Length);

            return BitConverter.ToInt32(buffer, 0);
        }

        public void Write(object value)
        {
            int val = Convert.ToInt32(value);

            byte[] buffer = BitConverter.GetBytes(val);

            Stream.Write(buffer, 0, buffer.Length);
        }

        public object Clone()
        {
            IntCell cell = new IntCell(Name);

            cell.Index = Index;
            cell.Flags = Flags;
            cell.Offset = Offset;
            cell.PrimaryType = PrimaryType;
            cell.SecondaryType = SecondaryType;

            return cell;
        }
    }
}

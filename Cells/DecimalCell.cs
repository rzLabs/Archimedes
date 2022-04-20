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
    /// Cell representing a decimal value
    /// </summary>
    public class DecimalCell : CellBase, ICellIO, ICloneable
    {
        public DecimalCell(string name) : base(name, typeof(decimal), ArcType.TYPE_DECIMAL) { }

        public DecimalCell(string name, CellFlags flags) : base(name, typeof(decimal), ArcType.TYPE_DECIMAL)
        {
            Flags = flags;
        }

        public object Read()
        {
            byte[] buffer = new byte[4];

            Stream.Read(buffer, 0, buffer.Length);

            int nVal = BitConverter.ToInt32(buffer, 0);
            decimal dVal = nVal / 100m;

            return dVal;
        }

        public void Write(object value)
        {
            decimal val = (decimal)value;
            int nVal = Convert.ToInt32(val * 100);

            byte[] buffer = BitConverter.GetBytes(nVal);

            Stream.Write(buffer, 0, buffer.Length);
        }

        public object Clone()
        {
            DecimalCell cell = new DecimalCell(Name);

            cell.Index = Index;
            cell.Flags = Flags;
            cell.Offset = Offset;
            cell.PrimaryType = PrimaryType;
            cell.SecondaryType = SecondaryType;

            return cell;
        }
    }
}

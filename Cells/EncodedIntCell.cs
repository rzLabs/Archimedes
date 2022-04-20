using Archimedes.Abstractions;
using Archimedes.Enums;
using Archimedes.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archimedes.Cells
{
    /// <summary>
    /// Cell representing a 4 byte encoded <b>int</b> value
    /// </summary>
    public class EncodedIntCell : CellBase, ICellIO, ICloneable
    {
        public EncodedIntCell(string name) : base(name, typeof(int), ArcType.TYPE_ENCODED_INT32) { }

        public object Read()
        {
            byte[] buffer = new byte[4];

            Stream.Read(buffer, 0, buffer.Length);

            int encodedVal = BitConverter.ToInt32(buffer, 0);

            return ScrambleMap.Restore(encodedVal);
        }

        public void Write(object value)
        {
            int val = Convert.ToInt32(value);
            int encVal = ScrambleMap.Scramble(val);

            byte[] buffer = BitConverter.GetBytes(encVal);

            Stream.Write(buffer, 0, buffer.Length);
        }

        public object Clone()
        {
            EncodedIntCell cell = new EncodedIntCell(Name);

            cell.Index = Index;
            cell.Flags = Flags;
            cell.Offset = Offset;
            cell.PrimaryType = PrimaryType;
            cell.SecondaryType = SecondaryType;

            return cell;
        }
    }
}

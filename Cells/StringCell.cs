using Archimedes.Abstractions;
using Archimedes.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archimedes.Cells
{
    /// <summary>
    /// Cell representing a variable length string value
    /// </summary>
    public class StringCell : CellBase, ICellIO, ICloneable
    {
        public StringCell(string name, int length = -1, ArcType secondaryType = ArcType.TYPE_STRING, string dependency = null) : base(name, typeof(string), secondaryType)
        {
            Dependency = dependency;
            Length = length;
        }

        public object Read()
        {
            long begOffset = Stream.Position;
            long strLen = 0;

            // Read only til we see a null character, then record the offset and reset the position.
            // This will allow us to only read useful data
            for (int i = 0; i < Length; i++)
            {
                if (Stream.ReadByte() == 0)
                {
                    strLen = (Stream.Position - begOffset) - 1;
                    break;
                }
            }

            // Calculate the amount of space we need to advance the stream
            long remainder = Length - strLen;

            // Reset the stream to read the string
            Stream.Seek(begOffset, SeekOrigin.Begin);

            byte[] buffer = new byte[strLen];

            Stream.Read(buffer, 0, buffer.Length);

            Stream.Seek(remainder, SeekOrigin.Current);

            // We will not encode the string data at this time for performance reasons. 
            // Let the calling application determine culture and encode
            return buffer;
        }

        public void Write(object value)
        {
            // Resize the input byte array so we can set the last character as a null
            byte[] buffer = (byte[])value;
            byte[] outBuffer = new byte[buffer.Length + 1];

            Buffer.BlockCopy(buffer, 0, outBuffer, 0, buffer.Length);

            // Set the last character to a null
            outBuffer[outBuffer.Length - 1] = (byte)'\0';

            Stream.Write(outBuffer, 0, outBuffer.Length);

            // If this cell is of the secondary type string, we must pad out the rest of the cell length with 0x00
            if (SecondaryType == ArcType.TYPE_STRING)
            {
                int remainder = Length - outBuffer.Length;

                if (remainder > 0)
                {
                    buffer = new byte[remainder];

                    Stream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        public object Clone()
        {
            StringCell cell = new StringCell(Name);

            cell.Index = Index;
            cell.Flags = Flags;
            cell.Offset = Offset;
            cell.PrimaryType = PrimaryType;
            cell.SecondaryType = SecondaryType;

            return cell;
        }
    }
}

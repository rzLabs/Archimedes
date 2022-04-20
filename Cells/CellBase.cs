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
    /// Base cell object describing a section of data in a given rdb
    /// </summary>
    public class CellBase
    {
        /// <summary>
        /// Stream object used to read/write data to/from file buffer.
        /// </summary>
        public MemoryStream Stream = null;

        private int index = -1;

        /// <summary>
        /// Index this Cell occupies in its parent row object (<b>immutable once set)</b>
        /// </summary>
        public int Index
        {
            get => index;
            set
            {
                if (index == -1)
                    index = value;
            }
        }

        /// <summary>
        /// Name of this cell
        /// </summary>
        public string Name;

        /// <summary>
        /// Length (in bytes) of this cell
        /// </summary>
        public int Length { get; set; } = -1;

        /// <summary>
        /// Offset of this cells value <b>(only used for ArcType.BIT_FROM_VECTOR!)</b>
        /// </summary>
        public int Offset;

        /// <summary>
        /// Primary type of the actual data this cell is linked to.
        /// </summary>
        public Type PrimaryType { get; set; } = typeof(object);

        /// <summary>
        /// Secondary type of this cell used in engine operations
        /// </summary>
        public ArcType SecondaryType { get; set; } = ArcType.NONE;

        /// <summary>
        /// Cell flags that may alter the behavior of the engine
        /// </summary>
        public CellFlags Flags { get; set; } = CellFlags.None;

        /// <summary>
        /// Cell whose value this cell depends on to be processed properly
        /// </summary>
        public string Dependency;

        /// <summary>
        /// Construct a new instance of the base cell descriptor class
        /// </summary>
        /// <param name="name">Name of the cell being created</param>
        /// <param name="type">Primary data type of the created cell</param>
        /// <param name="secondaryType">Secondary engine type of the created cell</param>
        public CellBase(string name, Type type, ArcType secondaryType)
        {
            Name = name;
            PrimaryType = type;
            SecondaryType = secondaryType;
            Length = defaultLength(secondaryType);
        }

        /// <summary>
        /// Get the default length of a given LuaType
        /// </summary>
        /// <param name="type">LuaType loaded from structure lua</param>
        /// <returns>Length <b>(in bytes)</b></returns>
        int defaultLength(ArcType type)
        {
            switch (type)
            {
                case ArcType.TYPE_BYTE:
                case ArcType.TYPE_BIT_FROM_VECTOR:
                    return 1;

                case ArcType.TYPE_SHORT:
                case ArcType.TYPE_USHORT:
                case ArcType.TYPE_INT16:
                case ArcType.TYPE_UINT_16:
                    return 2;

                case ArcType.TYPE_INT:
                case ArcType.TYPE_UINT:
                case ArcType.TYPE_INT32:
                case ArcType.TYPE_UINT32:
                case ArcType.TYPE_BIT_VECTOR:
                case ArcType.TYPE_SID:
                case ArcType.TYPE_STRING_LEN:
                case ArcType.TYPE_SINGLE:
                case ArcType.TYPE_FLOAT:
                case ArcType.TYPE_FLOAT32:
                case ArcType.TYPE_DECIMAL:
                    return 4;

                case ArcType.TYPE_LONG:
                case ArcType.TYPE_INT64:
                case ArcType.TYPE_DOUBLE:
                case ArcType.TYPE_DATESTRING:
                    return 8;

                case ArcType.TYPE_BYTE_ARRAY:
                case ArcType.TYPE_STRING:
                case ArcType.TYPE_STRING_BY_HEADER_REF:
                case ArcType.TYPE_STRING_BY_LEN:
                case ArcType.TYPE_STRING_BY_REF:
                    return -1;

                default:
                    return -1;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archimedes.Enums
{
    /// <summary>
    /// Secondary type which determines specific engine behaviors applied against primary object types.
    /// </summary>
    public enum ArcType
    {
        TYPE_BYTE = 0,
        TYPE_BYTE_ARRAY = 1,
        TYPE_BIT_VECTOR = 2,
        TYPE_BIT_FROM_VECTOR = 3,
        TYPE_INT16 = 4,
        TYPE_SHORT = 5,
        TYPE_UINT_16 = 6,
        TYPE_USHORT = 7,
        TYPE_INT32 = 8,
        TYPE_INT = 9,
        TYPE_UINT32 = 10,
        TYPE_UINT = 11,
        TYPE_INT64 = 12,
        TYPE_LONG = 13,
        TYPE_SINGLE = 14,
        TYPE_FLOAT = 15,
        TYPE_FLOAT32 = 16,
        TYPE_DOUBLE = 17,
        TYPE_FLOAT64 = 18,
        TYPE_DECIMAL = 19,
        TYPE_DATETIME = 20,
        TYPE_DATESTRING = 21,
        TYPE_SID = 22,
        TYPE_STRING = 23,
        TYPE_STRING_BY_LEN = 24,
        TYPE_STRING_BY_HEADER_REF = 25,
        TYPE_STRING_BY_REF = 26,
        TYPE_STRING_LEN = 27,
        TYPE_ENCODED_INT32 = 28,
        TYPE_SKIP = 29,
        TYPE_COPY_INT32 = 30,

        NONE
    }
}

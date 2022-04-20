
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archimedes.Enums
{
    /// <summary>
    /// Flags which can be combine/set to alter the way a cell is processed
    /// </summary>
    [Flags]
    public enum CellFlags
    {
        Hidden = 1,
        RowCount = 2,
        RdbIgnore = 4,
        SqlIgnore = 8,
        LoopCounter = 16,
        None = 1024
    }
}

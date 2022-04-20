using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archimedes.Enums
{
    /// <summary>
    /// Determines how a structure will be parsed
    /// </summary>
    [Flags]
    public enum ParseFlags
    {
        Info = 1,
        Structure = 2,
        Both = Info | Structure
    }
}

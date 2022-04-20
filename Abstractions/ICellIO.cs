using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archimedes.Abstractions
{
    /// <summary>
    /// Cell interface that garuntees the presence of Read and Write methods in derived cells
    /// </summary>
    public interface ICellIO
    {
        object Read();

        void Write(object value);
    }

}

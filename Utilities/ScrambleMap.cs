using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archimedes.Utilities
{
    /// <summary>
    /// Integer bits scrambling utility
    /// </summary>
    public struct ScrambleMap
    {
        /// <summary>
        /// Construct the map table
        /// </summary>
        static ScrambleMap()
        {
            int len = 32;

            for (int i = 0; i < len; ++i)
                map[i] = i;

            int idx = 3; //gap

            for (int i = 0; i < len; ++i)
            {
                int t = map[i];

                while (idx >= len)
                    idx -= len;

                map[i] = map[idx];
                map[idx] = t;

                idx += (3 + i);
            }
        }

        /// <summary>
        /// Scramble the bits of the given value to obscure the true integer
        /// </summary>
        /// <param name="value">Unencoded Value</param>
        /// <returns>Encoded Value</returns>
        public static int Scramble(int value)
        {
            int r = 0;

            for (int i = 0; i < 32; i++)
                if ((value & (1 << i)) != 0)
                    r |= (1 << map[i]);

            return r;
        }

        /// <summary>
        /// Restore the bits of the given value to restore the original unencoded value.
        /// </summary>
        /// <param name="value">Encoded value</param>
        /// <returns>Unencoded value</returns>
        public static int Restore(int value)
        {
            int r = 0;

            for (int i = 0; i < 32; ++i)
                if ((value & (1 << map[i])) != 0)
                    r |= (1 << i);

            return r;
        }

        static int[] map = new int[32];
    }
}

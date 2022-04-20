using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archimedes.Utilities
{
    /// <summary>
    /// Seeded identity value that can be incremented and decremented
    /// </summary>
    public static class SID
    {
        /// <summary>
        /// The current value of this seeded id
        /// </summary>
        public static int Current { get; set; } = 0;

        /// <summary>
        /// Increment (by 1) and return the value of this seeded id
        /// </summary>
        public static int Increment => ++Current;

        /// <summary>
        /// Decrement (by 1) and return the value of this seeded id
        /// </summary>
        public static int Decrement => Math.Max(--Current, 0);

        /// <summary>
        /// Reset the value of this seed id to 0 unless another value is provided
        /// </summary>
        /// <param name="value"></param>
        public static void New(int value = 0) => Current = value;
    }
}

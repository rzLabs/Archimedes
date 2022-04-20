using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archimedes.Utilities
{
    /// <summary>
    /// Conversion of string<>byte[] using the provided Encoding
    /// </summary>
    public static class ByteUtility
    {
        /// <summary>
        /// Encoding used to convert string<>byte[]
        /// </summary>
        public static Encoding Encoding = Encoding.Default;

        /// <summary>
        /// Convert the provided string into its encoded bytes (w/ additional blank bytes if length is provided)
        /// </summary>
        /// <param name="text">String to be encoded/converted</param>
        /// <param name="length">Length of the final array (optional)</param>
        /// <returns>Enocded byte[] representing a string</returns>
        public static byte[] ToBytes(string text, int length = -1)
        {
            byte[] msgBuffer = Encoding.GetBytes(text);
            byte[] outBuffer = msgBuffer;

            if (outBuffer == null)
                return null;

            if (length != -1)
            {
                outBuffer = new byte[length];
                Buffer.BlockCopy(msgBuffer, 0, outBuffer, 0, msgBuffer.Length);
            }

            return outBuffer;
        }

        /// <summary>
        /// Convert the provided byte[] into an encoded string
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string ToString(byte[] buffer) => Encoding?.GetString(buffer);
    }
}

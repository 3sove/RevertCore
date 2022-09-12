using System.Runtime.InteropServices;

namespace Revert.Core.Indexing
{
    public static class ByteArrayPInvokeComparer
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        public static int Compare(byte[] byteArray1, byte[] byteArray2, long count)
        {
            return memcmp(byteArray1, byteArray2, count);
        }


    }
}

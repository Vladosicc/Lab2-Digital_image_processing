using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCOI_2_R
{
    using System.Runtime.InteropServices;

    class Import
    {
        [DllImport("SCOIDLL.dll", EntryPoint = "GetGistSource", CallingConvention = CallingConvention.StdCall)]
        static public extern void GetGistSource(byte[] bytes, int lenght, int[] Output);
        
        [DllImport("SCOIDLL.dll", EntryPoint = "ReCalcBitmap", CallingConvention = CallingConvention.StdCall)]
        static public extern void ReCalcBitmap(byte[] bytes, int lenght, byte[] lvl);

        [DllImport("SCOIDLL.dll", EntryPoint = "All", CallingConvention = CallingConvention.StdCall)]
        static public extern void All(byte[] bytes, int lenght, byte[] lvl, int[] Output);
    }
}

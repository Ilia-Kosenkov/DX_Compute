using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CS_Interop;
using NUnit.Framework.Constraints;

using static CS_Interop.Interop;

namespace Tests
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            new Test().Fail();

            return 0;
        }
    }
}

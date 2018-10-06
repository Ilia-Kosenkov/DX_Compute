using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CS_Interop;

namespace Benchmark
{
    [SimpleJob]
    public class Interop_1
    {
        private const int A = 50;
        private const int B = 193;
        private static readonly int C = A + B;

        private DllHandler handler_;

        [GlobalSetup]
        public void GlobalSetup()
        {
            handler_ = new DllHandler("../../../Release/Native_Compute.dll");
        }

        [Benchmark]
        public void Static()
        {
            var method = handler_.GetMethod<Interop.AddFunc>("add");
            method.Invoke(A, B, out var res);
        }

        [Benchmark]
        public void Dynamic()
        {
            var method = handler_.GetMethod(
                "add",
                typeof(void),
                typeof(int),
                typeof(int),
                typeof(int).MakeByRefType());

            var par = new object[] {A, B, new int()};

            method.DynamicInvoke(par);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            handler_.Dispose();
        }
    }
}

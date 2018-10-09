// MIT License
// 
// Copyright(c) 2018 Ilia Kosenkov
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CS_Interop;

namespace Benchmark
{
    [SimpleJob]
    public class Interop_1
    {
        private DxLibrary _lib;
        private DxDevice _dev;

        public double[] A;
        public double[] B;

        public double[] C1;
        public double[] C2;
        public double[] C3;


        [Params(1_000, 100_000)]
        public int N;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var r = new Random();

            A = Enumerable.Range(0, N)
                          .Select(i => r.NextDouble())
                          .ToArray();
            B = Enumerable.Range(0, N)
                          .Select(i => r.NextDouble())
                          .ToArray();

            C1 = new double[N];
            C2 = new double[N];
            C3 = new double[N];


            _lib = new DxLibrary("../../../../x64/Release/Native_Compute.dll");
            _lib.Initialize();
            _dev = _lib.CreateDevice(0);
            using (var str = new FileStream(Path.Combine("../../../../x64/Release", "SimpleShader.cso"), FileMode.Open))
            {
                var data = DxLibrary.LoadShaderByteCode(str);

                _dev.CreateCsShader(data, "SimpleShader");
            }


        }

       // [Benchmark(Baseline = true)]
        public void Loop()
        {
            for (var i = 0; i < N; i++)
                C1[i] = A[i] + B[i];
        }

        //[Benchmark]
        public void ParallelLoop()
        {
            Parallel.For(0, N, (i) => { C2[i] = A[i] + B[i]; });
            
        }

        [Benchmark]
        public void GPU()
        {
            _dev.CreateStructuredBuffer(Marshal.SizeOf<double>(), N, "out_buff");
            _dev.CreateCpuBuffer<double>(N, "cpu_buff");
            _dev.CreateView("uav", "out_buff", ViewType.UnorderedAccess);
            var handle1 = _dev.CreateStructuredBufferFromPinned(A, "int_buff_1");
            var handle2 = _dev.CreateStructuredBufferFromPinned(B, "int_buff_2");

            _dev.CreateView("srv_1", "int_buff_1", ViewType.ShaderResource);
            _dev.CreateView("srv_2", "int_buff_2", ViewType.ShaderResource);

            _dev.SetupContext("SimpleShader", new[] { "srv_1", "srv_2" }, new[] { "uav" });
            _dev.Dispatch((uint)N, 1, 1);

            _dev.BufferCopy("out_buff", "cpu_buff");
            _dev.GrabFromCpuBuffer(C3, "cpu_buff");

            _dev.ClearContext();

            _dev.ClearContext();
            _dev.RemoveView("srv_1", ViewType.ShaderResource);
            _dev.RemoveView("srv_2", ViewType.ShaderResource);
            _dev.RemoveView("uav", ViewType.UnorderedAccess);
            _dev.RemoveBuffer("int_buff_2");
            _dev.RemoveBuffer("int_buff_1");
            _dev.RemoveBuffer("cpu_buff");
            _dev.RemoveBuffer("out_buff");
            handle1.Free();
            handle2.Free();
        }


        [GlobalCleanup]
        public void GlobalCleanup()
        {


            _dev.Dispose();
            _lib.Dispose();
        }
    }
}

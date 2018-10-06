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

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
using CS_Interop;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Test
    {
#if X86
        public const string DllPath = "../../../../Debug/Native_Compute.dll";
#endif
#if X64
        public const string DllPath = "../../../../x64/Debug/Native_Compute.dll";
#endif

        [Theory]
        public void Fail()
        {
            var path = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, DllPath));
            
            using (var x = new DllHandler(path))
            {
                var createFactory = x.GetMethod("create_factory", typeof(int));
                var freeFactory = x.GetMethod("free_factory", typeof(int));
                var listDevices = x.GetMethod(
                    "list_devices",
                    typeof(int),
                    typeof(int).MakeByRefType());

                var getAdapterDesc = x.GetMethod(
                    "get_adapter_descriptor",
                    typeof(int),
                    typeof(int),
                    typeof(DXGI_ADAPTER_DESC).MakeByRefType());

                createFactory.ExecuteNoParam();


                listDevices.ExecuteOneOut(out int nDev);

                var descs = new DXGI_ADAPTER_DESC[nDev];

                for (var i = 0; i < descs.Length; i++)
                {
                    getAdapterDesc.ExecuteOneInOneOut(i, out descs[i]);
                }

                foreach(var d in descs)
                    Console.Error.WriteLine($"{d.Description, -20}\t" +
                                            $"{d.DedicatedVideoMemory.ToUInt64(), -15}");

                freeFactory.ExecuteNoParam();

            }
        }
    }
}

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
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CS_Interop;
using NUnit.Framework;

namespace Tests
{
    public class TestParamSource
    {
        public static IEnumerable ArraySizes
        {
            get
            {
                yield return 100;
                yield return 10_000;
                yield return 1_000_000;
                yield return 10_000_000;
            }
        }
    }

    [TestFixture]
    public class Test
    {
#if X86
        public const string DllPath = "../../../../Debug";
#endif
#if X64
        public const string DllPath = "../../../../x64/Debug";
#endif

        public Random r;



        [SetUp]
        public void Setup()
        {
            r = new Random();
        }

        //[Theory]
        //[TestCaseSource(typeof(TestParamSource), nameof(TestParamSource.ArraySizes))]
        public void SimpleAdd(int n)
        {
            var buff1 = Enumerable.Range(0, n)
                                  .Select(i => 1.0 * r.NextDouble())
                                  .ToArray();
            var buff2 = Enumerable.Range(0, n)
                                  .Select(i => 1.0 * r.NextDouble())
                                  .ToArray();

            var sum = new double[n];
            for (var i = 0; i < n; i++)
                sum[i] = buff1[i] + buff2[i];

            var result = new double[n];

            var dirPath = Path.GetFullPath(Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                DllPath));

            var path = Path.Combine(dirPath, "Native_Compute.dll");

            using (var dll = new DxLibrary(path))
            {
                dll.Initialize();

                var devs = dll.ListAvailableDevices();

                var index = Array.FindIndex(devs, d => d.Description.Contains("1060"));

                using (var device = dll.CreateDevice(index))
                {

                    using (var str = new FileStream(Path.Combine(dirPath, "SimpleShader.cso"), FileMode.Open))
                    {
                        var data = DxLibrary.LoadShaderByteCode(str);

                        device.CreateCsShader(data, "SimpleShader");
                    }

                    var handle1 = device.CreateStructuredBufferFromPinned(buff1, "int_buff_1");
                    var handle2 = device.CreateStructuredBufferFromPinned(buff2, "int_buff_2");
                    device.CreateStructuredBuffer(Marshal.SizeOf<double>(), n, "out_buff");
                    device.CreateCpuBuffer<double>(n, "cpu_buff");

                    device.CreateView("srv_1", "int_buff_1", ViewType.ShaderResource);
                    device.CreateView("srv_2", "int_buff_2", ViewType.ShaderResource);
                    device.CreateView("uav", "out_buff", ViewType.UnorderedAccess);

                    device.SetupContext("SimpleShader", new[] {"srv_1", "srv_2"}, new[] {"uav"});
                    device.Dispatch((uint) n, 1, 1);


                    device.BufferCopy("out_buff", "cpu_buff");
                    device.GrabFromCpuBuffer(result, "cpu_buff");


                    device.ClearContext();
                    device.RemoveView("srv_1", ViewType.ShaderResource);
                    device.RemoveView("srv_2", ViewType.ShaderResource);
                    device.RemoveView("uav", ViewType.UnorderedAccess);
                    device.RemoveBuffer("cpu_buff");
                    device.RemoveBuffer("out_buff");
                    device.RemoveBuffer("int_buff_2");
                    device.RemoveBuffer("int_buff_1");


                    handle1.Free();
                    handle2.Free();
                }
            }

            Assert.That(result, Is.EqualTo(sum).AsCollection);
        }

        [Test]
        public void ManagedAdd()
        {
           Managed.SimpleAdder.Run();
        }
    }
}

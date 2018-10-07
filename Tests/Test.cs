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
        public const string DllPath = "../../../../Debug";
#endif
#if X64
        public const string DllPath = "../../../../x64/Debug";
#endif

        //[Theory]
        public void Fail()
        {
            var dirPath = Path.GetFullPath(Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                DllPath));

            var path = Path.Combine(dirPath, "Native_Compute.dll");

            using (var dll = new DxLibrary(path))
            {
                dll.Initialize();
                
                var devs = dll.ListAvailableDevices();

                var index = Array.FindIndex(devs, d => d.Description.Contains("1060"));

                var device = dll.CreateDevice(index);

                using (var str = new FileStream(Path.Combine(dirPath, "SimpleShader.cso"), FileMode.Open))
                {
                    var data = DxLibrary.LoadShaderByteCode(str);

                    device.CreateCsShader(data, "SimpleShader");
                }

                device.Dispose();
            }
        }
    }
}

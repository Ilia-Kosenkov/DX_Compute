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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using static CS_Interop.Interop;

namespace CS_Interop
{
    public class DxDevice : IDisposable
    {
        private readonly DxLibrary _libHandle;

        public int DeviceId
        {
            get;
            internal set;
        }

        public AdapterProperties Properties
        {
            get;
            internal set;
        }

        internal DxDevice(DxLibrary lib)
        {
            _libHandle = lib;
        }

        public void CreateCsShader(byte[] byteCode, string name)
        {
            if(byteCode is null)
                throw new ArgumentNullException(nameof(byteCode));
            if (byteCode.Length == 0)
                throw new ArgumentException("Empty byte code.", nameof(byteCode));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Illegal name.", nameof(name));


            var method = _libHandle.GetMethod("create_cs_shader", typeof(int),
                typeof(int), typeof(int), typeof(IntPtr), typeof(int));

            GCHandle handle = default;
            try
            {
                handle = GCHandle.Alloc(byteCode, GCHandleType.Pinned);
                if (!IsOk((int)method.DynamicInvoke(
                    DeviceId, name.GetHashCode(), handle.AddrOfPinnedObject(), byteCode.Length), 
                    out var ex)) throw ex;
            }
            finally
            {
                handle.Free();
            }
        }

        public void Dispose()
        {
            var method = _libHandle.GetMethod("free_device_resources_forced", typeof(int), typeof(int));

            method.DynamicInvoke(DeviceId);
        }
    }
}

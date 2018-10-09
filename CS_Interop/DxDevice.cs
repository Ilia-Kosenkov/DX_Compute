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

        public GCHandle CreateStructuredBufferFromPinned<T>(T[] data, string name)
        {
            
            var createBuffer = _libHandle.GetMethod("create_structured_buffer",
                typeof(int),
                typeof(int), typeof(int), typeof(IntPtr), typeof(int), typeof(int));

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            if (!IsOk((int) createBuffer.DynamicInvoke(
                DeviceId, name.GetHashCode(), handle.AddrOfPinnedObject(),
                Marshal.SizeOf<T>(), data.Length), out var ex))
            {
                handle.Free();
                throw ex;
            }

            return handle;
        }

        public void CreateStructuredBuffer(int elemSize, int numElements, string name)
        {
            var createBuffer = _libHandle.GetMethod("create_structured_buffer",
                typeof(int),
                typeof(int), typeof(int), typeof(IntPtr), typeof(int), typeof(int));
            if (!IsOk((int)createBuffer.DynamicInvoke(
                DeviceId, name.GetHashCode(), IntPtr.Zero,
                elemSize, numElements), out var ex))
                throw ex;
        }

        public void CreateCpuBuffer<T>(int numElements, string name)
        {
            var method = _libHandle.GetMethod("create_cpu_buffer",
                typeof(int),
                typeof(int), typeof(int), typeof(int), typeof(int));

            if (!IsOk((int)method.DynamicInvoke(
                DeviceId, name.GetHashCode(), 
                Marshal.SizeOf<T>(), numElements), out var ex)) throw ex;
        }

        public void BufferCopy(string source, string destination)
        {
             _libHandle.GetMethod("buffer_memcpy",
                typeof(int),
                typeof(int), typeof(int), typeof(int))
                                   .Execute(DeviceId, destination.GetHashCode(), source.GetHashCode());
        }

        public void GrabFromCpuBuffer<T>(T[] data, string name)
        {
            var method = _libHandle.GetMethod("grab_buffer_data",
                typeof(int),
                typeof(int), typeof(int), typeof(IntPtr));

            GCHandle handle = default;
            try
            {
                handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                method.Execute(DeviceId, name.GetHashCode(), handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }

        }

        public void RemoveBuffer(string name)
        {
           _libHandle.GetMethod("remove_buffer",
                typeof(int), typeof(int), typeof(int))
                     .Execute(DeviceId, name.GetHashCode());
        }

        public void CreateView(string viewName, string buffName, ViewType type)
        {
            switch (type)
            {
                case ViewType.ShaderResource:
                {
                    _libHandle.GetMethod("create_srv",
                                  typeof(int), 
                                  typeof(int), typeof(int), typeof(int))
                              .Execute(DeviceId, viewName.GetHashCode(), buffName.GetHashCode());
                    return;
                }
                case ViewType.UnorderedAccess:
                {
                    _libHandle.GetMethod("create_uav",
                                  typeof(int), 
                                  typeof(int), typeof(int), typeof(int))
                              .Execute(DeviceId, viewName.GetHashCode(), buffName.GetHashCode());
                    return;
                }
                default:
                    throw new NotSupportedException();
            }
        }

        public void RemoveView(string viewName, ViewType type)
        {
            switch (type)
            {
                case ViewType.ShaderResource:
                {
                    _libHandle.GetMethod("remove_srv",
                                  typeof(int), typeof(int), typeof(int))
                              .Execute(DeviceId, viewName.GetHashCode());
                    return;
                }
                case ViewType.UnorderedAccess:
                {
                    _libHandle.GetMethod("remove_uav",
                                  typeof(int), typeof(int), typeof(int))
                              .Execute(DeviceId, viewName.GetHashCode());
                    return;
                }
                default:
                    throw new NotSupportedException();
            }
        }

        public void SetupContext(string shader, string[] srvs, string[] uavs)
        {
            var method = _libHandle.GetMethod("setup_context",
                typeof(int),
                typeof(int), typeof(int), typeof(IntPtr), typeof(int), typeof(IntPtr), typeof(int));

            var srvInds = srvs.Select(s => s.GetHashCode()).ToArray();
            var uavInds = uavs.Select(u => u.GetHashCode()).ToArray();

            GCHandle srvHandle = default;
            GCHandle uavHandle = default;
            try
            {
                srvHandle = GCHandle.Alloc(srvInds, GCHandleType.Pinned);
                uavHandle = GCHandle.Alloc(uavInds, GCHandleType.Pinned);

                if (!IsOk((int) method.DynamicInvoke(
                    DeviceId, shader.GetHashCode(),
                    srvHandle.AddrOfPinnedObject(), srvs.Length,
                    uavHandle.AddrOfPinnedObject(), uavs.Length), out var ex))
                    throw ex;
            }
            finally
            {
                srvHandle.Free();
                uavHandle.Free();
            }

        }

        public void ClearContext()
            => _libHandle.GetMethod("clear_context",
                    typeof(int), typeof(int)).Execute(DeviceId);

        public void Dispatch(uint x, uint y, uint z)
        {
            var method = _libHandle.GetMethod("dispatch",
                typeof(int),
                typeof(int), typeof(uint), typeof(uint), typeof(uint));

            if (!IsOk((int) method.DynamicInvoke(
                DeviceId,
                x, y, z), out var ex))
                throw ex;
        }

        public void Dispose()
        {
            var method = _libHandle.GetMethod("free_device_resources_forced", typeof(int), typeof(int));

            method.DynamicInvoke(DeviceId);
        }
    }
}

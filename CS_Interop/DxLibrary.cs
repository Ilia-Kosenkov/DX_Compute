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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static CS_Interop.Interop;

namespace CS_Interop
{
    public class DxLibrary : DllHandler
    {
        private int _devNumber;

        public bool IsInitialized
        {
            get;
            private set;
        }

        public DxLibrary(string path) : base(path)
        {
        }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            var getFactory = GetMethod("get_factory", typeof(int), typeof(int).MakeByRefType());
            if(IsOk((int)getFactory.DynamicInvoke(new object[1]), out _))
                throw new InvalidOperationException("The library appears to be initialized outside of the current scope.");

            GetMethod("create_factory", typeof(int)).Execute();
            GetMethod("list_devices", typeof(int), typeof(int).MakeByRefType()).ExecuteOut(out _devNumber);

            IsInitialized = true;
        }

        public AdapterProperties[] ListAvailableDevices()
        {
            if(!IsInitialized)
                throw new InvalidOperationException("The library should be initialized first.");
            
            var props = new AdapterProperties[_devNumber];

            for(var i = 0 ; i < props.Length; i++)
                GetMethod("get_adapter_descriptor", typeof(int), 
                        typeof(int), typeof(AdapterProperties).MakeByRefType())
                    .ExecuteOut(i, out props[i]);

            return props;
        }

        public DxDevice CreateDevice(int index)
        {
            if(index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, 
                    "Should be greater than 0.");

            var props = ListAvailableDevices();

            if (index >= props.Length)
                throw new ArgumentOutOfRangeException(nameof(index), index, 
                    $"Should be less than {props.Length}.");

            return new DxDevice(this)
            {
                DeviceId = index,
                Properties = props[index]
            };
        }

        public static byte[] LoadShaderByteCode(Stream input)
        {
            if(!(input.CanRead && input.CanSeek))
                throw new InvalidOperationException("Stream does not support reading & seeking.");

            var data = new byte[input.Length];
            input.Read(data, 0, data.Length);

            return data;
        }
    }
}

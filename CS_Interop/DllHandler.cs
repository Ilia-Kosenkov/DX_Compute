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
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using static CS_Interop.Interop;

namespace CS_Interop
{
    public class DllHandler : IDisposable
    {
        private readonly IntPtr _libPtr;

        private readonly Dictionary<string, (long Hash, Type DynType, Delegate FuncPtr)>
            _definedTypes = new Dictionary<string, (long Hash, Type DynType, Delegate FuncPtr)>();

        private readonly Dictionary<string, object> _exportedPtrs = new Dictionary<string, object>();

        public bool IsDisposed { get; private set; }

        public DllHandler(string path)
        {
            var lPath = Path.GetFullPath(path);

            if (!File.Exists(lPath))
                throw new FileNotFoundException("Failed to load .dll.", lPath);

            _libPtr = LoadLibrary(lPath);

            if (IsNullPtr(_libPtr, out var ex)) throw ex;

        }

        public void Dispose()
        {
            FreeLibrary(_libPtr);
            IsDisposed = true;
        }


        public T GetMethod<T>(string name) where T : class
        {
            if (_exportedPtrs.ContainsKey(name))
                return (T) _exportedPtrs[name];

            var ptr = GetProcAddress(_libPtr, name);

            if (IsNullPtr(ptr, out var ex)) throw ex;

            var fPtr = Marshal.GetDelegateForFunctionPointer<T>(ptr);

            _exportedPtrs.Add(name, fPtr);

            return fPtr;

        }

        public Delegate GetMethod(string name, Type returnType, params Type[] types)
        {
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Invalid native method name.", nameof(name));

            if (types is null)
                throw new ArgumentNullException(nameof(types));

            if (returnType is null)
                throw new ArgumentNullException(nameof(returnType));

            var hash = types.Select(x => x.GetHashCode()).Sum() + returnType.GetHashCode();

            if (_definedTypes.ContainsKey(name))
            {
                var definedItem = _definedTypes[name];

                if (hash != definedItem.Hash)
                    throw new ArgumentException(
                        $"Method \"{name}\" is already exported, but with different parameter set.");

                return definedItem.FuncPtr;
            }
            
            var ptr = GetProcAddress(_libPtr, name);

            if (IsNullPtr(ptr, out var ex)) throw ex;

            var tBuilder = DelegateBuilder.MBuilder.DefineType(
                name,
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.AnsiClass | TypeAttributes.Sealed,
                typeof(MulticastDelegate));

            var ctorBuilder = tBuilder.DefineConstructor(
                MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { typeof(object), typeof(IntPtr) });


            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            ctorBuilder.SetImplementationFlags(MethodImplAttributes.Managed | MethodImplAttributes.Runtime);

            var invBuilder = tBuilder.DefineMethod(
                "Invoke",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
                MethodAttributes.Virtual,
                CallingConventions.Standard,
                returnType,
                null,
                null,
                types,
                null,
                null);

            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            invBuilder.SetImplementationFlags(MethodImplAttributes.Managed | MethodImplAttributes.Runtime);

            var attrCon = typeof(UnmanagedFunctionPointerAttribute).GetConstructors().First();
            var attrBuilder = new CustomAttributeBuilder(attrCon, new object[] {CallingConvention.Cdecl});

            tBuilder.SetCustomAttribute(attrBuilder);
            
            var t = tBuilder.CreateType();
            var fPtr = Marshal.GetDelegateForFunctionPointer(ptr, t);


            _definedTypes.Add(name, 
                (Hash:hash, DynType:t, FuncPtr: fPtr));

            return fPtr;
        }
        
    }
}

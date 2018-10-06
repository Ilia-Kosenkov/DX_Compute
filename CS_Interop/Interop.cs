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
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace CS_Interop
{
    public static class Interop
    {
        public static class DelegateBuilder
        {
            public static readonly AssemblyName AName =
                new AssemblyName("DynamicFunctionPointers");

            public static readonly AssemblyBuilder ABuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    AName, AssemblyBuilderAccess.Run);

            public static readonly ModuleBuilder MBuilder = ABuilder
                .DefineDynamicModule(AName.Name + "Module");

        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AddFunc(
            [In] int a,
            [In] int b,
            [Out] out int c);


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int SinglePtrOut([Out] out IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int NoParam();

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibrary([In] string path);

        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary([In] IntPtr ptr);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetProcAddress(
            [In] IntPtr ptr,
            [In] string name);

        public static void ExecuteNoParam(this Delegate del)
        {
           if (!IsOk((int) del.DynamicInvoke(), out var ex))
                throw ex;
        }

        public static void ExecuteOneOut<T>(this Delegate del, out T param)
        {
            var pars = new object[] {default(T)};
            if (!IsOk((int)del.DynamicInvoke(pars), out var ex))
                throw ex;
            param = (T) pars[0];
        }

        public static void ExecuteOneInOneOut<T1, T2>(this Delegate del, T1 inParam, out T2 outParam)
        {
            var pars = new object[] { inParam, default(T2) };
            if (!IsOk((int)del.DynamicInvoke(pars), out var ex))
                throw ex;
            outParam = (T2)pars[1];
        }


        public static bool PinvokeFailed(out Exception ex)
        {
            ex = null;
            var err = Marshal.GetLastWin32Error();
            if (err == 0)
                return false;

            var hresult = Marshal.GetHRForLastWin32Error();
            return !IsOk(hresult, out ex);
        }

        public static bool IsNullPtr(IntPtr ptr, out Exception ex)
        {
            ex = null;

            if (ptr == IntPtr.Zero)
                return PinvokeFailed(out ex);

            return false;
        }

        public static bool IsOk(int hresult, out Exception ex)
        => (ex = hresult == 0 ? null : Marshal.GetExceptionForHR(hresult)) == null;

    }
}

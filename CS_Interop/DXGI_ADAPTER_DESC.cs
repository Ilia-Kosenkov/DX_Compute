using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CS_Interop
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    [DebuggerDisplay("{" + nameof(Description) + "} : {DedicatedVideoMemory.ToUInt64()}")]
    // ReSharper disable once InconsistentNaming
    public struct DXGI_ADAPTER_DESC
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Description;

        public uint VendorId;
        public uint DeviceId;
        public uint SubSysId;
        public uint RevisionId;
        public UIntPtr DedicatedVideoMemory;
        public UIntPtr DedicatedSystemMemory;
        public UIntPtr DedicatedSharedMemory;
        public ulong LuidLow;
        public long LuidHigh;
    }
}

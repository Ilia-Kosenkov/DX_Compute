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

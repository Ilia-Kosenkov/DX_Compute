using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
//using SharpDX.DXGI;
using Device = SharpDX.DXGI.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Managed
{
    public class SimpleAdder
    {
        public static void Run()
        {
            const string path = "../../../../x64/Debug";
            var stack = new Stack<ComObject>();
            var n = 24;

            var input_1 = Enumerable.Range(0, n)
                                    .ToArray();

            var input_2 = Enumerable.Range(0, n)
                                    .Select(i => -i + 10)
                                    .ToArray();

            var input_3 = Enumerable.Range(0, n)
                                   .Select(i => 2*i)
                                   .ToArray();

            using (var factory = new Factory1())
            {
                using (var adapter = factory.GetAdapter1(0))
                {
                    var device = new SharpDX.Direct3D11.Device(adapter,
                        DeviceCreationFlags.Debug | DeviceCreationFlags.DisableGpuTimeout,
                        FeatureLevel.Level_11_0);
                    stack.Push(device);

                    var buff1 = new Buffer(device, sizeof(int) * n,
                        ResourceUsage.Dynamic, BindFlags.ShaderResource,
                        CpuAccessFlags.Write,
                        ResourceOptionFlags.BufferStructured,
                        sizeof(int));
                    stack.Push(buff1);

                    var buff2 = new Buffer(device, sizeof(int) * n,
                        ResourceUsage.Dynamic, BindFlags.ShaderResource,
                        CpuAccessFlags.Write,
                        ResourceOptionFlags.BufferStructured,
                        sizeof(int));
                    stack.Push(buff2);


                    var resBuff = new Buffer(device, sizeof(int) * n,
                        ResourceUsage.Default, BindFlags.UnorderedAccess,
                        CpuAccessFlags.Read, ResourceOptionFlags.BufferStructured,
                        sizeof(int));
                    stack.Push(resBuff);

                    var cpuBuff = new Buffer(device, sizeof(int) * n,
                        ResourceUsage.Staging, BindFlags.None,
                        CpuAccessFlags.Read, ResourceOptionFlags.BufferStructured, 
                        sizeof(int));
                    stack.Push(cpuBuff);

                    ComputeShader shader;

                    using (var str =
                        new FileStream(Path.GetFullPath(Path.Combine(path, "SimpleShader2.cso")), 
                            FileMode.Open))
                    {
                        var buff = new byte[str.Length];
                        str.Read(buff, 0, buff.Length);
                        shader = new ComputeShader(device, buff);     
                    }
                    stack.Push(shader);


                    var srv_1 = new ShaderResourceView(device, buff1, new ShaderResourceViewDescription()
                    {
                        BufferEx = new ShaderResourceViewDescription.ExtendedBufferResource()
                        {
                            ElementCount = n,
                            FirstElement = 0
                        },
                        Dimension = ShaderResourceViewDimension.ExtendedBuffer,
                        Format = Format.Unknown
                    });
                    stack.Push(srv_1);

                    var srv_2 = new ShaderResourceView(device, buff2, new ShaderResourceViewDescription()
                    {
                        BufferEx = new ShaderResourceViewDescription.ExtendedBufferResource()
                        {
                            ElementCount = n,
                            FirstElement = 0
                        },
                        Dimension = ShaderResourceViewDimension.ExtendedBuffer,
                        Format = Format.Unknown
                    });
                    stack.Push(srv_2);

                    var uav = new UnorderedAccessView(device, resBuff,
                        new UnorderedAccessViewDescription()
                        {
                            Buffer = new UnorderedAccessViewDescription.BufferResource()
                            {
                                ElementCount = n,
                                FirstElement = 0
                            },
                            Dimension = UnorderedAccessViewDimension.Buffer,
                            Format = Format.Unknown
                        });
                    stack.Push(uav);

                    device.ImmediateContext.MapSubresource(buff1,
                        0, MapMode.WriteDiscard, MapFlags.None, out var stream);
                    stream.WriteRange(input_1);
                    device.ImmediateContext.UnmapSubresource(buff1, 0);

                    device.ImmediateContext.MapSubresource(buff2,
                        0, MapMode.WriteDiscard, MapFlags.None, out stream);
                    stream.WriteRange(input_2);
                    device.ImmediateContext.UnmapSubresource(buff2, 0);

                    device.ImmediateContext.ComputeShader.Set(shader);
                    device.ImmediateContext.ComputeShader.SetUnorderedAccessView(0, uav);
                    device.ImmediateContext.ComputeShader.SetShaderResources(0, srv_1, srv_2);

                    device.ImmediateContext.Dispatch(n, 1, 1);

                    device.ImmediateContext.CopyResource(resBuff, cpuBuff);

                    device.ImmediateContext.MapSubresource(cpuBuff, MapMode.Read,
                        MapFlags.None, out stream);
                    var resultData = stream.ReadRange<int>(n);
                    device.ImmediateContext.UnmapSubresource(cpuBuff, 0);

                    for (var i = 0; i < n; i++)
                    {
                        Console.WriteLine($"{input_1[i]: 00;-00} + {input_2[i]: 00;-00} = {resultData[i]: 00;-00}");
                    }
                    device.ImmediateContext.MapSubresource(buff2,
                        0, MapMode.WriteDiscard, MapFlags.None, out stream);
                    stream.WriteRange(input_3);
                    device.ImmediateContext.UnmapSubresource(buff2, 0);

                    device.ImmediateContext.ComputeShader.SetUnorderedAccessView(0, uav);
                    device.ImmediateContext.ComputeShader.SetShaderResources(0, srv_1, srv_2);

                    device.ImmediateContext.Dispatch(n, 1, 1);

                    device.ImmediateContext.CopyResource(resBuff, cpuBuff);

                    device.ImmediateContext.MapSubresource(cpuBuff, MapMode.Read,
                        MapFlags.None, out stream);
                    resultData = stream.ReadRange<int>(n);
                    device.ImmediateContext.UnmapSubresource(cpuBuff, 0);

                    for (var i = 0; i < n; i++)
                    {
                        Console.WriteLine($"{input_1[i]: 00;-00} + {input_3[i]: 00;-00} = {resultData[i]: 00;-00}");
                    }

                    foreach (var item in stack)
                        item.Dispose();

                    device.Dispose();
                }
            }

        }
    }
}

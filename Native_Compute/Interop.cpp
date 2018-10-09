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

#include  "Interop.h"

EXPORT HRESULT get_factory(_Out_ VOID** pp_factory)
{
	
	*pp_factory = reinterpret_cast<void*>(factory.Get());
	debug_print("EXP: get_factory");
	return *pp_factory == nullptr ? E_POINTER : S_OK;

}

EXPORT HRESULT create_factory()
{
	IDXGIFactory* p_factory;

	const auto result = CreateDXGIFactory(__uuidof(IDXGIFactory),
		reinterpret_cast<VOID**>(&p_factory));

	if (result == S_OK)
		factory = PTR<IDXGIFactory>(p_factory);
	
	debug_print("EXP: create_factory");
	return result;

}

EXPORT HRESULT free_resources()
{
	devices.clear();

	CONST auto result = factory.Reset();

	debug_print("EXP: free_resources");
	return result;
}

EXPORT HRESULT list_devices(_Out_ INT* n_dev)
{
	if (factory.Get() == nullptr)
		return E_POINTER;

	auto result = S_OK;

	if (!devices.empty())
		devices.clear();

	for(unsigned int i = 0; result == S_OK; ++i)
	{
		IDXGIAdapter* p_local_adapter;

		result = factory->EnumAdapters(i, &p_local_adapter);
		if(result == S_OK)
		{
			device_item item;
			item.adapter = PTR<IDXGIAdapter>(p_local_adapter);
						
			if(item.adapter->GetDesc(&item.descriptor) != S_OK)
				RtlZeroMemory(&item.descriptor, sizeof(DXGI_ADAPTER_DESC));

			ID3D11Device* p_device;
			ID3D11DeviceContext* p_context;
			D3D_FEATURE_LEVEL acq_feature_level;

			result = D3D11CreateDevice(
				item.adapter.Get(),						 // Existing adapter
				D3D_DRIVER_TYPE_UNKNOWN,				 // Should be unknown
				nullptr,								 // Module for software emulation
				global_device_creation_flags,			 // Creation flags
				global_device_feature_levels,			 // Feature levels (req. 11.0)
				_countof(global_device_feature_levels),	 // Size of array
				D3D11_SDK_VERSION,						 // Work with DX11
				&p_device,								 // Addr of device
				&acq_feature_level,						 // Feature level supported
				&p_context);							 // Immediate context

			if(result == S_OK)
			{
				if(acq_feature_level != global_device_feature_levels[0])
				{
					p_context->Release();
					p_device->Release();
				}
				else
				{
					item.device = PTR<ID3D11Device>{ p_device };
					item.context = PTR<ID3D11DeviceContext>{ p_context };
				}
			}

			devices.push_back(item);
		}
	}

	*n_dev = static_cast<INT>(devices.size());

	debug_print("EXP: list_devices");
	return S_OK;
}

EXPORT HRESULT get_adapter_descriptor(_In_ CONST INT index, _Out_ BYTE* desc)
{
	if (index < 0 || index >= static_cast<INT>(devices.size()))
		return E_INVALIDARG;

	memcpy(desc, &(devices[index].descriptor), sizeof(DXGI_ADAPTER_DESC));
	BOOL is_created = devices[index].device != nullptr;
	memcpy(desc + sizeof(DXGI_ADAPTER_DESC), 
		&is_created, 
		sizeof(BOOL));
	debug_print("EXP: get_adapter_descriptor");
	return S_OK;
}

EXPORT HRESULT free_device_resources_forced(_In_ CONST INT index)
{
	if (index < 0 || index >= static_cast<INT>(devices.size()))
		return E_INVALIDARG;

	devices[index].force_resource_release();
	
	debug_print("EXP: free_device_resources_forced");
	return S_OK;
}

HRESULT create_cs_shader(CONST INT dev_id, CONST INT name_hash, VOID* p_buffer, CONST INT buffer_size)
{
	if (dev_id < 0 || dev_id >= static_cast<INT>(devices.size()))
		return E_INVALIDARG;

	debug_print("EXP: create_cs_shader");
		return devices[dev_id].create_cs_shader(name_hash, p_buffer, buffer_size);
}

HRESULT create_structured_buffer(
	CONST INT dev_id, CONST INT name_hash, VOID* p_data, CONST INT element_size, CONST INT element_count)
{
	if (dev_id < 0 || dev_id >= static_cast<INT>(devices.size()))
		return E_INVALIDARG;

	debug_print("EXP: create_structured_buffer");
	return devices[dev_id].create_structured_buffer(name_hash, p_data, element_size, element_count);
}

HRESULT get_buffer_desc(CONST INT dev_id, CONST INT name_hash)
{
	if (dev_id < 0 || dev_id >= static_cast<INT>(devices.size()))
		return E_INVALIDARG;

	debug_print("EXP: get_buffer_desc");
	return devices[dev_id].get_buffer_desc(name_hash);
}

HRESULT remove_buffer(CONST INT dev_id, CONST INT name_hash)
{
	if (dev_id < 0 || dev_id >= static_cast<INT>(devices.size()))
		return E_INVALIDARG;

	debug_print("EXP: remove_buffer");
	return devices[dev_id].remove_buffer(name_hash);
}

HRESULT create_srv(CONST INT dev_id, CONST INT name_hash, CONST INT buff_name_hash)
{
	if (dev_id < 0 || dev_id >= static_cast<INT>(devices.size()))
		return E_INVALIDARG;

	debug_print("EXP: create_srv");
	return devices[dev_id].create_srv(name_hash, buff_name_hash);
}

HRESULT create_uav(CONST INT dev_id, CONST INT name_hash, CONST INT buff_name_hash)
{
	if(dev_id < 0 || dev_id >= static_cast<INT>(devices.size()))
		return E_INVALIDARG;

	debug_print("EXP: create_uav");
	return devices[dev_id].create_uav(name_hash, buff_name_hash);
}

HRESULT remove_srv(CONST INT dev_id, CONST INT name_hash)
{
	if (dev_id < 0 || dev_id >= static_cast<INT>(devices.size()))
		return E_INVALIDARG;

	debug_print("EXP: remove_srv");
	return devices[dev_id].remove_srv(name_hash);
}

HRESULT remove_uav(CONST INT dev_id, CONST INT name_hash)
{
	if (dev_id < 0 || dev_id >= static_cast<INT>(devices.size()))
		return E_INVALIDARG;

	debug_print("EXP: remove_uav");
	return devices[dev_id].remove_uav(name_hash);
}

HRESULT buffer_memcpy(CONST INT dev_id, CONST INT dest_name_hash, CONST INT src_name_hash)
{
	if (dev_id < 0 || dev_id >= static_cast<INT>(devices.size()))
		return E_INVALIDARG;

	debug_print("EXP: buffer_memcpy");
	return devices[dev_id].buffer_memcpy(dest_name_hash, src_name_hash);
}

HRESULT create_cpu_buffer(CONST INT dev_id, CONST INT name_hash, CONST INT element_size, CONST INT element_count)
{
	if (dev_id < 0 || dev_id >= static_cast<INT>(devices.size()))
		return E_INVALIDARG;

	debug_print("EXP: create_cpu_buffer");
	return devices[dev_id].create_cpu_buffer(name_hash, element_size, element_count);
}

HRESULT grab_buffer_data(CONST INT dev_id, CONST INT name_hash, VOID* destination)
{
	if (dev_id < 0 || dev_id >= static_cast<INT>(devices.size()))
		return E_INVALIDARG;

	debug_print("EXP: grab_buffer_data");
	return devices[dev_id].grab_buffer_data(name_hash, destination);
}

HRESULT setup_context(
	CONST INT dev_id, CONST INT shader, INT* srvs, 
	CONST INT n_srvs, INT* uavs, CONST INT n_uavs)
{
	if (dev_id < 0 || dev_id >= static_cast<INT>(devices.size()))
		return E_INVALIDARG;

	debug_print("EXP: setup_context");
	return devices[dev_id].setup_context(shader, srvs, n_srvs, uavs, n_uavs);
}

HRESULT clear_context(CONST INT dev_id)
{
	if (dev_id < 0 || dev_id >= static_cast<INT>(devices.size()))
		return E_INVALIDARG;
	
	devices[dev_id].clear_context();

	debug_print("EXP: clear_context");
	return S_OK;
}

HRESULT dispatch(CONST INT dev_id, CONST UINT x, CONST UINT y, CONST UINT z)
{
	if (dev_id < 0 || dev_id >= static_cast<INT>(devices.size()))
		return E_INVALIDARG;

	devices[dev_id].context->Dispatch(x, y, z);
	debug_print("EXP: dispatch");

	return S_OK;
}

EXPORT VOID add(_In_ CONST INT a, _In_ CONST INT b, _Out_ INT* c)
{
	*c = a + b;
}



// ReSharper disable CppInconsistentNaming
BOOL WINAPI DllMain(
	HINSTANCE hinstDLL,  // handle to DLL module
	CONST DWORD fdwReason,     // reason for calling function
	LPVOID lpReserved)  // reserved
// ReSharper restore CppInconsistentNaming
{

	// Perform actions based on the reason for calling.
	switch (fdwReason)
	{
	case DLL_PROCESS_ATTACH:
		// Initialize once for each new process.
		// Return FALSE to fail DLL load.
		debug_print("DLL: proc_attached");
		break;

	case DLL_THREAD_ATTACH:
		// Do thread-specific initialization.
		debug_print("DLL: thread_attached");
		break;

	case DLL_THREAD_DETACH:
		// Do thread-specific cleanup.
		debug_print("DLL: thread_detached");
		break;

	case DLL_PROCESS_DETACH:
		// Perform any necessary cleanup.
		debug_print("DLL: proc_detached");
		break;
	default:
		debug_print("DLL: reason_undef");
		break;
	}
	return TRUE;  // Successful DLL_PROCESS_ATTACH.
}
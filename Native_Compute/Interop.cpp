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

void debug_print(const std::string& name)
{
	#ifdef _DEBUG
	std::cerr <<
		std::setw(40) <<
		name <<
		std::setw(10) <<
		std::this_thread::get_id() <<
		std::endl;
	#endif
}


EXPORT HRESULT get_factory(_Out_ void** pp_factory)
{
	
	*pp_factory = reinterpret_cast<void*>(factory.Get());
	debug_print("get_factory");
	return *pp_factory == nullptr ? E_POINTER : S_OK;

}

EXPORT HRESULT create_factory()
{
	IDXGIFactory* p_factory;

	const auto result = CreateDXGIFactory(__uuidof(IDXGIFactory),
		reinterpret_cast<void**>(&p_factory));

	if (result == S_OK)
		factory = PTR<IDXGIFactory>(p_factory);
	
	debug_print("create_factory");
	return result;

}

EXPORT HRESULT free_factory()
{
	for (auto item : devices)
	{
		RtlZeroMemory(&item.descriptor, sizeof(DXGI_ADAPTER_DESC));
		item.adapter.Reset();
	}

	const auto result = factory.Reset();

	debug_print("free_factory");
	return result;
}

EXPORT HRESULT list_devices(_Out_ size_t* n_dev)
{
	if (factory.Get() == nullptr)
		return E_POINTER;

	auto result = S_OK;

	for(unsigned int i = 0; result == S_OK; ++i)
	{
		IDXGIAdapter* p_local_adapter;

		result = factory->EnumAdapters(i, &p_local_adapter);
		if(result == S_OK)
		{
			device_item item{
				PTR<IDXGIAdapter>{p_local_adapter},
				DXGI_ADAPTER_DESC{}
			};
			
			if(item.adapter->GetDesc(&item.descriptor) != S_OK)
				RtlZeroMemory(&item.descriptor, sizeof(DXGI_ADAPTER_DESC));

			devices.push_back(item);
		}
	}

	*n_dev = devices.size();

	debug_print("list_devices");
	return S_OK;
}

EXPORT HRESULT get_adapter_descriptor(_In_ const int index, _Out_ DXGI_ADAPTER_DESC* desc)
{
	if (index < 0 || index >= static_cast<int>(devices.size()))
		return E_INVALIDARG;

	memcpy(desc, &(devices[index].descriptor), sizeof(DXGI_ADAPTER_DESC));

	debug_print("get_adapter_descriptor");
	return S_OK;
}


EXPORT void add(_In_ const int a, _In_ const int b, _Out_ int* c)
{
	*c = a + b;
}

// ReSharper disable CppInconsistentNaming
BOOL WINAPI DllMain(
	HINSTANCE hinstDLL,  // handle to DLL module
	const DWORD fdwReason,     // reason for calling function
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
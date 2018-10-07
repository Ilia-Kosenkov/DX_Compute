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

#include "DeviceItem.h"

void device_item::force_resource_release() noexcept
{
	compiled_shaders.clear();

	context.Reset();
	context = nullptr;
	device.Reset();
	device = nullptr;
	adapter.Reset();
	adapter = nullptr;
	RtlZeroMemory(&descriptor, sizeof(DXGI_ADAPTER_DESC));
	
	debug_print("CPP: device_force_resource_release");
}

HRESULT device_item::create_cs_shader(INT name_hash, VOID* p_buffer, CONST INT buffer_size)
{
	if (!device)
		return E_FAIL;

	if (compiled_shaders.count(name_hash) != 0)
		return E_INVALIDARG;

	ID3D11ComputeShader* p_shader;

	CONST auto result = device->CreateComputeShader(p_buffer, buffer_size, nullptr, &p_shader);

	if (result == S_OK)
	{
		compiled_shaders.insert(
			std::map<INT, PTR<ID3D11ComputeShader>>::value_type(
				name_hash,
				PTR<ID3D11ComputeShader>(p_shader)));
	}

	debug_print("CPP: device_create_cs_shader");
	return result;
}

device_item::device_item()
{
	debug_print("CPP: device_default_ctor");
}

device_item::device_item(CONST device_item& other) noexcept
{
	memcpy(&descriptor, &other.descriptor, sizeof(DXGI_ADAPTER_DESC));
	adapter = other.adapter;
	device = other.device;
	context = other.context;

	debug_print("CPP: device_copy_ctor");
}

device_item::device_item(device_item&& other) noexcept
{
	memcpy(&descriptor, &other.descriptor, sizeof(DXGI_ADAPTER_DESC));
	RtlZeroMemory(&other.descriptor, sizeof(DXGI_ADAPTER_DESC));

	adapter = nullptr;
	other.adapter.Swap(adapter);

	device = nullptr;
	other.device.Swap(device);

	context = nullptr;
	other.context.Swap(context);

	debug_print("CPP: device_move_ctor");
}

device_item& device_item::operator=(CONST device_item& other) noexcept
{
	memcpy(&descriptor, &other.descriptor, sizeof(DXGI_ADAPTER_DESC));
	adapter = other.adapter;
	device = other.device;
	context = other.context;

	debug_print("CPP: device_copy_assignment");
	return *this;
}

device_item& device_item::operator=(device_item&& other) noexcept
{
	memcpy(&descriptor, &other.descriptor, sizeof(DXGI_ADAPTER_DESC));
	RtlZeroMemory(&other.descriptor, sizeof(DXGI_ADAPTER_DESC));

	adapter = nullptr;
	other.adapter.Swap(adapter);

	device = nullptr;
	other.device.Swap(device);

	context = nullptr;
	other.context.Swap(context);

	debug_print("CPP: device_move_assignment");

	return *this;
}

device_item::~device_item()
{
	
	force_resource_release();
	debug_print("CPP: device_destructor");
}

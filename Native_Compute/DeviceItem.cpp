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

VOID device_item::force_resource_release() noexcept
{
	clear_context();
	srvs.clear();
	uavs.clear();
	buffers.clear();
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

HRESULT device_item::create_structured_buffer(
	INT name_hash, VOID* p_data, CONST INT element_size, CONST INT element_count)
{
	if (buffers.count(name_hash) != 0)
		return E_INVALIDARG;

	ID3D11Buffer *p_buffer = nullptr;
	D3D11_BUFFER_DESC desc;
	RtlZeroMemory(&desc, sizeof(D3D11_BUFFER_DESC));
	desc.BindFlags = D3D11_BIND_UNORDERED_ACCESS | D3D11_BIND_SHADER_RESOURCE;
	desc.ByteWidth = element_size * element_count;
	desc.StructureByteStride = element_size;
	desc.MiscFlags = D3D11_RESOURCE_MISC_BUFFER_STRUCTURED;

	HRESULT result;

	if (p_data != nullptr)
	{
		D3D11_SUBRESOURCE_DATA init_data;
		init_data.pSysMem = p_data;

		result = device->CreateBuffer(&desc, &init_data, &p_buffer);
	}
	else
		result = device->CreateBuffer(&desc, nullptr, &p_buffer);

	if(result == S_OK)
	{
		buffers.insert(
			std::map<INT, PTR<ID3D11Buffer>>::value_type(
				name_hash, 
				PTR<ID3D11Buffer>(p_buffer)));
	}

	debug_print("CPP: device_create_structured_buffer");
	return result;
}

HRESULT device_item::get_buffer_desc(CONST INT name_hash) const
{
	if (buffers.count(name_hash) == 0)
		return E_INVALIDARG;

	debug_print("CPP: device_get_buffer_desc");
	return S_OK;
}

HRESULT device_item::remove_buffer(CONST INT name_hash)
{
	if (buffers.count(name_hash) == 0)
		return E_INVALIDARG;

	buffers[name_hash].Reset();
	buffers.erase(name_hash);
	
	debug_print("CPP: device_remove_buffer");
	return S_OK;
}

HRESULT device_item::create_srv(CONST INT name_hash, CONST INT buff_name_hash)
{
	if (buffers.count(buff_name_hash) == 0 ||
		srvs.count(name_hash) != 0)
		return E_INVALIDARG;

	D3D11_BUFFER_DESC buff_desc;
	RtlZeroMemory(&buff_desc, sizeof(D3D11_BUFFER_DESC));
	buffers[buff_name_hash]->GetDesc(&buff_desc);

	D3D11_SHADER_RESOURCE_VIEW_DESC srv_desc;
	RtlZeroMemory(&srv_desc, sizeof(D3D11_SHADER_RESOURCE_VIEW_DESC));
	srv_desc.ViewDimension = D3D11_SRV_DIMENSION_BUFFEREX;
	srv_desc.BufferEx.FirstElement = 0;

	if (buff_desc.MiscFlags & D3D11_RESOURCE_MISC_BUFFER_ALLOW_RAW_VIEWS)
	{
		srv_desc.Format = DXGI_FORMAT_R32_TYPELESS;
		srv_desc.BufferEx.Flags = D3D11_BUFFEREX_SRV_FLAG_RAW;
		srv_desc.BufferEx.NumElements = buff_desc.ByteWidth / 4;
	}
	else if (buff_desc.MiscFlags & D3D11_RESOURCE_MISC_BUFFER_STRUCTURED)
	{
		srv_desc.Format = DXGI_FORMAT_UNKNOWN;
		srv_desc.BufferEx.NumElements = buff_desc.ByteWidth / buff_desc.StructureByteStride;
	}
	else
		return E_INVALIDARG;
	ID3D11ShaderResourceView *p_srv;
	const auto result = device->CreateShaderResourceView(buffers[buff_name_hash].Get(), &srv_desc, &p_srv);

	if(result == S_OK)
	{
		srvs.insert(std::map<INT, PTR<ID3D11ShaderResourceView>>::value_type(
			name_hash,
			PTR<ID3D11ShaderResourceView>(p_srv)));
	}

	debug_print("CPP: device_create_srv");
	return result;
}

HRESULT device_item::create_uav(CONST INT name_hash, CONST INT buff_name_hash)
{
	if (buffers.count(buff_name_hash) == 0 ||
		uavs.count(name_hash) != 0)
		return E_INVALIDARG;

	D3D11_BUFFER_DESC buff_desc;
	RtlZeroMemory(&buff_desc, sizeof(D3D11_BUFFER_DESC));
	buffers[buff_name_hash]->GetDesc(&buff_desc);

	D3D11_UNORDERED_ACCESS_VIEW_DESC uav_desc;
	RtlZeroMemory(&uav_desc, sizeof(D3D11_UNORDERED_ACCESS_VIEW_DESC));
	uav_desc.ViewDimension = D3D11_UAV_DIMENSION_BUFFER;
	uav_desc.Buffer.FirstElement = 0;

	if(buff_desc.MiscFlags & D3D11_RESOURCE_MISC_BUFFER_ALLOW_RAW_VIEWS)
	{
		uav_desc.Format = DXGI_FORMAT_R32_TYPELESS;
		uav_desc.Buffer.Flags = D3D11_BUFFER_UAV_FLAG_RAW;
		uav_desc.Buffer.NumElements = buff_desc.ByteWidth / 4;
	}
	if (buff_desc.MiscFlags & D3D11_RESOURCE_MISC_BUFFER_STRUCTURED)
	{
		uav_desc.Format = DXGI_FORMAT_UNKNOWN;
		uav_desc.Buffer.NumElements = buff_desc.ByteWidth / buff_desc.StructureByteStride;
	}
	else
		return E_INVALIDARG;
	ID3D11UnorderedAccessView *p_uav;
	CONST auto result = device->CreateUnorderedAccessView(buffers[buff_name_hash].Get(), &uav_desc, &p_uav);

	if(result == S_OK)
	{
		uavs.insert(std::map<INT, PTR<ID3D11UnorderedAccessView>>::value_type(
			name_hash,
			PTR<ID3D11UnorderedAccessView>(p_uav)));
	}

	debug_print("CPP: device_create_uav");
	return result;
}

HRESULT device_item::remove_srv(CONST INT name_hash)
{
	if (srvs.count(name_hash) == 0)
		return E_INVALIDARG;
	
	srvs[name_hash].Reset();
	srvs.erase(name_hash);

	debug_print("CPP: device_remove_srv");
	return S_OK;
}

HRESULT device_item::remove_uav(CONST INT name_hash)
{
	if (uavs.count(name_hash) == 0)
		return E_INVALIDARG;

	uavs[name_hash].Reset();
	uavs.erase(name_hash);

	debug_print("CPP: device_remove_uav");
	return S_OK;
}

HRESULT device_item::create_cpu_buffer(CONST INT name_hash, CONST INT element_size, CONST INT element_count)
{
	if (buffers.count(name_hash) != 0 )
		return E_INVALIDARG;

	D3D11_BUFFER_DESC desc;
	RtlZeroMemory(&desc, sizeof(D3D11_BUFFER_DESC));
	desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
	desc.Usage = D3D11_USAGE_STAGING;
	desc.StructureByteStride = element_size;
	desc.ByteWidth = element_size * element_count;

	ID3D11Buffer *p_buffer;
	CONST auto result = device->CreateBuffer(&desc, nullptr, &p_buffer);
	if(result == S_OK)
	{
		buffers.insert(std::map<INT, PTR<ID3D11Buffer>>::value_type(
			name_hash,
			PTR<ID3D11Buffer>(p_buffer)));
	}

	debug_print("CPP: device_create_cpu_buffer");
	return result;
}

HRESULT device_item::buffer_memcpy(CONST INT dest_name_hash, CONST INT src_name_hash)
{
	if (buffers.count(dest_name_hash) == 0 ||
		buffers.count(src_name_hash) == 0)
		return E_INVALIDARG;
	context->CopyResource(buffers[dest_name_hash].Get(), buffers[src_name_hash].Get());

	debug_print("CPP: device_buffer_memcpy");
	return S_OK;
}

HRESULT device_item::grab_buffer_data(CONST INT name_hash, VOID* destination)
{
	if (buffers.count(name_hash) == 0)
		return E_INVALIDARG;

	D3D11_BUFFER_DESC desc;
	RtlZeroMemory(&desc, sizeof(D3D11_BUFFER_DESC));
	buffers[name_hash]->GetDesc(&desc);
	D3D11_MAPPED_SUBRESOURCE subres;
	CONST auto result = context->Map(buffers[name_hash].Get(), NULL, D3D11_MAP_READ, NULL, &subres);
	if(result == S_OK)
	{
		memcpy(destination, subres.pData, desc.ByteWidth);
	}
	context->Unmap(buffers[name_hash].Get(), 0);

	debug_print("CPP: device_grab_buffer_data");
	return result;
}

HRESULT device_item::setup_context(CONST INT shader, INT* srvs, INT n_srvs, INT* uavs, INT n_uavs)
{
	if (compiled_shaders.count(shader) == 0)
		return E_INVALIDARG;

	std::vector<ID3D11UnorderedAccessView*> uav_vec;
	for(auto i = 0; i < n_uavs; i++)
	{
		if (this->uavs.count(uavs[i]) == 0)
			return E_INVALIDARG;
		uav_vec.push_back(this->uavs[uavs[i]].Get());
	}
	std::vector<ID3D11ShaderResourceView*> srv_vec;
	for (auto i = 0; i < n_srvs; i++)
	{
		if (this->srvs.count(srvs[i]) == 0)
			return E_INVALIDARG;
		srv_vec.push_back(this->srvs[srvs[i]].Get());
	}


	context->CSSetShader(compiled_shaders[shader].Get(), nullptr, NULL);
	context->CSSetShaderResources(0, n_srvs, srv_vec.data());
	context->CSSetUnorderedAccessViews(0, n_uavs, uav_vec.data(), nullptr);

	debug_print("CPP: device_setup_context");
	return S_OK;
}

VOID device_item::clear_context() const
{
	if (context)
	{
		ID3D11UnorderedAccessView* empty_uav[1] = { nullptr };
		ID3D11ShaderResourceView* empty_srv[1] = { nullptr };

		context->CSSetUnorderedAccessViews(0, 1, empty_uav, nullptr);
		context->CSSetShaderResources(0, 1, empty_srv);
		context->CSSetShader(nullptr, nullptr, 0);

		debug_print("CPP: device_clear_context");
	}
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

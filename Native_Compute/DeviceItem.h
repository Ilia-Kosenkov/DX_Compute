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

#pragma once

#include "Commons.h"


struct device_item
{
	std::map<INT, PTR<ID3D11Buffer>> buffers;
	std::map<INT, PTR<ID3D11UnorderedAccessView>> uavs;
	std::map < INT, PTR<ID3D11ShaderResourceView>> srvs;
	std::map<INT, PTR<ID3D11ComputeShader>> compiled_shaders;
	PTR<IDXGIAdapter> adapter;
	DXGI_ADAPTER_DESC descriptor{};
	PTR<ID3D11Device> device;
	PTR<ID3D11DeviceContext> context;


	VOID force_resource_release() noexcept;
	HRESULT create_cs_shader(_In_ INT name_hash, _In_ VOID* p_buffer, _In_ INT buffer_size);
	HRESULT create_structured_buffer(
		_In_ INT name_hash,
		_In_ VOID* p_data,
		_In_ INT element_size,
		_In_ INT element_count);
	HRESULT get_buffer_desc(_In_ INT name_hash) const;
	HRESULT remove_buffer(_In_ INT name_hash);
	HRESULT create_srv(_In_ INT name_hash, _In_ INT buff_name_hash);
	HRESULT create_uav(_In_ INT name_hash, _In_ INT buff_name_hash);
	HRESULT remove_srv(_In_ INT name_hash);
	HRESULT remove_uav(_In_ INT name_hash);
	HRESULT create_cpu_buffer(_In_ INT name_hash, _In_ INT element_size, _In_ INT element_count);
	HRESULT buffer_memcpy(_In_ INT dest, _In_ INT src);  // NOLINT(readability-inconsistent-declaration-parameter-name)
	HRESULT grab_buffer_data(_In_ INT name_hash, _Inout_ VOID* destination);
	HRESULT setup_context(_In_ INT shader, _In_ INT* srvs, _In_ INT n_srvs, _In_ INT* uavs, _In_ INT n_uavs);
	VOID clear_context() const;

	device_item();
	device_item(CONST device_item& other) noexcept;
	device_item(device_item&& other) noexcept;

	device_item& operator=(CONST device_item& other) noexcept;
	device_item& operator=(device_item&& other) noexcept;
	
	~device_item();
};


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
	std::map<INT, PTR<ID3D11ComputeShader>> compiled_shaders;
	PTR<IDXGIAdapter> adapter;
	DXGI_ADAPTER_DESC descriptor{};
	PTR<ID3D11Device> device;
	PTR<ID3D11DeviceContext> context;


	VOID force_resource_release() noexcept;
	HRESULT create_cs_shader(_In_ INT name_hash, _In_ VOID* p_buffer, _In_ INT buffer_size);

	device_item();
	device_item(CONST device_item& other) noexcept;
	device_item(device_item&& other) noexcept;

	device_item& operator=(CONST device_item& other) noexcept;
	device_item& operator=(device_item&& other) noexcept;
	
	~device_item();
};


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
#include "DeviceItem.h"

#define EXPORT extern "C" __declspec(dllexport)

// PreDefined flags
CONST UINT global_device_creation_flags = 
#ifdef _DEBUG
D3D11_CREATE_DEVICE_DEBUG |
#endif
D3D11_CREATE_DEVICE_DISABLE_GPU_TIMEOUT;

CONST D3D_FEATURE_LEVEL global_device_feature_levels[] = 
{ D3D_FEATURE_LEVEL_11_0 };


// Globals used in interop scenarios
PTR<IDXGIFactory> factory;
std::vector<device_item> devices;
INT selected_device_id = -1;


// Exports
EXPORT HRESULT get_factory(_Out_ VOID** pp_factory);
EXPORT HRESULT create_factory();
EXPORT HRESULT list_devices(_Out_ INT* n_dev);
EXPORT HRESULT free_resources();
EXPORT HRESULT get_adapter_descriptor(_In_ INT index, _Out_ BYTE* desc);
EXPORT HRESULT free_device_resources_forced(_In_ INT index);
EXPORT HRESULT create_cs_shader(_In_ INT dev_id, _In_ INT name_hash, _In_ VOID* p_buffer, _In_ INT buffer_size);
EXPORT HRESULT create_structured_buffer(
	_In_ INT dev_id,
	_In_ INT name_hash,
	_In_ VOID* p_data,
	_In_ INT element_size,
	_In_ INT element_count);
EXPORT HRESULT get_buffer_desc(_In_ INT dev_id, _In_ INT name_hash);
EXPORT HRESULT remove_buffer(_In_ INT dev_id, _In_ INT name_hash);
EXPORT HRESULT create_srv(_In_ INT dev_id, _In_ INT name_hash, _In_ INT buff_name_hash);
EXPORT HRESULT create_uav(_In_ INT dev_id, _In_ INT name_hash, _In_ INT buff_name_hash);
EXPORT HRESULT remove_srv(_In_ INT dev_id, _In_ INT name_hash);
EXPORT HRESULT remove_uav(_In_ INT dev_id, _In_ INT name_hash);
EXPORT HRESULT buffer_memcpy(_In_ INT dev_id, _In_ INT dest_name_hash, _In_ INT src_name_hash);
EXPORT HRESULT create_cpu_buffer(
	_In_ INT dev_id,
	_In_ INT name_hash,
	_In_ INT element_size,
	_In_ INT element_count);
EXPORT HRESULT grab_buffer_data(_In_ INT dev_id, _In_ INT name_hash, _Inout_ VOID* destination);
EXPORT HRESULT setup_context(_In_ INT dev_id, _In_ INT shader, _In_ INT* srvs, _In_ INT n_srvs, _In_ INT* uavs, _In_ INT n_uavs);
EXPORT HRESULT clear_context(_In_ INT dev_id);
EXPORT HRESULT dispatch(_In_ INT dev_id, _In_ UINT x, _In_ UINT y, _In_ UINT z);

EXPORT VOID add(_In_ INT a, _In_ INT b, _Out_ INT* c);
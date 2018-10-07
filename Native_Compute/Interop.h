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

EXPORT VOID add(_In_ INT a, _In_ INT b, _Out_ INT* c);
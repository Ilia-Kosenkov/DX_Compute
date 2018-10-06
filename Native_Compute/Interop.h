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

PTR<IDXGIFactory> factory;
std::vector<device_item> devices;

EXPORT HRESULT get_factory(_Out_ void** pp_factory);

EXPORT HRESULT create_factory();

EXPORT HRESULT list_devices(_Out_ size_t* n_dev);

EXPORT HRESULT free_factory();

EXPORT HRESULT get_adapter_descriptor(_In_ int index, _Out_ DXGI_ADAPTER_DESC* desc);

EXPORT void add(_In_ int a, _In_ int b, _Out_ int* c);
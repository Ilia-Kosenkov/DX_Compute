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
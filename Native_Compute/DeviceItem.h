#pragma once

#include "Commons.h"

struct device_item
{
	PTR<IDXGIAdapter> adapter;
	DXGI_ADAPTER_DESC descriptor;
};
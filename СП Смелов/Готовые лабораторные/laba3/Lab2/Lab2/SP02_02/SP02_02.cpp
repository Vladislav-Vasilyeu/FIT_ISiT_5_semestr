#include <iostream>
#include <Unknwn.h>
#include "../SP02_COM/IAdder.h"
#include "../SP02_COM/IMultiplier.h"

using std::cout;

#define IERR(s) std::cout << "error: " << s << std::endl;
#define IRES(s,r) std::cout << s << r << std::endl;

IAdder* pIAdder = nullptr;
IMultiplier* pIMultiplier = nullptr;

static const CLSID CLSID_CA =
{ 0x22499e77, 0xf234, 0x4f9c, { 0x81, 0x5e, 0xb5, 0x37, 0xb6, 0xc8, 0x65, 0x48 } };

void main() {
	IUnknown* pIUnknown = nullptr;

	CoInitialize(NULL);

	HRESULT hr0 = CoCreateInstance(CLSID_CA, NULL, CLSCTX_INPROC_SERVER, IID_IUnknown, (void**)&pIUnknown);

	if (FAILED(hr0)) {
		IERR("CoCreateInstance");
		return;
	}

	if (SUCCEEDED(pIUnknown->QueryInterface(IID_IMULTIPLIER, (void**)&pIMultiplier))) {
		double z = 0.0;

		if (FAILED(pIMultiplier->Mul(8.0, 3.0, z))) {
			IERR("IMultiplier->Mul")
		}
		else {
			IRES("8 * 3 = ", z)
		}

		if (FAILED(pIMultiplier->Div(9.0, 3.0, z))) {
			IERR("IMultiplier->Div")
		}
		else {
			IRES("9 / 3 = ", z)
		}
	}
	else {
		IERR("IMultiplier->QueryInterface")
	}

	pIMultiplier->Release();

	if (SUCCEEDED(pIUnknown->QueryInterface(IID_IADDER, (void**)&pIAdder))) {
		double z = 0.0;

		if (FAILED(pIAdder->Add(2.0, 3.0, z))) {
			IERR("IAdder->Add")
		}
		else {
			IRES("2 + 3 = ", z)
		}

		if (FAILED(pIAdder->Sub(6.0, 3.0, z))) {
			IERR("IAdder->Sub")
		}
		else {
			IRES("6 - 3 = ", z)
		}
	}
	else {
		IERR("IAdder->QueryInterface")
	}

	pIAdder->Release();
	pIUnknown->Release();

	CoUninitialize();
}

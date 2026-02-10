#include "pch.h"
#include "SP02.h"  
#include <Unknwn.h>
#include <stdexcept>
#include <iostream>

static const IID IID_IADDER =
{ 0xe04d1823, 0x1f2e, 0x114d, {0x22, 0xae, 0x4e, 0xab, 0x4c, 0xed, 0x64, 0xd8} };

static const IID IID_IMULTIPLIER =
{ 0xe0d12322, 0xad3a, 0x294e, {0x42, 0x98, 0x8d, 0x3c, 0x3e, 0xed, 0x64, 0xd8} };

static const CLSID CLSID_CA =
{ 0x22499e77, 0xf234, 0x4f9c, { 0x81, 0x5e, 0xb5, 0x37, 0xb6, 0xc8, 0x65, 0x48 } };

struct IAdder : public IUnknown {
    virtual HRESULT STDMETHODCALLTYPE Add(double x, double y, double& result) = 0;
    virtual HRESULT STDMETHODCALLTYPE Sub(double x, double y, double& result) = 0;
};

struct IMultiplier : public IUnknown {  
    virtual HRESULT STDMETHODCALLTYPE Mul(double x, double y, double& result) = 0;
    virtual HRESULT STDMETHODCALLTYPE Div(double x, double y, double& result) = 0;
};

static ULONG cObjects = 0;
#define IERR(s) std::cout << "error: " << s << std::endl;
#define IRES(s,r) std::cout << s << r << std::endl;

SP02LIB SP02::Init() {
    IUnknown* pIUnknown = nullptr;

    try {
        if (cObjects == 0) {
            if (FAILED(CoInitialize(NULL))) {
                throw std::runtime_error("CoInitialize");
            }
        }

        if (FAILED(CoCreateInstance(CLSID_CA, NULL, CLSCTX_INPROC_SERVER, IID_IUnknown, (void**)&pIUnknown))) {
            throw std::runtime_error("CreateInstance");
        }

        InterlockedIncrement(&cObjects);
        return pIUnknown;
    }
    catch (std::runtime_error error) {
        IERR(error.what());
        return nullptr;
    }
}

double SP02::Adder::Add(SP02LIB h, double x, double y) {
    double z = 0.0;
    IAdder* pIAdder = nullptr;

    try {
        if (FAILED(((IUnknown*)h)->QueryInterface(IID_IADDER, (void**)&pIAdder))) {
            throw std::runtime_error("QueryInterface");
        }

        if (FAILED(pIAdder->Add(x, y, z)))
            throw std::runtime_error("Add");
    }
    catch (std::runtime_error error) {
        IERR(error.what());
    }

    if (pIAdder != nullptr) {
        pIAdder->Release();
    }
    return z;
}

double SP02::Adder::Sub(SP02LIB h, double x, double y) {
    double z = 0.0;
    IAdder* pIAdder = nullptr;

    try {
        if (FAILED(((IUnknown*)h)->QueryInterface(IID_IADDER, (void**)&pIAdder))) {
            throw std::runtime_error("QueryInterface");
        }

        if (FAILED(pIAdder->Sub(x, y, z)))
            throw std::runtime_error("Sub");
    }
    catch (std::runtime_error error) {
        IERR(error.what());
    }

    if (pIAdder != nullptr) {
        pIAdder->Release();
    }
    return z;
}

double SP02::Multiplier::Mul(SP02LIB h, double x, double y) {
    IMultiplier* pIMultiplier = nullptr;
    double z = 0.0;

    try {
        if (FAILED(((IUnknown*)h)->QueryInterface(IID_IMULTIPLIER, (void**)&pIMultiplier))) {
            throw std::runtime_error("QueryInterface");
        }

        if (FAILED(pIMultiplier->Mul(x, y, z)))
            throw std::runtime_error("Mul");

        pIMultiplier->Release();
    }
    catch (std::runtime_error error) {
        IERR(error.what());
    }

    return z;
}

double SP02::Multiplier::Div(SP02LIB h, double x, double y) {
    try {
        IMultiplier* pIMultiplier = nullptr;
        double z = 0.0;

        if (FAILED(((IUnknown*)h)->QueryInterface(IID_IMULTIPLIER, (void**)&pIMultiplier))) {
            throw std::runtime_error("QueryInterface");
        }

        if (FAILED(pIMultiplier->Div(x, y, z)))
            throw std::runtime_error("Div");

        pIMultiplier->Release();
        return z;
    }
    catch (std::runtime_error error) {
        IERR(error.what());
        return 0;
    }
}

void SP02::Dispose(SP02LIB h) {
    ((IUnknown*)h)->Release();
    InterlockedDecrement(&cObjects);

    if (cObjects == 0)
        CoUninitialize();
}
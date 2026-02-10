#include "pch.h"
#include <fstream>
#include <string>

using namespace std;

extern ULONG g_lObjs;
extern ULONG g_lLocks;

SP02::SP02() : counter(1) {
    InterlockedIncrement(&g_lObjs);
}

SP02::~SP02() {
    InterlockedDecrement(&g_lObjs);
}

HRESULT __stdcall SP02::QueryInterface(const IID& iid, void** ppv) {
    if (iid == IID_IUnknown) {
        *ppv = (IUnknown*)(IAdder*)this;
    }
    else if (iid == IID_IADDER) {
        *ppv = (IAdder*)this;
    }
    else if (iid == IID_IMULTIPLIER) {
        *ppv = (IMultiplier*)this;
    }
    else {
        *ppv = NULL;
        return E_NOINTERFACE;
    }

    AddRef();
    return S_OK;
}

ULONG __stdcall SP02::AddRef() {
    ULONG result = InterlockedIncrement(&counter);
    return result;
}

ULONG __stdcall SP02::Release() {
    ULONG result = InterlockedDecrement(&counter);
    if (result == 0) {
        delete this;
        return 0;
    }
    return result;
}

HRESULT __stdcall SP02::Add(const double x, const double y, double& c) {
    c = x + y;
    return S_OK;
}

HRESULT __stdcall SP02::Sub(const double x, const double y, double& c) {
    c = x - y;
    return S_OK;
}

HRESULT __stdcall SP02::Mul(const double x, const double y, double& c) {
    c = x * y;
    return S_OK;
}

HRESULT __stdcall SP02::Div(const double x, const double y, double& c) {
    c = x / y;
    return S_OK;
}
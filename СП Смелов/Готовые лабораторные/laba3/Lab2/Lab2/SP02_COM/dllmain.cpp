#include "pch.h"
#include <Windows.h>
#include <combaseapi.h>
#include "MathFactory.h"
#include <cstdio>

using namespace std;

extern ULONG g_lObjs;   
extern ULONG g_lLocks;

HMODULE hmodule;

static const CLSID CLSID_OS12 =
{ 0x22499e77, 0xf234, 0x4f9c, { 0x81, 0x5e, 0xb5, 0x37, 0xb6, 0xc8, 0x65, 0x48 } };

const WCHAR* FNAME = L"SP02_COM.dll";
const WCHAR* VerInd = L"SP02.Component.1.0";
const WCHAR* ProgId = L"SP02.Component.1";


BOOL APIENTRY DllMain(
    HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        hmodule = hModule;
        break;
    }
    return TRUE;
}

HRESULT __declspec(dllexport) DllInstall(bool b, PCWSTR s)
{
    return S_OK;
}

HRESULT __declspec(dllexport) DllRegisterServer() {
    return RegisterServer(hmodule, CLSID_OS12, FNAME, VerInd, ProgId);
}

HRESULT __declspec(dllexport) DllUnregisterServer() {
    return UnregisterServer(CLSID_OS12, VerInd, ProgId);
}


STDAPI DllCanUnloadNow()
{
    if ((g_lObjs == 0) && (g_lLocks == 0))
        return S_OK;
    else
        return S_FALSE;
}

STDAPI DllGetClassObject(const CLSID& clsid, const IID& iid, LPVOID* ppv) {
    HRESULT rc = E_UNEXPECTED;
    MathFactory* pF;
    if (clsid != CLSID_OS12) rc = CLASS_E_CLASSNOTAVAILABLE;
    else if ((pF = new MathFactory()) == NULL) rc = E_OUTOFMEMORY;
    else {
        rc = pF->QueryInterface(iid, ppv);
        pF->Release();
    }
    return rc;
}
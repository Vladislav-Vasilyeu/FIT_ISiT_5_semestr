#pragma once
#include <assert.h>
#include <objbase.h>

void CLSIDtochar(
	const CLSID& clsid,       
	WCHAR* szCLSID,
	int length);


LONG recursiveDeleteKey(
	HKEY hKeyParent,        
	const WCHAR* lpszKeyChild);

BOOL setKeyAndValue(
	const WCHAR* szKey,        
	const WCHAR* szSubkey,     
	const WCHAR* szValue);

HRESULT RegisterServer(
	HMODULE hModule,            
	const CLSID& clsid,        
	const WCHAR* szFriendlyName, 
	const WCHAR* szVerIndProgID, 
	const WCHAR* szProgID);

HRESULT UnregisterServer(
	const CLSID& clsid,
	const WCHAR* szVerIndProgID,
	const WCHAR* szProgID);

#pragma once
#include <Windows.h>

#ifdef OS11_HTAPI_EXPORTS
#define OS11_HTAPI_API __declspec(dllexport)
#else
#define OS11_HTAPI_API __declspec(dllimport)
#endif // OS11_HTAPI_EXPORTS
//extern C; ///

namespace HT    // HT API
{
	// API HT - реализация интерфейса для хранения в мэппинге и т.д.

	OS11_HTAPI_API struct HTHANDLE    // структура дескриптора HT
	{
		HTHANDLE();
		HTHANDLE(int Capacity, int SecSnapshotInterval, int MaxKeyLength, int MaxPayloadLength, const char FileName[512]);
		int     Capacity;
		int     SecSnapshotInterval;
		int     MaxKeyLength;
		int     MaxPayloadLength;
		char    FileName[512];
		HANDLE  File;
		HANDLE  FileMapping;
		LPVOID  Addr;
		char    LastErrorMessage[512];
		time_t  lastsnaptime;
	};

	OS11_HTAPI_API struct Element   // элемент хранилища
	{
		OS11_HTAPI_API Element();
		OS11_HTAPI_API Element(const void* key, int keylength);                                             // for Get
		OS11_HTAPI_API Element(const void* key, int keylength, const void* payload, int  payloadlength);    // for Insert
		OS11_HTAPI_API Element(Element* oldelement, const void* newpayload, int  newpayloadlength);         // for update
		const void* key;
		int             keylength;
		const void* payload;
		int             payloadlength;
	};

	OS11_HTAPI_API HTHANDLE* Create(int	Capacity, int SecSnapshotInterval, int MaxKeyLength, int MaxPayloadLength, const char FileName[512]); 	

	OS11_HTAPI_API HTHANDLE* Open(const char    FileName[512]); 	

	OS11_HTAPI_API BOOL Snap(const HTHANDLE* hthandle);


	OS11_HTAPI_API BOOL Close(const HTHANDLE* hthandle);	   


	OS11_HTAPI_API BOOL Insert(const HTHANDLE* hthandle, const Element* element);	


	OS11_HTAPI_API BOOL Delete(const HTHANDLE* hthandle, const Element* element);	

	OS11_HTAPI_API Element* Get(const HTHANDLE* hthandle, const Element* element); 	


	OS11_HTAPI_API BOOL Update(const HTHANDLE* hthandle, const Element* oldelement, const void* newpayload, int newpayloadlength); 	

	OS11_HTAPI_API char* GetLastError(HTHANDLE* ht);

	OS11_HTAPI_API void print(const Element* element);


};

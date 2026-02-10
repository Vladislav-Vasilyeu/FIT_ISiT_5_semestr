#include <string>
#include <sstream>
#include "../OS11_HTAPI/pch.h"
#include "../OS11_HTAPI/HT.h"

using namespace std;

string intToString(int number);

int main(int argc, char* argv[])
{
	srand(static_cast<unsigned int>(time(nullptr)));
	try
	{
		#ifdef _WIN64
		HMODULE hmdll = LoadLibrary(L"D:\\Универ\\СП Смелов\\Готовые лабораторные\\lab2\\Lab4\\x64\\Debug\\OS11_HTAPI.dll");
		#else
		HMODULE hmdll = LoadLibrary(L"D:\\Универ\\СП Смелов\\Готовые лабораторные\\lab2\\Lab4\\x64\\Debug\\OS11_HTAPI.dll");
		#endif

		if (!hmdll)
		{
			DWORD err = GetLastError();
			cout << "-- LoadLibrary failed, error code: " << err << endl;
			throw "-- LoadLibrary failed";
		}
		cout << "-- LoadLibrary success" << endl;
	 
		auto open = (ht::HtHandle * (*)(const wchar_t*, bool)) GetProcAddress(hmdll, "open");
		auto insert = (BOOL(*)(ht::HtHandle*, const ht::Element*)) GetProcAddress(hmdll, "insert");
		auto createInsertElement = (ht::Element * (*)(const void*, int, const void*, int)) GetProcAddress(hmdll, "createInsertElement");

		ht::HtHandle* ht = open(L"HT1.ht", false);
		if (ht)
			cout << "-- open: success" << endl;
		else
			throw "-- open: error";

		while (true) {
			int numberKey = rand() % 50;
			string key = intToString(numberKey);
			cout << key << endl;

			ht::Element* element = createInsertElement(key.c_str(), key.length() + 1, "0", 2);
			if (insert(ht, element))
				cout << "-- insert: success" << endl;
			else
				cout << "-- insert: error" << endl;

			delete element;

			Sleep(1000);
		}
	}
	catch (const char* msg)
	{
		cout << msg << endl;
	}
}

string intToString(int number)
{
	stringstream convert;
	convert << number;

	return convert.str();
}

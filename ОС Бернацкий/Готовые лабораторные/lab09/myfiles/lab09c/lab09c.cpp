#include <windows.h>
#include <string>
#include <vector>

static bool IsTempFile(const std::wstring& name)
{
    if (name.empty()) return false;
    return (name.find(L".goutputstream-") != std::wstring::npos ||
            name.find(L".swp") != std::wstring::npos ||
            name.back() == L'~');
}

// Печать wide строки в консоль. Если STDOUT не консоль (перенаправление), выводим в UTF-8.
static void PrintWide(HANDLE hOut, const std::wstring& wstr)
{
    if (hOut == INVALID_HANDLE_VALUE) return;

    DWORD fileType = GetFileType(hOut);
    if (fileType == FILE_TYPE_CHAR) // обычная консоль
    {
        DWORD written = 0;
        WriteConsoleW(hOut, wstr.c_str(), static_cast<DWORD>(wstr.length()), &written, NULL);
    }
    else // перенаправлено в файл/pipe — конвертируем в UTF-8 и пишем через WriteFile
    {
        int needed = WideCharToMultiByte(CP_UTF8, 0, wstr.c_str(), (int)wstr.length(), NULL, 0, NULL, NULL);
        if (needed > 0)
        {
            std::vector<char> buf(needed);
            WideCharToMultiByte(CP_UTF8, 0, wstr.c_str(), (int)wstr.length(), buf.data(), needed, NULL, NULL);
            DWORD written = 0;
            WriteFile(hOut, buf.data(), (DWORD)needed, &written, NULL);
        }
    }
}

static void PrintDirectoryContents(HANDLE hOut, const std::wstring& path)
{
    std::wstring search = path;
    if (!search.empty() && search.back() != L'\\' && search.back() != L'/') search += L"\\";
    search += L"*";

    WIN32_FIND_DATAW fd;
    HANDLE hFind = FindFirstFileW(search.c_str(), &fd);
    if (hFind == INVALID_HANDLE_VALUE)
    {
        PrintWide(hOut, L"Не удалось открыть каталог: " + path + L"\r\n");
        return;
    }

    PrintWide(hOut, L"Содержимое каталога " + path + L":\r\n");
    do
    {
        std::wstring name = fd.cFileName;
        if (name == L"." || name == L"..") continue;
        if (fd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
            PrintWide(hOut, L"[DIR]  " + name + L"\r\n");
        else
            PrintWide(hOut, L"[FILE] " + name + L"\r\n");
    } while (FindNextFileW(hFind, &fd) != 0);

    FindClose(hFind);
}

int wmain(int argc, wchar_t* argv[])
{
    HANDLE hOut = GetStdHandle(STD_OUTPUT_HANDLE);
    if (argc < 2)
    {
        PrintWide(hOut, L"Использование: ");
        PrintWide(hOut, L"lab09c <каталог>\r\n");
        return 1;
    }

    const std::wstring path = argv[1];

    DWORD attrs = GetFileAttributesW(path.c_str());
    if (attrs == INVALID_FILE_ATTRIBUTES || !(attrs & FILE_ATTRIBUTE_DIRECTORY))
    {
        PrintWide(hOut, L"Каталог не существует: " + path + L"\r\n");
        return 1;
    }

    PrintDirectoryContents(hOut, path);

    HANDLE hDir = CreateFileW(
        path.c_str(),
        FILE_LIST_DIRECTORY,
        FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
        NULL,
        OPEN_EXISTING,
        FILE_FLAG_BACKUP_SEMANTICS,
        NULL);

    if (hDir == INVALID_HANDLE_VALUE)
    {
        PrintWide(hOut, L"CreateFileW failed: ");
        // вывести код ошибки как число
        DWORD err = GetLastError();
        wchar_t buf[64];
        swprintf_s(buf, L"%lu\r\n", err);
        PrintWide(hOut, buf);
        return 1;
    }

    PrintWide(hOut, L"\r\nОтслеживание изменений (Ctrl+C для выхода)...\r\n");

    std::wstring lastRenameOld;
    const DWORD bufSize = 16 * 1024;
    std::vector<BYTE> buffer(bufSize);

    while (true)
    {
        DWORD bytesReturned = 0;
        BOOL ok = ReadDirectoryChangesW(
            hDir,
            buffer.data(),
            (DWORD)buffer.size(),
            TRUE,
            FILE_NOTIFY_CHANGE_FILE_NAME | FILE_NOTIFY_CHANGE_DIR_NAME |
            FILE_NOTIFY_CHANGE_ATTRIBUTES | FILE_NOTIFY_CHANGE_SIZE |
            FILE_NOTIFY_CHANGE_LAST_WRITE | FILE_NOTIFY_CHANGE_CREATION,
            &bytesReturned,
            NULL,
            NULL);

        if (!ok)
        {
            DWORD err = GetLastError();
            wchar_t buf[128];
            swprintf_s(buf, L"ReadDirectoryChangesW failed: %lu\r\n", err);
            PrintWide(hOut, buf);
            break;
        }

        DWORD offset = 0;
        while (offset < bytesReturned)
        {
            FILE_NOTIFY_INFORMATION* fni = reinterpret_cast<FILE_NOTIFY_INFORMATION*>(buffer.data() + offset);
            int wlen = fni->FileNameLength / sizeof(WCHAR);
            std::wstring name(fni->FileName, fni->FileName + wlen);

            if (!IsTempFile(name))
            {
                switch (fni->Action)
                {
                case FILE_ACTION_ADDED:
                    PrintWide(hOut, L"[ADDED] " + name + L"\r\n");
                    break;
                case FILE_ACTION_REMOVED:
                    PrintWide(hOut, L"[REMOVED] " + name + L"\r\n");
                    break;
                case FILE_ACTION_MODIFIED:
                    PrintWide(hOut, L"[MODIFIED] " + name + L"\r\n");
                    break;
                case FILE_ACTION_RENAMED_OLD_NAME:
                    lastRenameOld = name;
                    break;
                case FILE_ACTION_RENAMED_NEW_NAME:
                    if (!lastRenameOld.empty())
                    {
                        PrintWide(hOut, L"[RENAMED] " + lastRenameOld + L" -> " + name + L"\r\n");
                        lastRenameOld.clear();
                    }
                    else
                    {
                        PrintWide(hOut, L"[RENAMED?] " + name + L"\r\n");
                    }
                    break;
                default:
                    {
                        wchar_t buf[64];
                        swprintf_s(buf, L"[UNKNOWN ACTION %u] %s\r\n", fni->Action, name.c_str());
                        PrintWide(hOut, buf);
                    }
                    break;
                }
            }

            if (fni->NextEntryOffset == 0) break;
            offset += fni->NextEntryOffset;
        }
    }

    CloseHandle(hDir);
    return 0;
}

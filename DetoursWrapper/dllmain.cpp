#include <Windows.h>
#include <vector> // std::size

#ifdef _WIN64
#pragma comment(lib, "../Detours/detours64.lib")
#define _AMD64_
#else
#pragma comment(lib, "../Detours/detours32.lib")
#define _X86_
#endif
#include "../Detours/detours.h"

extern "C" {
    __declspec(dllexport) int WINAPI RunWithDll(
        const wchar_t* programPath,
        const wchar_t* args,
        const wchar_t* workingDirectory,
        const char* dllPath)
    {
        STARTUPINFO si = { sizeof(si) };
        PROCESS_INFORMATION pi = { 0 };
        return DetourCreateProcessWithDllExW(
            programPath,
            (wchar_t*)args,
            NULL,
            NULL,
            TRUE,
            CREATE_DEFAULT_ERROR_MODE,
            NULL,
            workingDirectory,
            &si,
            &pi,
            dllPath,
            NULL
        ) ? 0 : 1;
    }

    __declspec(dllexport) void WINAPI GetRawInputInfo(HRAWINPUT hRawInput, HANDLE* hDevice, DWORD* type) {
        UINT size = 0;
        GetRawInputData(hRawInput, RID_INPUT, NULL, &size, sizeof(RAWINPUTHEADER));
        char* buffer = new char[size];
        GetRawInputData(hRawInput, RID_INPUT, buffer, &size, sizeof(RAWINPUTHEADER));
        RAWINPUT* rawinput = (RAWINPUT*)buffer;
        *hDevice = rawinput->header.hDevice;
        *type = rawinput->header.dwType;
        delete[] buffer;
    }

    __declspec(dllexport) void WINAPI ListenToRawInput(HWND hWnd) {
        RAWINPUTDEVICE rids[] = {
            { 0x01, 0x02, NULL, hWnd }, // Mouse
            { 0x01, 0x06, NULL, hWnd }, // Keyboard
        };
        RegisterRawInputDevices(rids, (UINT)std::size(rids), sizeof(RAWINPUTDEVICE));
    }
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}


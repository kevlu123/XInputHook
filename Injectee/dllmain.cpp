#include "dllmain.h"

#include <string>
#include <vector>
#include <unordered_map>
#include <mutex>
#include <span>

#include <Windows.h>
#include <Xinput.h>
#pragma comment(lib, "Xinput.lib")

#ifdef _WIN64
#pragma comment(lib, "../Detours/detours64.lib")
#define _AMD64_
#else
#pragma comment(lib, "../Detours/detours32.lib")
#define _X86_
#endif
#include "../Detours/detours.h"

#include "Lua/lua.hpp"
#include "myxinput.h"
#include "luaapi.h"

static std::mutex writeMutex;
static std::vector<uint8_t> writeBuffer;
static HANDLE hReadPipe = INVALID_HANDLE_VALUE;
static HANDLE hWritePipe = INVALID_HANDLE_VALUE;

static void* functionStorage[100];
static size_t functionStorageCount = 0;

static thread_local std::unordered_map<HWND, WNDPROC> oldWindowProcs;

static auto RealCreateWindowExA = CreateWindowExA;
static auto RealCreateWindowExW = CreateWindowExW;
static auto RealGetAsyncKeyState = GetAsyncKeyState;
static auto RealGetKeyState = GetKeyState;

enum class Opcode {
    // Client -> Server
    PrintMessage = 1,
    // Server -> Client
    RunScript = 100,
    SetDeviceNumbers = 101,
};

static void Error(const char* message) {
    MessageBoxA(NULL, message, "Error", MB_ICONERROR | MB_OK);
    ExitProcess(1);
}

static void WritePipe(Opcode opcode, const void* data, size_t size) {
    std::lock_guard<std::mutex> lock(writeMutex);
    DWORD messageLen = 5 + (DWORD)size;
    writeBuffer.insert(writeBuffer.end(), (uint8_t*)&messageLen, (uint8_t*)&messageLen + 4);
    writeBuffer.push_back((uint8_t)opcode);
    writeBuffer.insert(writeBuffer.end(), (uint8_t*)data, (uint8_t*)data + size);
}

void PrintMessage(const char* message) {
    WritePipe(Opcode::PrintMessage, message, strlen(message));
}

static void OnRunScriptMessage(std::span<uint8_t> data) {
    uint8_t empty[1]{};
    if (data.empty()) {
        data = { empty, 1 };
    }
    data.back() = '\0';
    if (LuaInit((const char*)data.data())) {
        PrintMessage("Script loaded.\n");
    } else {
        PrintMessage("Failed to load script.\n");
    }
}

static void OnSetDeviceNumbersMessage(std::span<uint8_t> data) {
    if (data.size() < 4) {
        return;
    }

    int* ints = (int*)data.data();
    int len = ints[0];
    if (data.size() != 4 + (size_t)len * 8) {
        return;
    }

    std::vector<HANDLE> hDevices;
    std::vector<DeviceNumber> deviceNumbers;
    for (int i = 0; i < len; i++) {
        hDevices.push_back((HANDLE)(intptr_t)ints[i * 2 + 1]);
        deviceNumbers.push_back((DeviceNumber)ints[i * 2 + 2]);
    }
    LuaSetDeviceNumbers(hDevices.data(), deviceNumbers.data(), len);
}

static LRESULT WINAPI CustomWindowProc(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam) {
    switch (Msg) {
    case WM_INPUT:
        LuaOnInput((HRAWINPUT)lParam);
        break;
    case WM_KEYDOWN:
    case WM_KEYUP:
        if (LuaIsMutedKey((int)wParam)) {
            return 0;
        }
        break;
    }

    auto it = oldWindowProcs.find(hWnd);
    if (it != oldWindowProcs.end()) {
        return (it->second)(hWnd, Msg, wParam, lParam);
    } else {
        return DefWindowProcW(hWnd, Msg, wParam, lParam);
    }
}

static HWND WINAPI MyCreateWindowExA(
    DWORD     dwExStyle,
    LPCSTR    lpClassName,
    LPCSTR    lpWindowName,
    DWORD     dwStyle,
    int       X,
    int       Y,
    int       nWidth,
    int       nHeight,
    HWND      hWndParent,
    HMENU     hMenu,
    HINSTANCE hInstance,
    LPVOID    lpParam)
{
    HWND hWnd = RealCreateWindowExA(
        dwExStyle,
        lpClassName,
        lpWindowName,
        dwStyle,
        X,
        Y,
        nWidth,
        nHeight,
        hWndParent,
        hMenu,
        hInstance,
        lpParam);

    if (hWnd != nullptr) {
        oldWindowProcs[hWnd] = (WNDPROC)SetWindowLongPtrA(hWnd, GWLP_WNDPROC, (LONG_PTR)CustomWindowProc);
    }

    return hWnd;
}

static HWND WINAPI MyCreateWindowExW(
    DWORD     dwExStyle,
    LPCWSTR   lpClassName,
    LPCWSTR   lpWindowName,
    DWORD     dwStyle,
    int       X,
    int       Y,
    int       nWidth,
    int       nHeight,
    HWND      hWndParent,
    HMENU     hMenu,
    HINSTANCE hInstance,
    LPVOID    lpParam) {
    HWND hWnd = RealCreateWindowExW(
        dwExStyle,
        lpClassName,
        lpWindowName,
        dwStyle,
        X,
        Y,
        nWidth,
        nHeight,
        hWndParent,
        hMenu,
        hInstance,
        lpParam);

    if (hWnd != nullptr) {
        oldWindowProcs[hWnd] = (WNDPROC)SetWindowLongPtrW(hWnd, GWLP_WNDPROC, (LONG_PTR)CustomWindowProc);
    }

    return hWnd;
}

static SHORT WINAPI MyGetAsyncKeyState(int vKey) {
    if (LuaIsMutedKey(vKey)) {
        return 0;
    }
    return RealGetAsyncKeyState(vKey);
}

static SHORT WINAPI MyGetKeyState(int vKey) {
    if (LuaIsMutedKey(vKey)) {
        return 0;
    }
    return RealGetKeyState(vKey);
}

static void PipeReader() {
    hReadPipe = CreateFileW(
        L"\\\\.\\pipe\\XInputHookPipeServerToClient",
        GENERIC_READ,
        0,
        NULL,
        OPEN_EXISTING,
        0,
        NULL);
    if (hReadPipe == INVALID_HANDLE_VALUE) {
        Error("Failed to open read pipe");
    }

    std::vector<uint8_t> buffer(1024 * 1024);
    size_t bufferPos = 0;
    DWORD bytesRead;
    while (ReadFile(hReadPipe, buffer.data() + bufferPos, (DWORD)(buffer.size() - bufferPos), &bytesRead, NULL)) {
        bufferPos += bytesRead;
        if (bufferPos < 5) {
            continue;
        }
        uint32_t messageLen;
        memcpy(&messageLen, buffer.data(), sizeof(messageLen));
        if (bufferPos < messageLen) {
            continue;
        } else if (messageLen > buffer.size()) {
            Error("Message too large");
        }

        Opcode opcode = (Opcode)buffer[4];
        std::span<uint8_t> data(buffer.data() + 5, messageLen - 5);
        switch (opcode) {
        case Opcode::RunScript: OnRunScriptMessage(data); break;
        case Opcode::SetDeviceNumbers: OnSetDeviceNumbersMessage(data); break;
        default: Error("<Received unknown opcode>"); break;
        }

        buffer.erase(buffer.begin(), buffer.begin() + messageLen);
        bufferPos -= messageLen;
    }
}

static void PipeWriter() {
    hWritePipe = CreateFileW(
        L"\\\\.\\pipe\\XInputHookPipeClientToServer",
        GENERIC_WRITE,
        0,
        NULL,
        OPEN_EXISTING,
        0,
        NULL);
    if (hWritePipe == INVALID_HANDLE_VALUE) {
        Error("Failed to open write pipe");
    }

    while (true) {
        std::unique_lock<std::mutex> lock(writeMutex);
        if (!writeBuffer.empty()) {
            DWORD bytesWritten;
            if (!WriteFile(hWritePipe, writeBuffer.data(), (DWORD)writeBuffer.size(), &bytesWritten, NULL)) {
                return;
            }
            writeBuffer.erase(writeBuffer.begin(), writeBuffer.begin() + bytesWritten);
        } else {
            lock.unlock();
            Sleep(5);
        }
    }
}

static DWORD WINAPI InitialThreadRoutine(LPVOID lpParam) {
    RAWINPUTDEVICE rids[] = {
        { 0x01, 0x02, NULL, NULL }, // Mouse
        { 0x01, 0x06, NULL, NULL }, // Keyboard
    };
    RegisterRawInputDevices(rids, (UINT)std::size(rids), sizeof(RAWINPUTDEVICE));

    HANDLE hThread = CreateThread(
        NULL,
        0,
        [](LPVOID) -> DWORD { PipeWriter(); return 0; },
        NULL,
        0,
        NULL);
    if (hThread != NULL) {
        CloseHandle(hThread);
    }

    PipeReader();
    return 0;
}

static void HookFunction(void* target, void* detour) {
    functionStorage[functionStorageCount] = target;
    DetourAttach(&functionStorage[functionStorageCount], detour);
    functionStorageCount++;
}

static void HookDllFunction(HMODULE hModule, LPCSTR symbol, void* detour) {
    if (auto f = GetProcAddress(hModule, symbol)) {
        HookFunction(f, detour);
    }
}

static void HookXInputDll(const wchar_t* dll) {
    HMODULE hModule = LoadLibraryW(dll);
    if (hModule == NULL) {
        return;
    }

    HookDllFunction(hModule, "XInputGetState", MyXInputGetState);
    HookDllFunction(hModule, (LPCSTR)100, MyXInputGetState);
    HookDllFunction(hModule, "XInputSetState", MyXInputSetState);
    HookDllFunction(hModule, "XInputGetCapabilities", MyXInputGetCapabilities);
    HookDllFunction(hModule, "XInputGetKeystroke", MyXInputGetKeystroke);
    HookDllFunction(hModule, "XInputGetBatteryInformation", MyXInputGetBatteryInformation);
    HookDllFunction(hModule, "XInputEnable", MyXInputEnable);
    HookDllFunction(hModule, "XInputGetDSoundAudioDeviceGuids", MyXInputGetDSoundAudioDeviceGuids);
    HookDllFunction(hModule, "XInputGetAudioDeviceIds", MyXInputGetAudioDeviceIds);
}

__declspec(dllexport)
void WINAPI MyDetourFinishHelperProcess(HWND hWnd, HINSTANCE hInst, LPSTR args, INT cmdShow) {
    DetourFinishHelperProcess(hWnd, hInst, args, cmdShow);
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
    if (DetourIsHelperProcess()) {
        return TRUE;
    }

    switch (ul_reason_for_call) {
    case DLL_PROCESS_ATTACH: {
        DetourRestoreAfterWith();
        DetourTransactionBegin();
        DetourUpdateThread(GetCurrentThread());
        DetourAttach(&(PVOID&)RealCreateWindowExA, MyCreateWindowExA);
        DetourAttach(&(PVOID&)RealCreateWindowExW, MyCreateWindowExW);
        HookFunction(&(PVOID&)RealGetAsyncKeyState, MyGetAsyncKeyState);
        HookFunction(&(PVOID&)RealGetKeyState, MyGetKeyState);
        HookXInputDll(L"xinput1_1.dll");
        HookXInputDll(L"xinput1_2.dll");
        HookXInputDll(L"xinput1_3.dll");
        HookXInputDll(L"xinput1_4.dll");
        HookXInputDll(L"xinput9_1_0.dll");
        DetourTransactionCommit();

        // Shouldn't really be calling CreateThread in DllMain but whatever
        HANDLE hThread = CreateThread(
            NULL,
            0,
            InitialThreadRoutine,
            NULL,
            0,
            NULL);
        if (hThread != NULL) {
            CloseHandle(hThread);
        }
        break;
    }
    case DLL_THREAD_ATTACH:
        break;
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
        break;
    }

    return TRUE;
}


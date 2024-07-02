#pragma once
#include "myxinput.h"

#include <mutex>
#include <array>

static std::mutex statesMutex;
static std::array<std::optional<XINPUT_STATE>, MAX_XINPUT_CONTROLLERS> states{};
static bool xinputEnabled = true;

void SetControllerState(size_t index, std::optional<XINPUT_STATE> state) {
    std::lock_guard<std::mutex> lock(statesMutex);
    states[index] = state;
}

void WINAPI MyXInputEnable(BOOL enable) WIN_NOEXCEPT {
    std::lock_guard<std::mutex> lock(statesMutex);
    xinputEnabled = enable;
}

DWORD WINAPI MyXInputGetState(DWORD dwUserIndex, XINPUT_STATE* pState) WIN_NOEXCEPT {
    *pState = {};
    if (dwUserIndex < 0 || dwUserIndex >= MAX_XINPUT_CONTROLLERS) {
        return ERROR_DEVICE_NOT_CONNECTED;
    }

    std::lock_guard<std::mutex> lock(statesMutex);
    if (!states[dwUserIndex].has_value()) {
        return ERROR_DEVICE_NOT_CONNECTED;
    }

    if (xinputEnabled) {
        *pState = states[dwUserIndex].value();
    }
    return ERROR_SUCCESS;
}

DWORD WINAPI MyXInputSetState(DWORD dwUserIndex, XINPUT_VIBRATION* pVibration) WIN_NOEXCEPT {
    *pVibration = {};
    if (dwUserIndex < 0 || dwUserIndex >= MAX_XINPUT_CONTROLLERS) {
        return ERROR_DEVICE_NOT_CONNECTED;
    }

    std::lock_guard<std::mutex> lock(statesMutex);
    if (!states[dwUserIndex].has_value()) {
        return ERROR_DEVICE_NOT_CONNECTED;
    }

    return ERROR_SUCCESS;
}

DWORD WINAPI MyXInputGetCapabilities(DWORD dwUserIndex, DWORD dwFlags, XINPUT_CAPABILITIES* pCapabilities) WIN_NOEXCEPT {
    *pCapabilities = {};
    if (dwUserIndex < 0 || dwUserIndex >= MAX_XINPUT_CONTROLLERS) {
        return ERROR_DEVICE_NOT_CONNECTED;
    }

    std::lock_guard<std::mutex> lock(statesMutex);
    if (!states[dwUserIndex].has_value()) {
        return ERROR_DEVICE_NOT_CONNECTED;
    }

    pCapabilities->Type = XINPUT_DEVTYPE_GAMEPAD;
    pCapabilities->SubType = XINPUT_DEVSUBTYPE_GAMEPAD;
    pCapabilities->Gamepad.wButtons = 0xF3FF;
    return ERROR_SUCCESS;
}

DWORD WINAPI MyXInputGetKeystroke(DWORD dwUserIndex, DWORD dwReserved, PXINPUT_KEYSTROKE pKeystroke) WIN_NOEXCEPT {
    if (dwUserIndex < 0 || dwUserIndex >= MAX_XINPUT_CONTROLLERS) {
        return ERROR_DEVICE_NOT_CONNECTED;
    }

    std::lock_guard<std::mutex> lock(statesMutex);
    if (!states[dwUserIndex].has_value()) {
        return ERROR_DEVICE_NOT_CONNECTED;
    }

    return ERROR_EMPTY;
}

DWORD WINAPI MyXInputGetBatteryInformation(DWORD dwUserIndex, BYTE devType, XINPUT_BATTERY_INFORMATION* pBatteryInformation) WIN_NOEXCEPT {
    *pBatteryInformation = {};
    pBatteryInformation->BatteryLevel = BATTERY_LEVEL_FULL;
    pBatteryInformation->BatteryType = BATTERY_TYPE_WIRED;
    return ERROR_SUCCESS;
}

DWORD WINAPI MyXInputGetAudioDeviceIds(DWORD dwUserIndex, LPWSTR pRenderDeviceId, UINT* pRenderCount, LPWSTR pCaptureDeviceId, UINT* pCaptureCount) WIN_NOEXCEPT {
    *pRenderCount = 0;
    *pCaptureCount = 0;
    *pRenderDeviceId = NULL;
    *pCaptureDeviceId = NULL;
    return ERROR_SUCCESS;
}

DWORD WINAPI MyXInputGetDSoundAudioDeviceGuids(DWORD dwUserIndex, GUID* pDSoundRenderGuid, GUID* pDSoundCaptureGuid) WIN_NOEXCEPT {
    *pDSoundRenderGuid = GUID_NULL;
    *pDSoundCaptureGuid = GUID_NULL;
    return ERROR_SUCCESS;
}

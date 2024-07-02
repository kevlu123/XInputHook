#pragma once
#include <Windows.h>
#include <Xinput.h>
#include <optional>
#include <stdint.h>

constexpr size_t MAX_XINPUT_CONTROLLERS = 4;
void SetControllerState(size_t index, std::optional<XINPUT_STATE> state);

void WINAPI MyXInputEnable(BOOL enable) WIN_NOEXCEPT;
DWORD WINAPI MyXInputGetState(DWORD dwUserIndex, XINPUT_STATE* pState) WIN_NOEXCEPT;
DWORD WINAPI MyXInputSetState(DWORD dwUserIndex, XINPUT_VIBRATION* pVibration) WIN_NOEXCEPT;
DWORD WINAPI MyXInputGetCapabilities(DWORD dwUserIndex, DWORD dwFlags, XINPUT_CAPABILITIES* pCapabilities) WIN_NOEXCEPT;
DWORD WINAPI MyXInputGetKeystroke(DWORD dwUserIndex, DWORD dwReserved, PXINPUT_KEYSTROKE pKeystroke) WIN_NOEXCEPT;
DWORD WINAPI MyXInputGetBatteryInformation(DWORD dwUserIndex, BYTE devType, XINPUT_BATTERY_INFORMATION* pBatteryInformation) WIN_NOEXCEPT;
DWORD WINAPI MyXInputGetAudioDeviceIds(DWORD  dwUserIndex, LPWSTR pRenderDeviceId, UINT* pRenderCount, LPWSTR pCaptureDeviceId, UINT* pCaptureCount) WIN_NOEXCEPT;
DWORD WINAPI MyXInputGetDSoundAudioDeviceGuids(DWORD dwUserIndex, GUID* pDSoundRenderGuid, GUID* pDSoundCaptureGuid) WIN_NOEXCEPT;
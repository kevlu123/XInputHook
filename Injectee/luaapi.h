#pragma once
#include <Windows.h>

enum DeviceNumber {
    DEVICE_KEYBOARD_0 = 100,
    DEVICE_KEYBOARD_1 = 101,
    DEVICE_KEYBOARD_2 = 102,
    DEVICE_KEYBOARD_3 = 103,
    DEVICE_MOUSE_0 = 200,
    DEVICE_MOUSE_1 = 201,
    DEVICE_MOUSE_2 = 202,
    DEVICE_MOUSE_3 = 203,
};

bool LuaInit(const char* script);
void LuaOnInput(HRAWINPUT hRawInput);
void LuaSetDeviceNumbers(const HANDLE* hDevices, const DeviceNumber* deviceNumbers, int len);
bool LuaIsMutedKey(int key);

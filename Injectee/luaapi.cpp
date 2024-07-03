#include "luaapi.h"

#include <utility>
#include <mutex>
#include <memory>
#include <unordered_map>
#include <unordered_set>

#include "Lua/lua.hpp"
#include "myxinput.h"
#include "dllmain.h"

static std::mutex luaMutex;
static lua_State* L = nullptr;
static std::unordered_map<HANDLE, int> hdevices;
static std::unordered_set<int> mutedKeys;

#define CONSTANT(name) { #name, name }

static const std::pair<const char*, int> CONSTANTS[] = {
    CONSTANT(DEVICE_KEYBOARD_0),
    CONSTANT(DEVICE_KEYBOARD_1),
    CONSTANT(DEVICE_KEYBOARD_2),
    CONSTANT(DEVICE_KEYBOARD_3),
    CONSTANT(DEVICE_MOUSE_0),
    CONSTANT(DEVICE_MOUSE_1),
    CONSTANT(DEVICE_MOUSE_2),
    CONSTANT(DEVICE_MOUSE_3),

    // RAWINPUT device types (https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-rawinputheader)
    CONSTANT(RIM_TYPEMOUSE),
    CONSTANT(RIM_TYPEKEYBOARD),
    CONSTANT(RIM_TYPEHID),

    // RAWINPUT keyboard flags (https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-rawkeyboard)
    CONSTANT(RI_KEY_MAKE),
    CONSTANT(RI_KEY_BREAK),
    CONSTANT(RI_KEY_E0),
    CONSTANT(RI_KEY_E1),

    // RAWINPUT mouse flags (https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-rawmouse)
    CONSTANT(MOUSE_MOVE_RELATIVE),
    CONSTANT(MOUSE_MOVE_ABSOLUTE),
    CONSTANT(MOUSE_VIRTUAL_DESKTOP),
    CONSTANT(MOUSE_ATTRIBUTES_CHANGED),
    CONSTANT(MOUSE_MOVE_NOCOALESCE),

    // RAWINPUT mouse button flags (https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-rawmouse)
    CONSTANT(RI_MOUSE_BUTTON_1_DOWN),
    CONSTANT(RI_MOUSE_LEFT_BUTTON_DOWN),
    CONSTANT(RI_MOUSE_BUTTON_1_UP),
    CONSTANT(RI_MOUSE_LEFT_BUTTON_UP),
    CONSTANT(RI_MOUSE_BUTTON_2_DOWN),
    CONSTANT(RI_MOUSE_RIGHT_BUTTON_DOWN),
    CONSTANT(RI_MOUSE_BUTTON_2_UP),
    CONSTANT(RI_MOUSE_RIGHT_BUTTON_UP),
    CONSTANT(RI_MOUSE_BUTTON_3_DOWN),
    CONSTANT(RI_MOUSE_MIDDLE_BUTTON_DOWN),
    CONSTANT(RI_MOUSE_BUTTON_3_UP),
    CONSTANT(RI_MOUSE_MIDDLE_BUTTON_UP),
    CONSTANT(RI_MOUSE_BUTTON_4_DOWN),
    CONSTANT(RI_MOUSE_BUTTON_4_UP),
    CONSTANT(RI_MOUSE_BUTTON_5_DOWN),
    CONSTANT(RI_MOUSE_BUTTON_5_UP),
    CONSTANT(RI_MOUSE_WHEEL),
    CONSTANT(RI_MOUSE_HWHEEL),

    // XINPUT_GAMEPAD buttons (https://learn.microsoft.com/en-us/windows/win32/api/xinput/ns-xinput-xinput_gamepad)
    CONSTANT(XINPUT_GAMEPAD_DPAD_UP),
    CONSTANT(XINPUT_GAMEPAD_DPAD_DOWN),
    CONSTANT(XINPUT_GAMEPAD_DPAD_LEFT),
    CONSTANT(XINPUT_GAMEPAD_DPAD_RIGHT),
    CONSTANT(XINPUT_GAMEPAD_START),
    CONSTANT(XINPUT_GAMEPAD_BACK),
    CONSTANT(XINPUT_GAMEPAD_LEFT_THUMB),
    CONSTANT(XINPUT_GAMEPAD_RIGHT_THUMB),
    CONSTANT(XINPUT_GAMEPAD_LEFT_SHOULDER),
    CONSTANT(XINPUT_GAMEPAD_RIGHT_SHOULDER),
    CONSTANT(XINPUT_GAMEPAD_A),
    CONSTANT(XINPUT_GAMEPAD_B),
    CONSTANT(XINPUT_GAMEPAD_X),
    CONSTANT(XINPUT_GAMEPAD_Y),

    // Virtual key codes (https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes)
    CONSTANT(VK_LBUTTON),
    CONSTANT(VK_RBUTTON),
    CONSTANT(VK_CANCEL),
    CONSTANT(VK_MBUTTON),
    CONSTANT(VK_XBUTTON1),
    CONSTANT(VK_XBUTTON2),
    CONSTANT(VK_BACK),
    CONSTANT(VK_TAB),
    CONSTANT(VK_CLEAR),
    CONSTANT(VK_RETURN),
    CONSTANT(VK_SHIFT),
    CONSTANT(VK_CONTROL),
    CONSTANT(VK_MENU),
    CONSTANT(VK_PAUSE),
    CONSTANT(VK_CAPITAL),
    CONSTANT(VK_KANA),
    CONSTANT(VK_HANGUL),
    CONSTANT(VK_IME_ON),
    CONSTANT(VK_JUNJA),
    CONSTANT(VK_FINAL),
    CONSTANT(VK_HANJA),
    CONSTANT(VK_KANJI),
    CONSTANT(VK_IME_OFF),
    CONSTANT(VK_ESCAPE),
    CONSTANT(VK_CONVERT),
    CONSTANT(VK_NONCONVERT),
    CONSTANT(VK_ACCEPT),
    CONSTANT(VK_MODECHANGE),
    CONSTANT(VK_SPACE),
    CONSTANT(VK_PRIOR),
    CONSTANT(VK_NEXT),
    CONSTANT(VK_END),
    CONSTANT(VK_HOME),
    CONSTANT(VK_LEFT),
    CONSTANT(VK_UP),
    CONSTANT(VK_RIGHT),
    CONSTANT(VK_DOWN),
    CONSTANT(VK_SELECT),
    CONSTANT(VK_PRINT),
    CONSTANT(VK_EXECUTE),
    CONSTANT(VK_SNAPSHOT),
    CONSTANT(VK_INSERT),
    CONSTANT(VK_DELETE),
    CONSTANT(VK_HELP),
    CONSTANT(VK_LWIN),
    CONSTANT(VK_RWIN),
    CONSTANT(VK_APPS),
    CONSTANT(VK_SLEEP),
    CONSTANT(VK_NUMPAD0),
    CONSTANT(VK_NUMPAD1),
    CONSTANT(VK_NUMPAD2),
    CONSTANT(VK_NUMPAD3),
    CONSTANT(VK_NUMPAD4),
    CONSTANT(VK_NUMPAD5),
    CONSTANT(VK_NUMPAD6),
    CONSTANT(VK_NUMPAD7),
    CONSTANT(VK_NUMPAD8),
    CONSTANT(VK_NUMPAD9),
    CONSTANT(VK_MULTIPLY),
    CONSTANT(VK_ADD),
    CONSTANT(VK_SEPARATOR),
    CONSTANT(VK_SUBTRACT),
    CONSTANT(VK_DECIMAL),
    CONSTANT(VK_DIVIDE),
    CONSTANT(VK_F1),
    CONSTANT(VK_F2),
    CONSTANT(VK_F3),
    CONSTANT(VK_F4),
    CONSTANT(VK_F5),
    CONSTANT(VK_F6),
    CONSTANT(VK_F7),
    CONSTANT(VK_F8),
    CONSTANT(VK_F9),
    CONSTANT(VK_F10),
    CONSTANT(VK_F11),
    CONSTANT(VK_F12),
    CONSTANT(VK_F13),
    CONSTANT(VK_F14),
    CONSTANT(VK_F15),
    CONSTANT(VK_F16),
    CONSTANT(VK_F17),
    CONSTANT(VK_F18),
    CONSTANT(VK_F19),
    CONSTANT(VK_F20),
    CONSTANT(VK_F21),
    CONSTANT(VK_F22),
    CONSTANT(VK_F23),
    CONSTANT(VK_F24),
    CONSTANT(VK_NUMLOCK),
    CONSTANT(VK_SCROLL),
    CONSTANT(VK_LSHIFT),
    CONSTANT(VK_RSHIFT),
    CONSTANT(VK_LCONTROL),
    CONSTANT(VK_RCONTROL),
    CONSTANT(VK_LMENU),
    CONSTANT(VK_RMENU),
    CONSTANT(VK_BROWSER_BACK),
    CONSTANT(VK_BROWSER_FORWARD),
    CONSTANT(VK_BROWSER_REFRESH),
    CONSTANT(VK_BROWSER_STOP),
    CONSTANT(VK_BROWSER_SEARCH),
    CONSTANT(VK_BROWSER_FAVORITES),
    CONSTANT(VK_BROWSER_HOME),
    CONSTANT(VK_VOLUME_MUTE),
    CONSTANT(VK_VOLUME_DOWN),
    CONSTANT(VK_VOLUME_UP),
    CONSTANT(VK_MEDIA_NEXT_TRACK),
    CONSTANT(VK_MEDIA_PREV_TRACK),
    CONSTANT(VK_MEDIA_STOP),
    CONSTANT(VK_MEDIA_PLAY_PAUSE),
    CONSTANT(VK_LAUNCH_MAIL),
    CONSTANT(VK_LAUNCH_MEDIA_SELECT),
    CONSTANT(VK_LAUNCH_APP1),
    CONSTANT(VK_LAUNCH_APP2),
    CONSTANT(VK_OEM_1),
    CONSTANT(VK_OEM_PLUS),
    CONSTANT(VK_OEM_COMMA),
    CONSTANT(VK_OEM_MINUS),
    CONSTANT(VK_OEM_PERIOD),
    CONSTANT(VK_OEM_2),
    CONSTANT(VK_OEM_3),
    CONSTANT(VK_OEM_4),
    CONSTANT(VK_OEM_5),
    CONSTANT(VK_OEM_6),
    CONSTANT(VK_OEM_7),
    CONSTANT(VK_OEM_8),
    CONSTANT(VK_OEM_102),
    CONSTANT(VK_PROCESSKEY),
    CONSTANT(VK_PACKET),
    CONSTANT(VK_ATTN),
    CONSTANT(VK_CRSEL),
    CONSTANT(VK_EXSEL),
    CONSTANT(VK_EREOF),
    CONSTANT(VK_PLAY),
    CONSTANT(VK_ZOOM),
    CONSTANT(VK_NONAME),
    CONSTANT(VK_PA1),
    CONSTANT(VK_OEM_CLEAR),
};

static void LuaError() {
    PrintMessage("LUA ERROR:\n");
    PrintMessage(lua_tostring(L, -1));
    PrintMessage("\n");
    lua_close(L);
    L = nullptr;
}

static void PushRawInput(const RAWINPUT* input) {
    lua_newtable(L);

    lua_pushstring(L, "header");
    lua_newtable(L);
    {
        lua_pushstring(L, "dwType");
        lua_pushinteger(L, input->header.dwType);
        lua_settable(L, -3);

        lua_pushstring(L, "dwSize");
        lua_pushinteger(L, input->header.dwSize);
        lua_settable(L, -3);

        lua_pushstring(L, "hDevice");
        lua_pushinteger(L, (lua_Integer)input->header.hDevice);
        lua_settable(L, -3);

        lua_pushstring(L, "wParam");
        lua_pushinteger(L, input->header.wParam);
        lua_settable(L, -3);
    }
    lua_settable(L, -3);

    lua_pushstring(L, "mouse");
    lua_newtable(L);
    {
        lua_pushstring(L, "usFlags");
        lua_pushinteger(L, input->data.mouse.usFlags);
        lua_settable(L, -3);

        lua_pushstring(L, "ulButtons");
        lua_pushinteger(L, input->data.mouse.ulButtons);
        lua_settable(L, -3);

        lua_pushstring(L, "usButtonFlags");
        lua_pushinteger(L, input->data.mouse.usButtonFlags);
        lua_settable(L, -3);

        lua_pushstring(L, "usButtonData");
        lua_pushinteger(L, input->data.mouse.usButtonData);
        lua_settable(L, -3);

        lua_pushstring(L, "ulRawButtons");
        lua_pushinteger(L, input->data.mouse.ulRawButtons);
        lua_settable(L, -3);

        lua_pushstring(L, "lLastX");
        lua_pushinteger(L, input->data.mouse.lLastX);
        lua_settable(L, -3);

        lua_pushstring(L, "lLastY");
        lua_pushinteger(L, input->data.mouse.lLastY);
        lua_settable(L, -3);

        lua_pushstring(L, "ulExtraInformation");
        lua_pushinteger(L, input->data.mouse.ulExtraInformation);
        lua_settable(L, -3);
    }
    lua_settable(L, -3);

    lua_pushstring(L, "keyboard");
    lua_newtable(L);
    {
        lua_pushstring(L, "MakeCode");
        lua_pushinteger(L, input->data.keyboard.MakeCode);
        lua_settable(L, -3);

        lua_pushstring(L, "Flags");
        lua_pushinteger(L, input->data.keyboard.Flags);
        lua_settable(L, -3);

        lua_pushstring(L, "Reserved");
        lua_pushinteger(L, input->data.keyboard.Reserved);
        lua_settable(L, -3);

        lua_pushstring(L, "VKey");
        lua_pushinteger(L, input->data.keyboard.VKey);
        lua_settable(L, -3);

        lua_pushstring(L, "Message");
        lua_pushinteger(L, input->data.keyboard.Message);
        lua_settable(L, -3);

        lua_pushstring(L, "ExtraInformation");
        lua_pushinteger(L, input->data.keyboard.ExtraInformation);
        lua_settable(L, -3);
    }
    lua_settable(L, -3);
}

static int SetState(lua_State* L) {
    if (!lua_isinteger(L, 1)) {
        lua_pushstring(L, "Invalid argument #1: expected integer");
        lua_error(L);
    }

    lua_Integer index = lua_tointeger(L, 1);
    if (index < 0 || index >= MAX_XINPUT_CONTROLLERS) {
        lua_pushstring(L, "Invalid argument #1: out of range");
        lua_error(L);
    }

    if (lua_isnil(L, 2)) {
        SetControllerState((size_t)index, std::nullopt);
        return 0;
    }

    if (!lua_istable(L, 2)) {
        lua_pushstring(L, "Invalid argument #2: expected table or nil");
        lua_error(L);
    }

    XINPUT_STATE state{};

    lua_getfield(L, 2, "dwPacketNumber");
    if (!lua_isinteger(L, -1)) {
        lua_pushstring(L, "Invalid field 'dwPacketNumber': expected integer");
        lua_error(L);
    }
    state.dwPacketNumber = (DWORD)lua_tointeger(L, -1);

    lua_getfield(L, 2, "Gamepad");
    if (!lua_istable(L, -1)) {
        lua_pushstring(L, "Invalid field 'Gamepad': expected table");
        lua_error(L);
    }

    lua_getfield(L, -1, "wButtons");
    if (lua_isinteger(L, -1)) {
        state.Gamepad.wButtons = (WORD)lua_tointeger(L, -1);
    }
    lua_pop(L, 1);

    lua_getfield(L, -1, "bLeftTrigger");
    if (lua_isnumber(L, -1)) {
        state.Gamepad.bLeftTrigger = (BYTE)lua_tonumber(L, -1);
    }
    lua_pop(L, 1);

    lua_getfield(L, -1, "bRightTrigger");
    if (lua_isnumber(L, -1)) {
        state.Gamepad.bRightTrigger = (BYTE)lua_tonumber(L, -1);
    }
    lua_pop(L, 1);

    lua_getfield(L, -1, "sThumbLX");
    if (lua_isnumber(L, -1)) {
        state.Gamepad.sThumbLX = (SHORT)lua_tonumber(L, -1);
    }
    lua_pop(L, 1);

    lua_getfield(L, -1, "sThumbLY");
    if (lua_isnumber(L, -1)) {
        state.Gamepad.sThumbLY = (SHORT)lua_tonumber(L, -1);
    }
    lua_pop(L, 1);

    lua_getfield(L, -1, "sThumbRX");
    if (lua_isnumber(L, -1)) {
        state.Gamepad.sThumbRX = (SHORT)lua_tonumber(L, -1);
    }
    lua_pop(L, 1);

    lua_getfield(L, -1, "sThumbRY");
    if (lua_isnumber(L, -1)) {
        state.Gamepad.sThumbRY = (SHORT)lua_tonumber(L, -1);
    }
    lua_pop(L, 1);

    SetControllerState((size_t)index, state);
    return 0;
}

static int Print(lua_State* L) {
    int nargs = lua_gettop(L);
    for (int i = 1; i <= nargs; i++) {
        const char* s = lua_tostring(L, i);
        if (s) {
            PrintMessage(s);
        }
        if (i < nargs) {
            PrintMessage("\t");
        }
    }
    PrintMessage("\n");
    return 0;
}

static int GetDeviceNumber(lua_State* L) {
    if (!lua_isinteger(L, 1)) {
        lua_pushstring(L, "Invalid argument #1: expected integer");
        lua_error(L);
    }

    HANDLE hDevice = (HANDLE)lua_tointeger(L, 1);
    auto it = hdevices.find(hDevice);
    int deviceNumber = it != hdevices.end() ? it->second : 0;
    lua_pushinteger(L, deviceNumber);
    return 1;
}

static int MuteKey(lua_State* L) {
    if (!lua_isinteger(L, 1)) {
        lua_pushstring(L, "Invalid argument #1: expected integer");
        lua_error(L);
    }

    int key = (int)lua_tointeger(L, 1);
    mutedKeys.insert(key);
    return 0;
}

bool LuaInit(const char* script) {
    std::lock_guard<std::mutex> lock(luaMutex);
    SetControllerState(0, std::nullopt);
    SetControllerState(1, std::nullopt);
    SetControllerState(2, std::nullopt);
    SetControllerState(3, std::nullopt);
    mutedKeys.clear();
    if (L) {
        lua_close(L);
    }

    L = luaL_newstate();
    luaL_openlibs(L);

    luaL_Reg functions[] = {
        { "print", Print },
        { "setstate", SetState },
        { "devicenumber", GetDeviceNumber },
        { "mutekey", MuteKey },
        { NULL, NULL }
    };
    lua_getglobal(L, "_G");
    luaL_setfuncs(L, functions, 0);
    lua_pop(L, 1);

    for (const auto& [name, value] : CONSTANTS) {
        lua_pushinteger(L, value);
        lua_setglobal(L, name);
    }

    if (luaL_dostring(L, script) != LUA_OK) {
        LuaError();
    }
    return L != nullptr;
}

static std::shared_ptr<RAWINPUT> GetRawInput(HRAWINPUT hRawInput) {
    UINT size = 0;
    GetRawInputData(hRawInput, RID_INPUT, NULL, &size, sizeof(RAWINPUTHEADER));
    char* buffer = new char[size];
    GetRawInputData(hRawInput, RID_INPUT, buffer, &size, sizeof(RAWINPUTHEADER));
    return std::shared_ptr<RAWINPUT>((RAWINPUT*)buffer, [](RAWINPUT* p) { delete[](char*)p; });
}

void LuaOnInput(HRAWINPUT hRawInput) {
    std::lock_guard<std::mutex> lock(luaMutex);
    if (L) {
        lua_getglobal(L, "oninput");
        if (lua_isfunction(L, -1)) {
            auto rawInput = GetRawInput(hRawInput);
            PushRawInput(rawInput.get());
            if (lua_pcall(L, 1, 0, 0) != LUA_OK) {
                LuaError();
            }
        } else {
            lua_pushstring(L, "oninput is not a function");
            LuaError();
        }
    }
}

void LuaSetDeviceNumbers(const HANDLE* hDevices, const DeviceNumber* deviceNumbers, int len) {
    std::lock_guard<std::mutex> lock(luaMutex);
    hdevices.clear();
    for (int i = 0; i < len; i++) {
        hdevices[hDevices[i]] = deviceNumbers[i];
    }
}

bool LuaIsMutedKey(int key) {
    std::lock_guard<std::mutex> lock(luaMutex);
    return mutedKeys.find(key) != mutedKeys.end();
}

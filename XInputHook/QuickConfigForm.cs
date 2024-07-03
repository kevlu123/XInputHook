using System.Text;
using System.Text.Json;
using FormTimer = System.Windows.Forms.Timer;

namespace XInputHook;

public partial class QuickConfigForm : Form {
    private readonly FormTimer timer = new();
    private readonly Profile profile;
    private Button? controllerButton;

    class ButtonConfig {
        public Button? button;
        public required int Key { get; set; }
        public required string ButtonName { get; set; }
    }

    class ControllerConfig {
        public List<ButtonConfig> ButtonConfigs { get; set; } = [];
        public int TriggerMagnitude { get; set; } = 255;
        public int JoystickMagnitude { get; set; } = 32767;
        public bool NormaliseJoystick { get; set; }
        public KeyboardListenMode KeyboardListenMode { get; set; } = KeyboardListenMode.Any;
        public required bool Enabled { get; set; }
    }

    class Config {
        public List<ControllerConfig> ControllerConfigs { get; set; } = [];
        public uint Checksum { get; set; }
    }

    private readonly List<ControllerConfig> controllerConfigs;
    private ControllerConfig currentControllerConfig;
    private readonly bool warningOnApply;
    private bool suppressModificationEvent;

    public QuickConfigForm(Profile profile) {
        this.profile = profile;
        InitializeComponent();
        applyButton.Enabled = false;

        async void Unfocus() {
            await Task.Yield();
            ActiveControl = null;
        }
        Unfocus();

        try {
            warningOnApply = true;
            var code = File.ReadAllLines(profile.GetScriptPath());
            if (code.Length == 0) {
                warningOnApply = false;
            }
            var json = code[^1][2..];
            var config = JsonSerializer.Deserialize<Config>(json)
                ?? throw new Exception();
            if (config.ControllerConfigs.Count != 4) {
                throw new Exception();
            }
            controllerConfigs = config.ControllerConfigs;
            warningOnApply = config.Checksum != StringChecksum(string.Join("\n", code[..^1]));
        } catch {
            controllerConfigs = [
            new ControllerConfig { Enabled = true },
                new ControllerConfig { Enabled = false },
                new ControllerConfig { Enabled = false },
                new ControllerConfig { Enabled = false },
            ];
        }
        foreach (var controllerConfig in controllerConfigs) {
            SetButtonField(controllerConfig);
        }
        currentControllerConfig = controllerConfigs[0];

        LoadConfigUI();
        Text += $" - {profile.Name}";
    }

    private void LoadConfigUI() {
        suppressModificationEvent = true;

        foreach (var buttonConfig in currentControllerConfig.ButtonConfigs) {
            buttonConfig.button!.Text = buttonConfig.Key == 0 ? "" : ((Keys)buttonConfig.Key).ToString();
        }

        triggerMagnitudeTextBox.Text = currentControllerConfig.TriggerMagnitude.ToString();
        joystickMagnitudeTextBox.Text = currentControllerConfig.JoystickMagnitude.ToString();
        normaliseJoystickCheckBox.Checked = currentControllerConfig.NormaliseJoystick;
        if (Enum.IsDefined(currentControllerConfig.KeyboardListenMode)) {
            keyboardNumberComboBox.SelectedIndex = (int)currentControllerConfig.KeyboardListenMode;
        } else {
            keyboardNumberComboBox.SelectedIndex = 0;
        }

        suppressModificationEvent = false;
    }

    private void ControllerButtonClicked(object sender, MouseEventArgs e) {
        StopButtonListen(Keys.None);

        controllerButton = (Button)sender;
        controllerButton.Text = "...";

        timer.Interval = 2000;
        timer.Tick += (s, e) => StopButtonListen(Keys.None);
        timer.Start();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys key) {
        if (controllerButton != null) {
            StopButtonListen(key);
            return true;
        } else {
            return base.ProcessCmdKey(ref msg, key);
        }
    }

    private static string KeyToString(Keys key) {
        var s = key == Keys.None ? "" : key.ToString();
        return s.Split(',').Last();
    }

    private void StopButtonListen(Keys key) {
        ActiveControl = null;
        if (controllerButton == null) {
            return;
        }
        timer.Stop();
        GetButtonConfigByButton(controllerButton).Key = (int)key;
        controllerButton.Text = key == Keys.None ? "" : KeyToString(key);
        controllerButton = null;
        OnModification();
    }

    private ButtonConfig GetButtonConfigByButton(Button button, ControllerConfig? controller = null) {
        return (controller ?? currentControllerConfig).ButtonConfigs.Find(b => b.button == button)!;
    }

    private bool Apply() {
        if (warningOnApply) {
            var result = MessageBox.Show(
                "The script has been modified externally. Overwrite the script?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No) {
                return false;
            }
        }

        var script = GenerateScript();
        try {
            profile.SetScript(script);
            return true;
        } catch (Exception ex) {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private void ApplyButtonClicked(object sender, EventArgs e) {
        if (Apply()) {
            applyButton.Enabled = false;
        }
    }

    private void OkButtonClicked(object sender, EventArgs e) {
        if (Apply()) {
            Close();
        }
    }

    private void CancelButtonClicked(object sender, EventArgs e) {
        Close();
    }

    private void ClearButtonClicked(object sender, EventArgs e) {
        var index = controllerConfigs.IndexOf(currentControllerConfig);
        currentControllerConfig = new ControllerConfig { Enabled = currentControllerConfig.Enabled };
        SetButtonField(currentControllerConfig);
        controllerConfigs[index] = currentControllerConfig;
        LoadConfigUI();
    }

    private ControllerConfig SetButtonField(ControllerConfig controllerConfig) {
        var namesToButtons = new Dictionary<string, Button>() {
            { "ljoyup", ljoyup },
            { "ljoydown", ljoydown },
            { "ljoyleft", ljoyleft },
            { "ljoyright", ljoyright },
            { "ljoybutton", ljoybutton },
            { "rjoyup", rjoyup },
            { "rjoydown", rjoydown },
            { "rjoyleft", rjoyleft },
            { "rjoyright", rjoyright },
            { "rjoybutton", rjoybutton },
            { "dpadup", dpadup },
            { "dpaddown", dpaddown },
            { "dpadleft", dpadleft },
            { "dpadright", dpadright },
            { "buttona", buttona },
            { "buttonb", buttonb },
            { "buttonx", buttonx },
            { "buttony", buttony },
            { "buttonback", buttonback },
            { "buttonstart", buttonstart },
            { "bumperleft", bumperleft },
            { "bumperright", bumperright },
            { "triggerleft", triggerleft },
            { "triggerright", triggerright },
        };

        foreach (var (name, button) in namesToButtons) {
            var config = controllerConfig.ButtonConfigs.Find(c => c.ButtonName == name);
            if (config == null) {
                controllerConfig.ButtonConfigs.Add(new ButtonConfig {
                    button = button,
                    Key = 0,
                    ButtonName = name,
                });
            } else {
                config.button = button;
            }
        }

        return controllerConfig;
    }

    private void ControllerRadioChanged(object sender, EventArgs e) {
        var radio = (RadioButton)sender;
        if (radio.Checked) {
            if (radio == controllerRadio1) {
                currentControllerConfig = controllerConfigs[0];
            } else if (radio == controllerRadio2) {
                currentControllerConfig = controllerConfigs[1];
            } else if (radio == controllerRadio3) {
                currentControllerConfig = controllerConfigs[2];
            } else if (radio == controllerRadio4) {
                currentControllerConfig = controllerConfigs[3];
            }
            StopButtonListen(Keys.None);
            LoadConfigUI();
        }
    }

    private void EnabledCheckChanged(object sender, EventArgs e) {
        var index = 0;
        if (sender == enabledCheckBox1) {
            index = 0;
        } else if (sender == enabledCheckBox2) {
            index = 1;
        } else if (sender == enabledCheckBox3) {
            index = 2;
        } else if (sender == enabledCheckBox4) {
            index = 3;
        }

        controllerConfigs[index].Enabled = ((CheckBox)sender).Checked;
        OnModification();
    }

    private void OnModification() {
        if (!suppressModificationEvent) {
            applyButton.Enabled = true;
        }
    }

    private void TriggerMagnitudeTextChanged(object sender, EventArgs e) {
        if (int.TryParse(triggerMagnitudeTextBox.Text, out var value)) {
            currentControllerConfig.TriggerMagnitude = value;
            OnModification();
        }
    }

    private void JoystickMagnitudeTextChanged(object sender, EventArgs e) {
        if (int.TryParse(joystickMagnitudeTextBox.Text, out var value)) {
            currentControllerConfig.JoystickMagnitude = value;
            OnModification();
        }
    }

    private void NormaliseJoystickCheckChanged(object sender, EventArgs e) {
        currentControllerConfig.NormaliseJoystick = normaliseJoystickCheckBox.Checked;
        OnModification();
    }

    private void KeyboardNumberSelectedIndexChanged(object sender, EventArgs e) {
        currentControllerConfig.KeyboardListenMode = (KeyboardListenMode)keyboardNumberComboBox.SelectedIndex;
        OnModification();
    }

    private void OnFormClosed(object sender, FormClosedEventArgs e) {
        timer.Dispose();
    }

    private static uint StringChecksum(string s) {
        s = s.Replace("\r", "");
        int checksum = 0;
        var data = Encoding.UTF8.GetBytes(s);
        for (int i = 0; i < data.Length; i++) {
            checksum += data[i] * i;
        }
        return (uint)checksum;
    }

    private string GenerateScript() {
        var format = @"-- Script generated by XInputHook

-------- Configuration --------

controller1 = {{
    enabled = {},
    triggermagnitude = {},
    joystickmagnitude = {},
    normalisejoystick = {},
    keyboard = function(hdevice)
        return {}
    end,
    keys = {{
        dpadup          = {}, -- {}
        dpaddown        = {}, -- {}
        dpadleft        = {}, -- {}
        dpadright       = {}, -- {}
        buttona         = {}, -- {}
        buttonb         = {}, -- {}
        buttonx         = {}, -- {}
        buttony         = {}, -- {}
        buttonback      = {}, -- {}
        buttonstart     = {}, -- {}
        bumperleft      = {}, -- {}
        bumperright     = {}, -- {}
        triggerleft     = {}, -- {}
        triggerright    = {}, -- {}
        ljoybutton      = {}, -- {}
        rjoybutton      = {}, -- {}
        ljoyup          = {}, -- {}
        ljoydown        = {}, -- {}
        ljoyleft        = {}, -- {}
        ljoyright       = {}, -- {}
        rjoyup          = {}, -- {}
        rjoydown        = {}, -- {}
        rjoyleft        = {}, -- {}
        rjoyright       = {}, -- {}
    }},
}}

controller2 = {{
    enabled = {},
    triggermagnitude = {},
    joystickmagnitude = {},
    normalisejoystick = {},
    keyboard = function(hdevice)
        return {}
    end,
    keys = {{
        dpadup          = {}, -- {}
        dpaddown        = {}, -- {}
        dpadleft        = {}, -- {}
        dpadright       = {}, -- {}
        buttona         = {}, -- {}
        buttonb         = {}, -- {}
        buttonx         = {}, -- {}
        buttony         = {}, -- {}
        buttonback      = {}, -- {}
        buttonstart     = {}, -- {}
        bumperleft      = {}, -- {}
        bumperright     = {}, -- {}
        triggerleft     = {}, -- {}
        triggerright    = {}, -- {}
        ljoybutton      = {}, -- {}
        rjoybutton      = {}, -- {}
        ljoyup          = {}, -- {}
        ljoydown        = {}, -- {}
        ljoyleft        = {}, -- {}
        ljoyright       = {}, -- {}
        rjoyup          = {}, -- {}
        rjoydown        = {}, -- {}
        rjoyleft        = {}, -- {}
        rjoyright       = {}, -- {}
    }},
}}

controller3 = {{
    enabled = {},
    triggermagnitude = {},
    joystickmagnitude = {},
    normalisejoystick = {},
    keyboard = function(hdevice)
        return {}
    end,
    keys = {{
        dpadup          = {}, -- {}
        dpaddown        = {}, -- {}
        dpadleft        = {}, -- {}
        dpadright       = {}, -- {}
        buttona         = {}, -- {}
        buttonb         = {}, -- {}
        buttonx         = {}, -- {}
        buttony         = {}, -- {}
        buttonback      = {}, -- {}
        buttonstart     = {}, -- {}
        bumperleft      = {}, -- {}
        bumperright     = {}, -- {}
        triggerleft     = {}, -- {}
        triggerright    = {}, -- {}
        ljoybutton      = {}, -- {}
        rjoybutton      = {}, -- {}
        ljoyup          = {}, -- {}
        ljoydown        = {}, -- {}
        ljoyleft        = {}, -- {}
        ljoyright       = {}, -- {}
        rjoyup          = {}, -- {}
        rjoydown        = {}, -- {}
        rjoyleft        = {}, -- {}
        rjoyright       = {}, -- {}
    }},
}}

controller4 = {{
    enabled = {},
    triggermagnitude = {},
    joystickmagnitude = {},
    normalisejoystick = {},
    keyboard = function(hdevice)
        return {}
    end,
    keys = {{
        dpadup          = {}, -- {}
        dpaddown        = {}, -- {}
        dpadleft        = {}, -- {}
        dpadright       = {}, -- {}
        buttona         = {}, -- {}
        buttonb         = {}, -- {}
        buttonx         = {}, -- {}
        buttony         = {}, -- {}
        buttonback      = {}, -- {}
        buttonstart     = {}, -- {}
        bumperleft      = {}, -- {}
        bumperright     = {}, -- {}
        triggerleft     = {}, -- {}
        triggerright    = {}, -- {}
        ljoybutton      = {}, -- {}
        rjoybutton      = {}, -- {}
        ljoyup          = {}, -- {}
        ljoydown        = {}, -- {}
        ljoyleft        = {}, -- {}
        ljoyright       = {}, -- {}
        rjoyup          = {}, -- {}
        rjoydown        = {}, -- {}
        rjoyleft        = {}, -- {}
        rjoyright       = {}, -- {}
    }},
}}

-------- End Configuration --------

function newstate(c)
    return {{
        dpadup = false,
        dpaddown = false,
        dpadleft = false,
        dpadright = false,
        buttona = false,
        buttonb = false,
        buttonx = false,
        buttony = false,
        buttonback = false,
        buttonstart = false,
        bumperleft = false,
        bumperright = false,
        triggerleft = false,
        triggerright = false,
        ljoybutton = false,
        rjoybutton = false,
        ljoyup = false,
        ljoydown = false,
        ljoyleft = false,
        ljoyright = false,
        rjoyup = false,
        rjoydown = false,
        rjoyleft = false,
        rjoyright = false,

        dwPacketNumber = 0,
        Gamepad = {{
            wButtons = 0,
            bLeftTrigger = 0,
            bRightTrigger = 0,
            sThumbLX = 0,
            sThumbLY = 0,
            sThumbRX = 0,
            sThumbRY = 0,
        }},
    }}
end

function normalise(x, y)
	if x == 0 and y == 0 then
		return 0, 0
    end
    local div = math.sqrt(x * x + y * y)
    x = x / div
    y = y / div
    return x, y
end

function oninput(rawinput)
    if rawinput.header.dwType ~= RIM_TYPEKEYBOARD then
        return
    end

    local key = rawinput.keyboard.VKey
    if key == 0 then
        return
    end

    local flags = rawinput.keyboard.Flags

    for i, c in ipairs(controllers) do
        if not c.keyboard(rawinput.header.hDevice) then
            goto continue
        end

	    packet = packet + 1

        if not c.enabled then
            setstate(i - 1, nil)
            return
        end
		
        local state = c.state
        state.dwPacketNumber = packet

		local keys = c.keys
        local keydown = (flags & RI_KEY_BREAK) == 0
        if key == keys.dpadup then
            state.dpadup = keydown
        elseif key == keys.dpaddown then
            state.dpaddown = keydown
        elseif key == keys.dpadleft then
            state.dpadleft = keydown
        elseif key == keys.dpadright then
            state.dpadright = keydown
        elseif key == keys.buttona then
            state.buttona = keydown
        elseif key == keys.buttonb then
            state.buttonb = keydown
        elseif key == keys.buttonx then
            state.buttonx = keydown
        elseif key == keys.buttony then
            state.buttony = keydown
        elseif key == keys.buttonback then
            state.buttonback = keydown
        elseif key == keys.buttonstart then
            state.buttonstart = keydown
        elseif key == keys.bumperleft then
            state.bumperleft = keydown
        elseif key == keys.bumperright then
            state.bumperright = keydown
        elseif key == keys.triggerleft then
            state.triggerleft = keydown
        elseif key == keys.triggerright then
            state.triggerright = keydown
        elseif key == keys.ljoybutton then
            state.ljoybutton = keydown
        elseif key == keys.rjoybutton then
            state.rjoybutton = keydown
        elseif key == keys.ljoyup then
            state.ljoyup = keydown
        elseif key == keys.ljoydown then
            state.ljoydown = keydown
        elseif key == keys.ljoyleft then
            state.ljoyleft = keydown
        elseif key == keys.ljoyright then
            state.ljoyright = keydown
        elseif key == keys.rjoyup then
            state.rjoyup = keydown
        elseif key == keys.rjoydown then
            state.rjoydown = keydown
        elseif key == keys.rjoyleft then
            state.rjoyleft = keydown
        elseif key == keys.rjoyright then
            state.rjoyright = keydown
        end

        local buttons = 0
        if state.dpadup then
            buttons = buttons | XINPUT_GAMEPAD_DPAD_UP
        end
        if state.dpaddown then
            buttons = buttons | XINPUT_GAMEPAD_DPAD_DOWN
        end
        if state.dpadleft then
            buttons = buttons | XINPUT_GAMEPAD_DPAD_LEFT
        end
        if state.dpadright then
            buttons = buttons | XINPUT_GAMEPAD_DPAD_RIGHT
        end
        if state.buttona then
            buttons = buttons | XINPUT_GAMEPAD_A
        end
        if state.buttonb then
            buttons = buttons | XINPUT_GAMEPAD_B
        end
        if state.buttonx then
            buttons = buttons | XINPUT_GAMEPAD_X
        end
        if state.buttony then
            buttons = buttons | XINPUT_GAMEPAD_Y
        end
        if state.buttonback then
            buttons = buttons | XINPUT_GAMEPAD_BACK
        end
        if state.buttonstart then
            buttons = buttons | XINPUT_GAMEPAD_START
        end
        if state.bumperleft then
            buttons = buttons | XINPUT_GAMEPAD_LEFT_SHOULDER
        end
        if state.bumperright then
            buttons = buttons | XINPUT_GAMEPAD_RIGHT_SHOULDER
        end
        if state.ljoybutton then
            buttons = buttons | XINPUT_GAMEPAD_LEFT_THUMB
        end
        if state.rjoybutton then
            buttons = buttons | XINPUT_GAMEPAD_RIGHT_THUMB
        end
        state.Gamepad.wButtons = buttons

        local ljoyx = 0
        local ljoyy = 0
        if state.ljoydown then
            ljoyy = ljoyy - 1
        end
        if state.ljoyup then
            ljoyy = ljoyy + 1
        end
        if state.ljoyleft then
            ljoyx = ljoyx - 1
        end
        if state.ljoyright then
            ljoyx = ljoyx + 1
        end
        if c.normalisejoystick then
            ljoyx, ljoyy = normalise(ljoyx, ljoyy)
        end
		ljoyx = ljoyx * c.joystickmagnitude
		ljoyy = ljoyy * c.joystickmagnitude
        state.Gamepad.sThumbLX, state.Gamepad.sThumbLY = ljoyx, ljoyy

        local rjoyx = 0
        local rjoyy = 0
        if state.rjoydown then
            rjoyy = rjoyy - 1
        end
        if state.rjoyup then
            rjoyy = rjoyy + 1
        end
        if state.rjoyleft then
            rjoyx = rjoyx - 1
        end
        if state.rjoyright then
            rjoyx = rjoyx + 1
        end
        if c.normalisejoystick then
            rjoyx, rjoyy = normalise(rjoyx, rjoyy)
        end
		rjoyx = rjoyx * c.joystickmagnitude
		rjoyy = rjoyy * c.joystickmagnitude
        state.Gamepad.sThumbRX, state.Gamepad.sThumbRY = rjoyx, rjoyy

        if state.triggerleft then
            state.Gamepad.bLeftTrigger = c.triggermagnitude
        else
            state.Gamepad.bLeftTrigger = 0
        end
        if state.triggerright then
            state.Gamepad.bRightTrigger = c.triggermagnitude
        else
            state.Gamepad.bRightTrigger = 0
        end

	    setstate(i - 1, state)
        ::continue::
    end
end

packet = 0

controllers = {{
    controller1,
    controller2,
    controller3,
    controller4,
}}

for _, c in ipairs(controllers) do
    c.state = newstate()
    if c.enabled then
        for _, value in pairs(c.keys) do
            mutekey(value)
        end
    end
end

-- Do not edit the following line. This is used to load the configuration again.";

        int n = 0;
        int i;
        while ((i = format.IndexOf("{}")) != -1) {
            format = format.Remove(i, 2).Insert(i, "{" + n.ToString() + "}");
            n++;
        }

        var c1 = controllerConfigs[0];
        var c2 = controllerConfigs[1];
        var c3 = controllerConfigs[2];
        var c4 = controllerConfigs[3];

        static string BoolToString(bool b) => b ? "true" : "false";
        static string KeyboardListenModeToString(KeyboardListenMode mode) {
            return mode switch {
                KeyboardListenMode.Keyboard0 => "devicenumber(hdevice) == DEVICE_KEYBOARD_0",
                KeyboardListenMode.Keyboard1 => "devicenumber(hdevice) == DEVICE_KEYBOARD_1",
                KeyboardListenMode.Keyboard2 => "devicenumber(hdevice) == DEVICE_KEYBOARD_2",
                KeyboardListenMode.Keyboard3 => "devicenumber(hdevice) == DEVICE_KEYBOARD_3",
                _ => "true"
            };
        }

        var code = string.Format(format,
            BoolToString(c1.Enabled),
            c1.TriggerMagnitude,
            c1.JoystickMagnitude,
            BoolToString(c1.NormaliseJoystick),
            KeyboardListenModeToString(c1.KeyboardListenMode),
            GetButtonConfigByButton(dpadup, c1).Key, (Keys)GetButtonConfigByButton(dpadup, c1).Key,
            GetButtonConfigByButton(dpaddown, c1).Key, (Keys)GetButtonConfigByButton(dpaddown, c1).Key,
            GetButtonConfigByButton(dpadleft, c1).Key, (Keys)GetButtonConfigByButton(dpadleft, c1).Key,
            GetButtonConfigByButton(dpadright, c1).Key, (Keys)GetButtonConfigByButton(dpadright, c1).Key,
            GetButtonConfigByButton(buttona, c1).Key, (Keys)GetButtonConfigByButton(buttona, c1).Key,
            GetButtonConfigByButton(buttonb, c1).Key, (Keys)GetButtonConfigByButton(buttonb, c1).Key,
            GetButtonConfigByButton(buttonx, c1).Key, (Keys)GetButtonConfigByButton(buttonx, c1).Key,
            GetButtonConfigByButton(buttony, c1).Key, (Keys)GetButtonConfigByButton(buttony, c1).Key,
            GetButtonConfigByButton(buttonback, c1).Key, (Keys)GetButtonConfigByButton(buttonback, c1).Key,
            GetButtonConfigByButton(buttonstart, c1).Key, (Keys)GetButtonConfigByButton(buttonstart, c1).Key,
            GetButtonConfigByButton(bumperleft, c1).Key, (Keys)GetButtonConfigByButton(bumperleft, c1).Key,
            GetButtonConfigByButton(bumperright, c1).Key, (Keys)GetButtonConfigByButton(bumperright, c1).Key,
            GetButtonConfigByButton(triggerleft, c1).Key, (Keys)GetButtonConfigByButton(triggerleft, c1).Key,
            GetButtonConfigByButton(triggerright, c1).Key, (Keys)GetButtonConfigByButton(triggerright, c1).Key,
            GetButtonConfigByButton(ljoybutton, c1).Key, (Keys)GetButtonConfigByButton(ljoybutton, c1).Key,
            GetButtonConfigByButton(rjoybutton, c1).Key, (Keys)GetButtonConfigByButton(rjoybutton, c1).Key,
            GetButtonConfigByButton(ljoyup, c1).Key, (Keys)GetButtonConfigByButton(ljoyup, c1).Key,
            GetButtonConfigByButton(ljoydown, c1).Key, (Keys)GetButtonConfigByButton(ljoydown, c1).Key,
            GetButtonConfigByButton(ljoyleft, c1).Key, (Keys)GetButtonConfigByButton(ljoyleft, c1).Key,
            GetButtonConfigByButton(ljoyright, c1).Key, (Keys)GetButtonConfigByButton(ljoyright, c1).Key,
            GetButtonConfigByButton(rjoyup, c1).Key, (Keys)GetButtonConfigByButton(rjoyup, c1).Key,
            GetButtonConfigByButton(rjoydown, c1).Key, (Keys)GetButtonConfigByButton(rjoydown, c1).Key,
            GetButtonConfigByButton(rjoyleft, c1).Key, (Keys)GetButtonConfigByButton(rjoyleft, c1).Key,
            GetButtonConfigByButton(rjoyright, c1).Key, (Keys)GetButtonConfigByButton(rjoyright, c1).Key,

            BoolToString(c2.Enabled),
            c2.TriggerMagnitude,
            c2.JoystickMagnitude,
            BoolToString(c2.NormaliseJoystick),
            KeyboardListenModeToString(c2.KeyboardListenMode),
            GetButtonConfigByButton(dpadup, c2).Key, (Keys)GetButtonConfigByButton(dpadup, c2).Key,
            GetButtonConfigByButton(dpaddown, c2).Key, (Keys)GetButtonConfigByButton(dpaddown, c2).Key,
            GetButtonConfigByButton(dpadleft, c2).Key, (Keys)GetButtonConfigByButton(dpadleft, c2).Key,
            GetButtonConfigByButton(dpadright, c2).Key, (Keys)GetButtonConfigByButton(dpadright, c2).Key,
            GetButtonConfigByButton(buttona, c2).Key, (Keys)GetButtonConfigByButton(buttona, c2).Key,
            GetButtonConfigByButton(buttonb, c2).Key, (Keys)GetButtonConfigByButton(buttonb, c2).Key,
            GetButtonConfigByButton(buttonx, c2).Key, (Keys)GetButtonConfigByButton(buttonx, c2).Key,
            GetButtonConfigByButton(buttony, c2).Key, (Keys)GetButtonConfigByButton(buttony, c2).Key,
            GetButtonConfigByButton(buttonback, c2).Key, (Keys)GetButtonConfigByButton(buttonback, c2).Key,
            GetButtonConfigByButton(buttonstart, c2).Key, (Keys)GetButtonConfigByButton(buttonstart, c2).Key,
            GetButtonConfigByButton(bumperleft, c2).Key, (Keys)GetButtonConfigByButton(bumperleft, c2).Key,
            GetButtonConfigByButton(bumperright, c2).Key, (Keys)GetButtonConfigByButton(bumperright, c2).Key,
            GetButtonConfigByButton(triggerleft, c2).Key, (Keys)GetButtonConfigByButton(triggerleft, c2).Key,
            GetButtonConfigByButton(triggerright, c2).Key, (Keys)GetButtonConfigByButton(triggerright, c2).Key,
            GetButtonConfigByButton(ljoybutton, c2).Key, (Keys)GetButtonConfigByButton(ljoybutton, c2).Key,
            GetButtonConfigByButton(rjoybutton, c2).Key, (Keys)GetButtonConfigByButton(rjoybutton, c2).Key,
            GetButtonConfigByButton(ljoyup, c2).Key, (Keys)GetButtonConfigByButton(ljoyup, c2).Key,
            GetButtonConfigByButton(ljoydown, c2).Key, (Keys)GetButtonConfigByButton(ljoydown, c2).Key,
            GetButtonConfigByButton(ljoyleft, c2).Key, (Keys)GetButtonConfigByButton(ljoyleft, c2).Key,
            GetButtonConfigByButton(ljoyright, c2).Key, (Keys)GetButtonConfigByButton(ljoyright, c2).Key,
            GetButtonConfigByButton(rjoyup, c2).Key, (Keys)GetButtonConfigByButton(rjoyup, c2).Key,
            GetButtonConfigByButton(rjoydown, c2).Key, (Keys)GetButtonConfigByButton(rjoydown, c2).Key,
            GetButtonConfigByButton(rjoyleft, c2).Key, (Keys)GetButtonConfigByButton(rjoyleft, c2).Key,
            GetButtonConfigByButton(rjoyright, c2).Key, (Keys)GetButtonConfigByButton(rjoyright, c2).Key,

            BoolToString(c3.Enabled),
            c3.TriggerMagnitude,
            c3.JoystickMagnitude,
            BoolToString(c3.NormaliseJoystick),
            KeyboardListenModeToString(c3.KeyboardListenMode),
            GetButtonConfigByButton(dpadup, c3).Key, (Keys)GetButtonConfigByButton(dpadup, c3).Key,
            GetButtonConfigByButton(dpaddown, c3).Key, (Keys)GetButtonConfigByButton(dpaddown, c3).Key,
            GetButtonConfigByButton(dpadleft, c3).Key, (Keys)GetButtonConfigByButton(dpadleft, c3).Key,
            GetButtonConfigByButton(dpadright, c3).Key, (Keys)GetButtonConfigByButton(dpadright, c3).Key,
            GetButtonConfigByButton(buttona, c3).Key, (Keys)GetButtonConfigByButton(buttona, c3).Key,
            GetButtonConfigByButton(buttonb, c3).Key, (Keys)GetButtonConfigByButton(buttonb, c3).Key,
            GetButtonConfigByButton(buttonx, c3).Key, (Keys)GetButtonConfigByButton(buttonx, c3).Key,
            GetButtonConfigByButton(buttony, c3).Key, (Keys)GetButtonConfigByButton(buttony, c3).Key,
            GetButtonConfigByButton(buttonback, c3).Key, (Keys)GetButtonConfigByButton(buttonback, c3).Key,
            GetButtonConfigByButton(buttonstart, c3).Key, (Keys)GetButtonConfigByButton(buttonstart, c3).Key,
            GetButtonConfigByButton(bumperleft, c3).Key, (Keys)GetButtonConfigByButton(bumperleft, c3).Key,
            GetButtonConfigByButton(bumperright, c3).Key, (Keys)GetButtonConfigByButton(bumperright, c3).Key,
            GetButtonConfigByButton(triggerleft, c3).Key, (Keys)GetButtonConfigByButton(triggerleft, c3).Key,
            GetButtonConfigByButton(triggerright, c3).Key, (Keys)GetButtonConfigByButton(triggerright, c3).Key,
            GetButtonConfigByButton(ljoybutton, c3).Key, (Keys)GetButtonConfigByButton(ljoybutton, c3).Key,
            GetButtonConfigByButton(rjoybutton, c3).Key, (Keys)GetButtonConfigByButton(rjoybutton, c3).Key,
            GetButtonConfigByButton(ljoyup, c3).Key, (Keys)GetButtonConfigByButton(ljoyup, c3).Key,
            GetButtonConfigByButton(ljoydown, c3).Key, (Keys)GetButtonConfigByButton(ljoydown, c3).Key,
            GetButtonConfigByButton(ljoyleft, c3).Key, (Keys)GetButtonConfigByButton(ljoyleft, c3).Key,
            GetButtonConfigByButton(ljoyright, c3).Key, (Keys)GetButtonConfigByButton(ljoyright, c3).Key,
            GetButtonConfigByButton(rjoyup, c3).Key, (Keys)GetButtonConfigByButton(rjoyup, c3).Key,
            GetButtonConfigByButton(rjoydown, c3).Key, (Keys)GetButtonConfigByButton(rjoydown, c3).Key,
            GetButtonConfigByButton(rjoyleft, c3).Key, (Keys)GetButtonConfigByButton(rjoyleft, c3).Key,
            GetButtonConfigByButton(rjoyright, c3).Key, (Keys)GetButtonConfigByButton(rjoyright, c3).Key,

            BoolToString(c4.Enabled),
            c4.TriggerMagnitude,
            c4.JoystickMagnitude,
            BoolToString(c4.NormaliseJoystick),
            KeyboardListenModeToString(c4.KeyboardListenMode),
            GetButtonConfigByButton(dpadup, c4).Key, (Keys)GetButtonConfigByButton(dpadup, c4).Key,
            GetButtonConfigByButton(dpaddown, c4).Key, (Keys)GetButtonConfigByButton(dpaddown, c4).Key,
            GetButtonConfigByButton(dpadleft, c4).Key, (Keys)GetButtonConfigByButton(dpadleft, c4).Key,
            GetButtonConfigByButton(dpadright, c4).Key, (Keys)GetButtonConfigByButton(dpadright, c4).Key,
            GetButtonConfigByButton(buttona, c4).Key, (Keys)GetButtonConfigByButton(buttona, c4).Key,
            GetButtonConfigByButton(buttonb, c4).Key, (Keys)GetButtonConfigByButton(buttonb, c4).Key,
            GetButtonConfigByButton(buttonx, c4).Key, (Keys)GetButtonConfigByButton(buttonx, c4).Key,
            GetButtonConfigByButton(buttony, c4).Key, (Keys)GetButtonConfigByButton(buttony, c4).Key,
            GetButtonConfigByButton(buttonback, c4).Key, (Keys)GetButtonConfigByButton(buttonback, c4).Key,
            GetButtonConfigByButton(buttonstart, c4).Key, (Keys)GetButtonConfigByButton(buttonstart, c4).Key,
            GetButtonConfigByButton(bumperleft, c4).Key, (Keys)GetButtonConfigByButton(bumperleft, c4).Key,
            GetButtonConfigByButton(bumperright, c4).Key, (Keys)GetButtonConfigByButton(bumperright, c4).Key,
            GetButtonConfigByButton(triggerleft, c4).Key, (Keys)GetButtonConfigByButton(triggerleft, c4).Key,
            GetButtonConfigByButton(triggerright, c4).Key, (Keys)GetButtonConfigByButton(triggerright, c4).Key,
            GetButtonConfigByButton(ljoybutton, c4).Key, (Keys)GetButtonConfigByButton(ljoybutton, c4).Key,
            GetButtonConfigByButton(rjoybutton, c4).Key, (Keys)GetButtonConfigByButton(rjoybutton, c4).Key,
            GetButtonConfigByButton(ljoyup, c4).Key, (Keys)GetButtonConfigByButton(ljoyup, c4).Key,
            GetButtonConfigByButton(ljoydown, c4).Key, (Keys)GetButtonConfigByButton(ljoydown, c4).Key,
            GetButtonConfigByButton(ljoyleft, c4).Key, (Keys)GetButtonConfigByButton(ljoyleft, c4).Key,
            GetButtonConfigByButton(ljoyright, c4).Key, (Keys)GetButtonConfigByButton(ljoyright, c4).Key,
            GetButtonConfigByButton(rjoyup, c4).Key, (Keys)GetButtonConfigByButton(rjoyup, c4).Key,
            GetButtonConfigByButton(rjoydown, c4).Key, (Keys)GetButtonConfigByButton(rjoydown, c4).Key,
            GetButtonConfigByButton(rjoyleft, c4).Key, (Keys)GetButtonConfigByButton(rjoyleft, c4).Key,
            GetButtonConfigByButton(rjoyright, c4).Key, (Keys)GetButtonConfigByButton(rjoyright, c4).Key
        );

        var config = new Config {
            ControllerConfigs = controllerConfigs,
            Checksum = StringChecksum(code),
        };
        return code + $"\n--{JsonSerializer.Serialize(config)}";
    }
}

enum KeyboardListenMode {
    Any = 0,
    Keyboard0 = 1,
    Keyboard1 = 2,
    Keyboard2 = 3,
    Keyboard3 = 4,
}

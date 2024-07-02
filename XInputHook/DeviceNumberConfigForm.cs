namespace XInputHook;

public partial class DeviceNumberConfigForm : Form {
    class Entry {
        public required nint Value { get; init; }
        public required string Text { get; init; }

        public override string ToString() {
            return Text;
        }
    }

    private readonly Dictionary<nint, DeviceNumber> deviceNumbers;
    private nint lastInputDevice = -1; // INVALID_HANDLE_VALUE

    public DeviceNumberConfigForm(Dictionary<nint, DeviceNumber> deviceNumbers) {
        this.deviceNumbers = deviceNumbers;
        InitializeComponent();
        DeviceNumbersChanged();
    }

    public void DeviceNumbersChanged() {
        var keyboards = deviceNumbers
            .Where(d => d.Value.IsKeyboard())
            .OrderBy(d => d.Value);
        var mice = deviceNumbers
            .Where(d => d.Value.IsMouse())
            .OrderBy(d => d.Value);

        keyboardsListBox.Items.Clear();
        foreach (var keyboard in keyboards) {
            var ordinal = keyboard.Value.GetKeyboardOrdinal();
            keyboardsListBox.Items.Add(new Entry {
                Value = keyboard.Key,
                Text = $"[{ordinal}] Keyboard ID 0x{keyboard.Key:X}",
            });
        }
        miceListBox.Items.Clear();
        foreach (var mouse in mice) {
            var ordinal = mouse.Value.GetMouseOrdinal();
            miceListBox.Items.Add(new Entry {
                Value = mouse.Key,
                Text = $"[{ordinal}] Mouse ID 0x{mouse.Key:X}",
            });
        }

        keyboardUpButton.Enabled = false;
        keyboardDownButton.Enabled = false;
        mouseUpButton.Enabled = false;
        mouseDownButton.Enabled = false;
        ActiveControl = null;
    }

    public void InputReceived(nint device, DeviceNumber number) {
        if (lastInputDevice != device) {
            lastInputDevice = device;
            var type = number.IsKeyboard() ? "Keyboard" : "Mouse";
            statusLabel.Text = $"Last input received from device 0x{device:X} ({type})";
        }
    }

    private void KeyboardSelectedIndexChanged(object sender, EventArgs e) {
        var index = keyboardsListBox.SelectedIndex;
        if (index < 0) {
            keyboardUpButton.Enabled = false;
            keyboardDownButton.Enabled = false;
            return;
        }
        keyboardUpButton.Enabled = index > 0;
        keyboardDownButton.Enabled = index < keyboardsListBox.Items.Count - 1;
    }

    private void MouseSelectedIndexChanged(object sender, EventArgs e) {
        var index = miceListBox.SelectedIndex;
        if (index < 0) {
            mouseUpButton.Enabled = false;
            mouseDownButton.Enabled = false;
            return;
        }
        mouseUpButton.Enabled = index > 0;
        mouseDownButton.Enabled = index < miceListBox.Items.Count - 1;
    }

    private void SwapDeviceNumbers(nint a, nint b) {
        (deviceNumbers[b], deviceNumbers[a]) = (deviceNumbers[a], deviceNumbers[b]);
        DeviceNumbersChanged();
    }

    private void KeyboardUpButtonPressed(object sender, EventArgs e) {
        var swapIndex = keyboardsListBox.SelectedIndex - 1;
        var item = (Entry)keyboardsListBox.SelectedItem!;
        var swap = (Entry)keyboardsListBox.Items[swapIndex];
        SwapDeviceNumbers(item.Value, swap.Value);
        keyboardsListBox.SelectedIndex = swapIndex;
    }

    private void KeyboardDownButtonPressed(object sender, EventArgs e) {
        var swapIndex = keyboardsListBox.SelectedIndex + 1;
        var item = (Entry)keyboardsListBox.SelectedItem!;
        var swap = (Entry)keyboardsListBox.Items[swapIndex];
        SwapDeviceNumbers(item.Value, swap.Value);
        keyboardsListBox.SelectedIndex = swapIndex;
    }

    private void MouseUpButtonPressed(object sender, EventArgs e) {
        var swapIndex = miceListBox.SelectedIndex - 1;
        var item = (Entry)miceListBox.SelectedItem!;
        var swap = (Entry)miceListBox.Items[swapIndex];
        SwapDeviceNumbers(item.Value, swap.Value);
        miceListBox.SelectedIndex = swapIndex;
    }

    private void MouseDownButtonPressed(object sender, EventArgs e) {
        var swapIndex = miceListBox.SelectedIndex + 1;
        var item = (Entry)miceListBox.SelectedItem!;
        var swap = (Entry)miceListBox.Items[swapIndex];
        SwapDeviceNumbers(item.Value, swap.Value);
        miceListBox.SelectedIndex = swapIndex;
    }
}

public enum DeviceNumber {
    DEVICE_KEYBOARD_0 = 100,
    DEVICE_MOUSE_0 = 200,
};

public static class DeviceNumberExtensions {
    public static bool IsKeyboard(this DeviceNumber deviceNumber) {
        return deviceNumber >= DeviceNumber.DEVICE_KEYBOARD_0
            && deviceNumber < DeviceNumber.DEVICE_KEYBOARD_0 + 100;
    }

    public static bool IsMouse(this DeviceNumber deviceNumber) {
        return deviceNumber >= DeviceNumber.DEVICE_MOUSE_0
            && deviceNumber < DeviceNumber.DEVICE_MOUSE_0 + 100;
    }

    public static nint GetKeyboardOrdinal(this DeviceNumber deviceNumber) {
        return deviceNumber - DeviceNumber.DEVICE_KEYBOARD_0;
    }

    public static nint GetMouseOrdinal(this DeviceNumber deviceNumber) {
        return deviceNumber - DeviceNumber.DEVICE_MOUSE_0;
    }
}

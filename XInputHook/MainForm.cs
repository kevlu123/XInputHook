using System.Runtime.InteropServices;
using System.Diagnostics;

namespace XInputHook;

public enum InputDeviceType {
    Mouse = 0,
    Keyboard = 1,
}

public partial class MainForm : Form {
    private Profile currentProfile;
    private PipeServer? pipeServer;
    private DeviceNumberConfigForm? deviceNumberConfigForm;
    private Dictionary<nint, DeviceNumber> deviceNumbers = [];

    public MainForm() {
        InitializeComponent();
        exitContextMenuItem.Click += (s, e) => Close();

        ListenToRawInput(Handle);
        currentProfile = Profile.GetSelectedProfile();
        ShowProfile(currentProfile);

        Text += $" v{Program.Version}";
    }

    private void RunButtonClicked(object sender, EventArgs e) {
        if (pipeServer != null) {
            pipeServer.WriteMessage(new BeginMessage {
                Script = currentProfile.GetScript(),
            });
            return;
        }

        var dllPath = Program.GetResourcePath("Injectee64.dll");
        if (!RunWithDll(currentProfile.ProgramPath, currentProfile.Arguments, currentProfile.GetWorkingDirectoryOrDefault(), dllPath)) {
            MessageBox.Show("Failed to run the program.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try {
            var script = currentProfile.GetScript();
            pipeServer = new PipeServer {
                OnError = (e) => Invoke(() => {
                    if (!IsDisposed) OnPipeError(e);
                }),
                OnMessage = (m) => Invoke(() => {
                    if (!IsDisposed) OnPipeMessage(m);
                }),
            };

            runButton.Text = "Reload";
            WriteOutput("======== Running ========\n");

            pipeServer.WriteMessage(new BeginMessage {
                Script = script,
            });
            pipeServer.WriteMessage(new SetDeviceNumbersMessage {
                DeviceNumbers = deviceNumbers,
            });
        } catch (Exception ex) {
            OnPipeError(ex);
        }
    }

    private void NewProfileButtonClicked(object sender, EventArgs e) {
        ShowProfile(Profile.CreateNewDefaultProfile());
    }

    private void DeleteProfileButtonClicked(object sender, EventArgs e) {
        var result = MessageBox.Show(
            "Are you sure you want to delete this profile?",
            "Delete Profile",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (result == DialogResult.Yes) {
            Profile.DeleteProfile(currentProfile);
            ShowProfile(Profile.Profiles[0]);
        }
    }

    private void DuplicateProfileButtonClicked(object sender, EventArgs e) {
        var profile = currentProfile.Clone();
        Profile.AddProfile(profile);
        ShowProfile(profile);
    }

    private void EditScriptButtonClicked(object sender, EventArgs e) {
        var path = currentProfile.GetScriptPath();
        var process = new Process {
            StartInfo = new ProcessStartInfo(path) {
                UseShellExecute = true
            }
        };
        process.Start();
    }

    private void ProfileComboBoxSelectedValueChanged(object sender, EventArgs e) {
        var selectedItem = profileComboBox.SelectedItem;
        if (selectedItem != null) {
            currentProfile = (Profile)selectedItem;
            ShowProfile(currentProfile, updateDataSource: false);
        }
    }

    private void ProfileFieldFocusLeave(object sender, EventArgs e) {
        try {
            if (currentProfile.Name != profileComboBox.Text && File.Exists(currentProfile.GetScriptPath())) {
                File.Copy(
                    currentProfile.GetScriptPath(),
                    Profile.GetScriptPath(profileComboBox.Text),
                    overwrite: true);
            }
        } catch { }
        currentProfile.Name = profileComboBox.Text;
        currentProfile.ProgramPath = programTextBox.Text;
        currentProfile.Arguments = argumentsTextBox.Text;
        currentProfile.WorkingDirectory = workingDirectoryTextBox.Text;
        Profile.UpdateProfile(currentProfile);
        ShowProfile(currentProfile);
    }

    private void ShowProfile(Profile profile, bool updateDataSource = true) {
        currentProfile = profile;
        if (updateDataSource) {
            profileComboBox.DataSource = null;
            profileComboBox.DataSource = Profile.Profiles;
            profileComboBox.SelectedItem = profile;
        }
        programTextBox.Text = currentProfile.ProgramPath;
        argumentsTextBox.Text = currentProfile.Arguments;
        workingDirectoryTextBox.Text = currentProfile.WorkingDirectory;
    }

    private void ClosePipe() {
        if (pipeServer == null) {
            return;
        }

        pipeServer.Dispose();
        pipeServer = null;
        runButton.Text = "Run";
        WriteOutput("======== Stopped ========\n");
    }

    private void OnPipeError(Exception e) {
        if (e is not EndOfStreamException) {
            WriteOutput($"Error: {e.Message}\n");
        }
        ClosePipe();
    }

    private void OnPipeMessage(IReceiveableMessage message) {
        if (message is PrintMessage printMessage) {
            WriteOutput(printMessage.Text);
        }
    }

    private void WriteOutput(string text) {
        outputTextBox.AppendText(text.ReplaceLineEndings());
    }

    private void QuickConfigButtonPressed(object sender, EventArgs e) {
        using var dialog = new QuickConfigForm(currentProfile);
        dialog.ShowDialog();
    }

    private void EditDeviceNumbersButtonClicked(object sender, EventArgs e) {
        try {
            using var dialog = new DeviceNumberConfigForm(deviceNumbers);
            deviceNumberConfigForm = dialog;
            dialog.ShowDialog();
            pipeServer?.WriteMessage(new SetDeviceNumbersMessage {
                DeviceNumbers = deviceNumbers,
            });
        } finally {
            deviceNumberConfigForm = null;
        }
    }

    private void OnFormClosed(object sender, FormClosedEventArgs e) {
        Profile.SetSelectedProfile(currentProfile);
    }

    private void OnTrayDoubleClicked(object sender, MouseEventArgs e) {
        WindowState = FormWindowState.Normal;
    }

    private void OnFormResized(object sender, EventArgs e) {
        ShowInTaskbar = WindowState != FormWindowState.Minimized;
    }

    protected override void WndProc(ref Message m) {
        // WM_INPUT
        if (m.Msg == 0x00FF && deviceNumbers.Count < 100) {
            GetRawInputInfo(m.LParam, out var device, out var type);
            DeviceNumber deviceNumber;
            if (type == InputDeviceType.Keyboard) {
                deviceNumber = DeviceNumber.DEVICE_KEYBOARD_0 + deviceNumbers.Count(d => d.Value.IsKeyboard());
            } else {
                deviceNumber = DeviceNumber.DEVICE_MOUSE_0 + deviceNumbers.Count(d => d.Value.IsMouse());
            }
            deviceNumberConfigForm?.InputReceived(device, deviceNumber);
            if (deviceNumbers.TryAdd(device, deviceNumber)) {
                Debug.WriteLine($"Device: {device:X} ({type})");
                deviceNumberConfigForm?.DeviceNumbersChanged();
                pipeServer?.WriteMessage(new SetDeviceNumbersMessage {
                    DeviceNumbers = deviceNumbers,
                });
            }
        }

        base.WndProc(ref m);
    }

    [LibraryImport("DetoursWrapper32.dll", EntryPoint = "RunWithDll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int RunWithDll32(string programPath, string args, string workingDirectory, [MarshalAs(UnmanagedType.LPStr)] string dllPath);

    [LibraryImport("DetoursWrapper64.dll", EntryPoint = "RunWithDll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int RunWithDll64(string programPath, string args, string workingDirectory, [MarshalAs(UnmanagedType.LPStr)] string dllPath);

    private static bool RunWithDll(string programPath, string args, string workingDirectory, string dllPath) {
        return Environment.Is64BitProcess
            ? (RunWithDll64(programPath, args, workingDirectory, dllPath) == 0)
            : (RunWithDll32(programPath, args, workingDirectory, dllPath) == 0);
    }

    [LibraryImport("DetoursWrapper32", EntryPoint = "GetRawInputInfo")]
    private static unsafe partial void GetRawInputInfo32(nint hRawInput, out nint device, out int type);

    [LibraryImport("DetoursWrapper64", EntryPoint = "GetRawInputInfo")]
    private static unsafe partial void GetRawInputInfo64(nint hRawInput, out nint device, out int type);

    public static void GetRawInputInfo(nint hRawInput, out nint device, out InputDeviceType type) {
        unsafe {
            int t;
            if (Environment.Is64BitProcess) {
                GetRawInputInfo64(hRawInput, out device, out t);
            } else {
                GetRawInputInfo32(hRawInput, out device, out t);
            }
            type = (InputDeviceType)t;
        }
    }

    [LibraryImport("DetoursWrapper32", EntryPoint = "ListenToRawInput")]
    private static partial void ListenToRawInput32(nint hWnd);

    [LibraryImport("DetoursWrapper64", EntryPoint = "ListenToRawInput")]
    private static partial void ListenToRawInput64(nint hWnd);

    public static void ListenToRawInput(nint hWnd) {
        if (Environment.Is64BitProcess) {
            ListenToRawInput64(hWnd);
        } else {
            ListenToRawInput32(hWnd);
        }
    }

    private void ProgramPickButtonClicked(object sender, EventArgs e) {
        openFileDialog.Title = "Select Program";
        openFileDialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
        if (openFileDialog.ShowDialog() != DialogResult.OK) {
            return;
        }
        programTextBox.Text = openFileDialog.FileName;
    }
}

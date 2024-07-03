namespace XInputHook;

static class Program {
    public const string Version = "1.1";

    [STAThread]
    static void Main() {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }

    public static string GetResourcePath(string name) {
        return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath) ?? "", name);
    }
}
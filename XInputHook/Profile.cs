using System.Text.Json;
using System.Text.Json.Serialization;

namespace XInputHook;

public class Profile {
    public required string Name { get; set; }
    public required string ProgramPath { get; set; }
    public required string Arguments { get; set; }
    public required string WorkingDirectory { get; set; }

    [JsonInclude]
    private bool Selected { get; set; }

    public Profile Clone() {
        return new Profile {
            Name = Name,
            ProgramPath = ProgramPath,
            Arguments = Arguments,
            WorkingDirectory = WorkingDirectory,
        };
    }

    public static string GetScriptPath(string name) {
        return Program.GetResourcePath($"Scripts\\{name}.lua");
    }

    public string GetScriptPath() {
        var path = GetScriptPath(Name);
        try {
            if (!File.Exists(path)) {
                File.WriteAllText(path, "");
            }
        } catch { }
        return path;
    }

    public string GetScript() {
        return File.ReadAllText(GetScriptPath());
    }

    public void SetScript(string script) {
        File.WriteAllText(GetScriptPath(), script);
    }

    public override string ToString() {
        return Name;
    }

    private const string PROFILES_FILENAME = "Profiles.json";
    private static readonly JsonSerializerOptions jsonOptions = new() {
        WriteIndented = true,
    };

    public static List<Profile> Profiles { get; }

    static Profile() {
        Profiles = LoadProfiles();
        CreateProfileIfEmpty();
    }

    private static List<Profile> LoadProfiles() {
        try {
            Directory.CreateDirectory(Program.GetResourcePath("Scripts"));
        } catch { }

        try {
            var path = Program.GetResourcePath(PROFILES_FILENAME);
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Profile>>(json)
                ?.OrderBy(p => p.Name).ToList()
                ?? [];
        } catch {
            return [];
        }
    }

    private static void CreateProfileIfEmpty() {
        if (Profiles.Count == 0) {
            CreateNewDefaultProfile();
        }
    }

    public string GetWorkingDirectoryOrDefault() {
        try {
            return WorkingDirectory.Length == 0
                ? Path.GetDirectoryName(ProgramPath) ?? ""
                : WorkingDirectory;
        } catch {
            return WorkingDirectory;
        }
    }

    private static void InsertProfile(Profile profile) {
        Profiles.Add(profile);
        Profiles.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
    }

    public static void SaveProfiles() {
        try {
            var path = Program.GetResourcePath(PROFILES_FILENAME);
            var json = JsonSerializer.Serialize(Profiles, jsonOptions);
            File.WriteAllText(path, json);
        } catch { }
    }

    public static Profile CreateNewDefaultProfile() {
        int i = 1;
        while (Profiles.Any(p => p.Name == $"Profile {i}")) {
            i++;
        }

        var profile = new Profile {
            Name = $"Profile {i}",
            ProgramPath = "",
            Arguments = "",
            WorkingDirectory = ""
        };
        InsertProfile(profile);
        SaveProfiles();
        return profile;
    }

    public static void DeleteProfile(Profile profile) {
        Profiles.Remove(profile);
        CreateProfileIfEmpty();
        SaveProfiles();
    }

    public static void AddProfile(Profile profile) {
        InsertProfile(profile);
        SaveProfiles();
    }

    public static void UpdateProfile(Profile profile) {
        Profiles.Remove(profile);
        InsertProfile(profile);
        SaveProfiles();
    }

    public static void SetSelectedProfile(Profile profile) {
        foreach (var p in Profiles) {
            p.Selected = p == profile;
        }
        SaveProfiles();
    }

    public static Profile GetSelectedProfile() {
        return Profiles.Find(p => p.Selected)
            ?? Profiles[0];
    }
}

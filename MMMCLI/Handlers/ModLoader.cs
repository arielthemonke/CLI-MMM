using System.IO.Compression;
using Newtonsoft.Json;

namespace MMMCLI.Handlers;

public class ModLoader
{
    public static List<ModInfo> mods = new List<ModInfo>();
    public static async Task LoadModsFromGithubRepoAsync()
    {
        var url = "https://raw.githubusercontent.com/The-Graze/MonkeModInfo/refs/heads/master/modinfo.json";
        
        using var client = new HttpClient();
        var json = await client.GetStringAsync(url);
        mods = JsonConvert.DeserializeObject<List<ModInfo>>(json);
    }

    public static async Task<bool> CheckForBepinexAsync()
    {
        if (Directory.Exists(Path.Combine(Program.GamePath, "BepInEx")))
        {
            if (File.Exists(Path.Combine(Program.GamePath, "winhttp.dll")))
            {
                return true;
            }
        }

        await InstallBepInExAsync();
        return false;
    }

    private static async Task InstallBepInExAsync()
    {
        var url = "https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.3/BepInEx_win_x64_5.4.23.3.zip";
        var zipPath = Path.Combine(Path.GetTempPath(), "BepInEx.zip");

        Console.WriteLine("Downloading BepInEx...");
        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await response.Content.CopyToAsync(fs);
            }
        }
        Console.WriteLine("Download complete.");

        Console.WriteLine("Extracting BepInEx...");
        ZipFile.ExtractToDirectory(zipPath, Program.GamePath, true);
        Console.WriteLine("extraction complete.");

        File.Delete(zipPath);
        Console.WriteLine("Cleaned up temporary files.");
        await FixBepInExConfig();
    }

    public static async Task FixBepInExConfig()
    {
        Console.WriteLine("Fixing BepInEx config...");
        string url = "https://raw.githubusercontent.com/The-Graze/MonkeModInfo/master/BepInEx.cfg";
        string configPath = Path.Combine(Program.GamePath, "BepInEx", "config", "BepInEx.cfg");

        try
        {
            using var client = new HttpClient();
            Console.WriteLine("Downloading BepInEx config...");
            var configContent = await client.GetStringAsync(url);
            Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
            await File.WriteAllTextAsync(configPath, configContent);
            Console.WriteLine("Config downloaded and applied.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"never mind, it didnt work :sob: {ex}");
        }
    }
    public static async Task ShowModMenuButNotCheatMenuAsync()
    {
        var enhancedMods = mods.Select(modInfo => new ModInfo
        {
            name = modInfo.name,
            author = modInfo.author,
            version = modInfo.version,
            download_url = modInfo.download_url,
            install_location = modInfo.install_location,
            dependencies = modInfo.dependencies
        }).ToList();
        EnhancedInstaller.Initialize(Program.GamePath, enhancedMods);
        await EnhancedInstaller.ShowModMenu();
    }
}
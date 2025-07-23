using System.IO.Compression;
using static MMMCLI.Handlers.ModLoader;

namespace MMMCLI.Handlers;

public static class EnhancedInstaller
{
    private static readonly HttpClient httpClient = new ();
    private static List<ModInfo> mods = new ();
    private static string gamePath;

    public static async Task<string> DownloadFile(string url, string fileName)
    {
        try
        {
            Console.WriteLine($"Downloading {fileName}...");
            
            var fileBytes = await httpClient.GetByteArrayAsync(url);
            var downloadDir = Path.GetTempPath();
            Directory.CreateDirectory(downloadDir);
            
            var filePath = Path.Combine(downloadDir, fileName);
            await File.WriteAllBytesAsync(filePath, fileBytes);
            
            Console.WriteLine("Download completed!");
            return filePath;
        }
        catch (Exception ex)
        {
            throw new Exception($"Download failed: {ex.Message}", ex);
        }
    }

    private static ModInfo GetModFromString(string mod)
    {
        return mods.FirstOrDefault(x => x.name == mod);
    }

    public static async Task<bool> InstallMod(ModInfo mod, ModInfo modSender = null)
    {
        if (mod == null)
            throw new ArgumentNullException(nameof(mod));

        try
        {
            Console.WriteLine($"Installing {mod.name}...");

            if (mod.dependencies != null && mod.dependencies.Length > 0)
            {
                Console.WriteLine($"Installing dependencies...");
                foreach (var dep in mod.dependencies)
                {
                    var depmod = GetModFromString(dep);
                    if (depmod != modSender)
                    {
                        InstallMod(depmod, mod);
                    }
                }
                Console.WriteLine("Dependencies installed!");
                Console.WriteLine();
            }

            var installLocation = !string.IsNullOrEmpty(mod.install_location) 
                ? mod.install_location
                : "BepInEx/plugins";
            
            var targetDirectory = Path.Combine(gamePath, installLocation);
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
                Console.WriteLine($"Created directory: {targetDirectory}");
            }

            if (string.IsNullOrEmpty(mod.download_url))
            {
                throw new InvalidOperationException("Mod download URL is empty");
            }

            var uri = new Uri(mod.download_url);
            var fileName = Path.GetFileName(uri.LocalPath);
            
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = $"{mod.name}.dll";
            }

            var downloadPath = await DownloadFile(mod.download_url, fileName);

            if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Extracting ZIP archive...");
                
                using (var archive = ZipFile.OpenRead(downloadPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (string.IsNullOrEmpty(entry.Name))
                            continue;
                            
                        var entryPath = entry.FullName.Replace('\\', '/');
                        
                        if (entryPath.Contains("../") || Path.IsPathRooted(entryPath))
                        {
                            Console.WriteLine($"Skipping unsafe path: {entryPath}");
                            continue;
                        }

                        var destinationPath = Path.Combine(targetDirectory, entry.FullName);
                        var fullDestinationPath = Path.GetFullPath(destinationPath);
                        var fullTargetPath = Path.GetFullPath(targetDirectory);
                        
                        if (!fullDestinationPath.StartsWith(fullTargetPath, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"Skipping path outside target directory: {entryPath}");
                            continue;
                        }

                        var directory = Path.GetDirectoryName(destinationPath);
                        
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        entry.ExtractToFile(destinationPath, overwrite: true);
                        Console.WriteLine($"Extracted: {Path.GetRelativePath(targetDirectory, destinationPath)}");
                    }
                }
                
                if (File.Exists(downloadPath))
                {
                    File.Delete(downloadPath);
                }
            }
            else
            {
                var modFolder = Path.Combine(targetDirectory, mod.name);
                if (!Directory.Exists(modFolder))
                {
                    Directory.CreateDirectory(modFolder);
                }

                var targetPath = Path.Combine(modFolder, fileName);

                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }

                File.Move(downloadPath, targetPath);
                Console.WriteLine($"Installed file: {targetPath}");
            }

            Console.WriteLine($"Successfully installed {mod.name} v{mod.version}!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to install {mod.name}: {ex}");
            return false;
        }
    }

    public static async Task ShowModMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.Clear();
            Console.WriteLine("╔═══════════════════════════════════════╗");
            Console.WriteLine("║         Monke Mod Manager CLI         ║");
            Console.WriteLine("╚═══════════════════════════════════════╝");
            Console.WriteLine();

            for (int i = 0; i < mods.Count; i++)
            {
                var mod = mods[i];
                Console.WriteLine($"[{i + 1:D2}] {mod.name} by {mod.author} - v{mod.version}");
                
                if (mod.dependencies != null && mod.dependencies.Length > 0)
                {
                    Console.WriteLine($"     Dependencies: {string.Join(", ", mod.dependencies)}");
                }
            }

            Console.WriteLine("[0] Exit");
            Console.WriteLine();
            Console.Write("Enter the number of the mod to install: ");

            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
                continue;

            if (input.Equals("0"))
            {
                Console.WriteLine("Exiting Monke Mod Manager...");
                break;
            }
            else if (int.TryParse(input, out int selection) && selection >= 1 && selection <= mods.Count)
            {
                var selectedMod = mods[selection - 1];
                Console.Clear();
                Console.WriteLine($"═══ {selectedMod.name.ToUpper()} ═══");
                Console.WriteLine($"Author: {selectedMod.author}");
                Console.WriteLine($"Version: {selectedMod.version}");
                Console.WriteLine();
                
                if (selectedMod.dependencies != null && selectedMod.dependencies.Length > 0)
                {
                    Console.WriteLine("This mod has dependencies:");
                    foreach (var dep in selectedMod.dependencies)
                    {
                        Console.WriteLine($"   - {dep}");
                    }
                    Console.WriteLine("Make sure these are installed first!");
                    Console.WriteLine();
                }
                
                Console.Write("Do you want to install this mod? (Y/n): ");
                var confirm = Console.ReadLine()?.ToLower();
                
                if (confirm == "y" || confirm == "yes" || confirm == "")
                {
                    var success = await InstallMod(selectedMod);
                    Console.WriteLine();
                    Console.WriteLine(success ? "Installation completed!" : "Installation failed!");
                    Console.WriteLine("Press Enter to continue...");
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("Invalid selection. Press Enter to continue...");
                Console.ReadLine();
            }
        }
    }

    public static void Initialize(string gamePathParam, List<ModInfo> modList)
    {
        gamePath = gamePathParam;
        mods = modList;
    }
}
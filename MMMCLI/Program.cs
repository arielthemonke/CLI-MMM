using MMMCLI.Handlers;

namespace MMMCLI;

public class Program
{
    public static string GamePath;

    static async Task Main(string[] ars)
    {
        await ModLoader.LoadModsFromGithubRepoAsync();
        GamePath = FindGamePath();
        _ = await ModLoader.CheckForBepinexAsync();
        await ModLoader.ShowModMenuButNotCheatMenuAsync();
    }

    static string FindGamePath()
    {
        var steamDir = @"C:\Program Files (x86)\Steam\steamapps\common\Gorilla Tag";
        var oculusDir = @"C:\Program Files\Oculus\Software\Software\another-axiom-gorilla-tag";
        if (Directory.Exists(steamDir))
        {
            return steamDir;
        }
        if (Directory.Exists(oculusDir))
        {
            return oculusDir;
        }
        while (true)
        {
            Console.Write("Could not find Gorilla Tag automatically using the most advanced technology.\nPlease enter your game directory manually: ");
            string gameDir = Console.ReadLine()?.Trim('"');

            if (!string.IsNullOrEmpty(gameDir) && Directory.Exists(gameDir))
            {
                Console.WriteLine($"Directory inputed: {gameDir}");
                return gameDir;
            }

            Console.WriteLine("invalid directory, please try again");
        }
    }
}
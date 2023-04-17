using System.Text.Json;
using ImprovedConsole;

namespace altVPackage
{
    public class Program
    {
        private static readonly List<string> Branches = new() { "release", "rc", "dev" };
        private static Config Config = new Config();
        private const string CDN_URL = "cdn.alt-mp.com";

        public static async Task Main(string[] args)
        {
            ConsoleWrapper.WriteLine("alt:V Package started", LogType.Success, true);

            if (!File.Exists("./config.json"))
            {
                ConsoleWrapper.WriteLine("config file not found", LogType.Error, true);
                ConsoleWrapper.WriteLine("Please enter branch name");

                var branch = Console.ReadLine()?.ToLower() ?? "release";
                Config.Branch = !Branches.Contains(branch) ? Branches.First() : branch;

                ConsoleWrapper.WriteLine("Windows build? (y/n)");
                Config.Windows = Console.ReadLine()?.ToLower() == "y";

                ConsoleWrapper.WriteLine("Do you want server build? (y/n)");
                Config.Server = Console.ReadLine()?.ToLower() == "y";

                ConsoleWrapper.WriteLine("Do you want voice build? (y/n)");
                Config.Voice = Console.ReadLine()?.ToLower() == "y";

                ConsoleWrapper.WriteLine("Do you want CSharp build? (y/n)");
                Config.CSharp = Console.ReadLine()?.ToLower() == "y";

                ConsoleWrapper.WriteLine("Do you want JS build? (y/n)");
                Config.Js = Console.ReadLine()?.ToLower() == "y";

                if (Config.Branch == "release")
                {
                    ConsoleWrapper.WriteLine("Do you want JS bytecode build? (y/n)");
                    Config.JsByteCode = Console.ReadLine()?.ToLower() == "y";
                }

                ConsoleWrapper.WriteLine("Input output path");
                Config.OutputPath = Console.ReadLine() ?? "./";

                await File.WriteAllTextAsync("./config.json", JsonSerializer.Serialize(Config));
                ConsoleWrapper.WriteLine("config file created", LogType.Success, true);
            }
            else
            {
                Config = JsonSerializer.Deserialize<Config>(File.ReadAllText("./config.json"));
                if (Config is null)
                {
                    ConsoleWrapper.WriteLine("config file is invalid, remove it and run the program again", LogType.Error, true);
                    return;
                }
            }

            var system = Config.Windows ? "x64_win32" : "x64_linux";
            
            string version = await Utils.GetVersion(Config.Branch, system) ?? "unknown";
            ConsoleWrapper.WriteLine($"Begin packages download for branch: {Config.Branch}, version: {version}, system: {system}", LogType.Info, true);

            if (Config.Server)
            {
                Directory.CreateDirectory($@"{Config.OutputPath}/data");
                await DownloadServer(system);
            }

            if (Config.CSharp)
            {
                Directory.CreateDirectory($@"{Config.OutputPath}/modules");
                await DownloadCsharp(system);
            }

            if (Config.Js)
            {
                Directory.CreateDirectory($@"{Config.OutputPath}/modules/js-module");
                await DownloadJs(system);
            }

            if (Config.Voice) await DownloadVoice(system);

            if (Config is { JsByteCode: true, Branch: "release" }) await DownloadJsByteCode(system);

            ConsoleWrapper.WriteLine("Download finished", LogType.Success, true);
            Console.ReadKey();
        }

        private static async Task DownloadServer(string system)
        {
            ConsoleWrapper.WriteLine("Starting server download", LogType.Info, true);

            if (await Utils.CalculateHash($"{Config.OutputPath}/{(Config.Windows ? "altv-server.exe" : "altv-server")}") !=
                await Utils.GetUpdatedHash($"https://{CDN_URL}/server/{Config.Branch}/{system}/update.json",
                    $"{(Config.Windows ? "altv-server.exe" : "altv-server")}"))
            {
                await Utils.DownloadFile(
                    $"https://{CDN_URL}/server/{Config.Branch}/{system}/{(Config.Windows ? "altv-server.exe" : "altv-server")}",
                    $"{Config.OutputPath}/{(Config.Windows ? "altv-server.exe" : "altv-server")}");
                
                ConsoleWrapper.WriteLine("Server is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine("Server is up to date and skipped", LogType.Info, true);

            if (await Utils.CalculateHash($"{Config.OutputPath}/data/vehmodels.bin") !=
                await Utils.GetUpdatedHash($"https://{CDN_URL}/data/{Config.Branch}/update.json",
                    "vehmodels.bin"))
            {
                await Utils.DownloadFile($"https://{CDN_URL}/data/{Config.Branch}/data/vehmodels.bin",
                    $"{Config.OutputPath}/data/vehmodels.bin");
                
                ConsoleWrapper.WriteLine("vehmodels.bin is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine("vehmodels.bin is up to date and skipped", LogType.Info, true);

            if (await Utils.CalculateHash($"{Config.OutputPath}/data/vehmods.bin") !=
                await Utils.GetUpdatedHash($"https://{CDN_URL}/data/{Config.Branch}/update.json",
                    "vehmods.bin"))
            {
                await Utils.DownloadFile($"https://{CDN_URL}/data/{Config.Branch}/data/vehmods.bin",
                    $"{Config.OutputPath}/data/vehmods.bin");
                
                ConsoleWrapper.WriteLine("vehmods.bin is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine("vehmods.bin is up to date and skipped", LogType.Info, true);

            if (await Utils.CalculateHash($"{Config.OutputPath}/data/clothes.bin") !=
                await Utils.GetUpdatedHash($"https://{CDN_URL}/data/{Config.Branch}/update.json",
                    "clothes.bin"))
            {
                await Utils.DownloadFile($"https://{CDN_URL}/data/{Config.Branch}/data/clothes.bin",
                    $"{Config.OutputPath}/data/clothes.bin");
                
                ConsoleWrapper.WriteLine("clothes.bin is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine("clothes.bin is up to date and skipped", LogType.Info, true);

            if (await Utils.CalculateHash($"{Config.OutputPath}/data/pedmodels.bin") !=
                await Utils.GetUpdatedHash($"https://{CDN_URL}/data/{Config.Branch}/update.json",
                    "pedmodels.bin"))
            {
                await Utils.DownloadFile($"https://{CDN_URL}/data/{Config.Branch}/data/pedmodels.bin",
                    $"{Config.OutputPath}/data/pedmodels.bin");
                
                ConsoleWrapper.WriteLine("pedmodels.bin is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine("pedmodels.bin is up to date and skipped", LogType.Info, true);
            
            ConsoleWrapper.WriteLine("Finish server download", LogType.Success, true);
        }

        private static async Task DownloadVoice(string system)
        {
            ConsoleWrapper.WriteLine("Starting voice server download", LogType.Info, true);

            if (await Utils.CalculateHash(
                    $"{Config.OutputPath}/{(Config.Windows ? "altv-voice-server.exe" : "altv-voice-server")}") !=
                await Utils.GetUpdatedHash($"https://{CDN_URL}/voice-server/{Config.Branch}/{system}/update.json",
                    $"{(Config.Windows ? "altv-voice-server.exe" : "altv-voice-server")}"))
            {
                await Utils.DownloadFile(
                    $"https://{CDN_URL}/voice-server/{Config.Branch}/{system}/{(Config.Windows ? "altv-voice-server.exe" : "altv-voice-server")}",
                    $"{Config.OutputPath}/{(Config.Windows ? "altv-voice-server.exe" : "altv-voice-server")}");
                
                ConsoleWrapper.WriteLine("altv-voice-server.exe is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine("Voice is up to date and skipped", LogType.Info, true);
            
            ConsoleWrapper.WriteLine("Finish voice server download", LogType.Success, true);
        }

        private static async Task DownloadCsharp(string system)
        {
            ConsoleWrapper.WriteLine("Starting csharp download", LogType.Info, true);

            if (await Utils.CalculateHash($"{Config.OutputPath}/AltV.Net.Host.dll") != await Utils.GetUpdatedHash(
                    $"https://{CDN_URL}/coreclr-module/{Config.Branch}/{system}/update.json",
                    "AltV.Net.Host.dll"))
            {
                await Utils.DownloadFile($"https://{CDN_URL}/coreclr-module/{Config.Branch}/{system}/AltV.Net.Host.dll",
                    $"{Config.OutputPath}/AltV.Net.Host.dll");
                
                ConsoleWrapper.WriteLine("AltV.Net.Host.dll is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine("AltV.Net.Host.dll is up to date and skipped", LogType.Info, true);

            if (await Utils.CalculateHash($"{Config.OutputPath}/AltV.Net.Host.runtimeconfig.json") != await Utils.GetUpdatedHash(
                    $"https://{CDN_URL}/coreclr-module/{Config.Branch}/{system}/update.json",
                    "AltV.Net.Host.runtimeconfig.json"))
            {
                await Utils.DownloadFile(
                    $"https://{CDN_URL}/coreclr-module/{Config.Branch}/{system}/AltV.Net.Host.runtimeconfig.json",
                    $"{Config.OutputPath}/AltV.Net.Host.runtimeconfig.json");
                
                ConsoleWrapper.WriteLine("AltV.Net.Host.runtimeconfig.json is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine("AltV.Net.Host.runtimeconfig.json is up to date and skipped", LogType.Info, true);

            if (await Utils.CalculateHash(
                    $"{Config.OutputPath}/{(Config.Windows ? "modules/csharp-module.dll" : "modules/libcsharp-module.so")}") !=
                await Utils.GetUpdatedHash(
                    $"https://{CDN_URL}/coreclr-module/{Config.Branch}/{system}/update.json",
                    $"{(Config.Windows ? "modules/csharp-module.dll" : "modules/libcsharp-module.so")}"))
            {
                await Utils.DownloadFile(
                    $"https://{CDN_URL}/coreclr-module/{Config.Branch}/{system}/modules/{(Config.Windows ? "csharp-module.dll" : "libcsharp-module.so")}",
                    $"{Config.OutputPath}/modules/{(Config.Windows ? "csharp-module.dll" : "libcsharp-module.so")}");
                
                ConsoleWrapper.WriteLine("csharp-module.dll is downloaded", LogType.Success, true);
            }
            else
                ConsoleWrapper.WriteLine(
                    $"{(Config.Windows ? "csharp-module.dll" : "libcsharp-module.so")} is up to date and skipped", LogType.Info, true);
            
            ConsoleWrapper.WriteLine("Finish csharp download", LogType.Success, true);
        }

        private static async Task DownloadJs(string system)
        {
            ConsoleWrapper.WriteLine("Starting js download", LogType.Info, true);

            if (await Utils.CalculateHash(
                    $"{Config.OutputPath}/{(Config.Windows ? "modules/js-module/js-module.dll" : "modules/js-module/libjs-module.so")}") !=
                await Utils.GetUpdatedHash(
                    $"https://{CDN_URL}/js-module/{Config.Branch}/{system}/update.json",
                    $"{(Config.Windows ? "modules/js-module/js-module.dll" : "modules/js-module/libjs-module.so")}"))
            {
                await Utils.DownloadFile(
                    $"https://{CDN_URL}/js-module/{Config.Branch}/{system}/modules/js-module/{(Config.Windows ? "js-module.dll" : "libjs-module.so")}",
                    $"{Config.OutputPath}/modules/js-module/{(Config.Windows ? "js-module.dll" : "libjs-module.so")}");
                
                ConsoleWrapper.WriteLine("js-module.dll is downloaded", LogType.Success, true);
            }
            else
                ConsoleWrapper.WriteLine(
                    $"{(Config.Windows ? "js-module.dll" : "libjs-module.so")} is up to date and skipped", LogType.Info, true);

            if (await Utils.CalculateHash(
                    $"{Config.OutputPath}/{(Config.Windows ? "modules/js-module/libnode.dll" : "modules/js-module/libnode.so.108")}") !=
                await Utils.GetUpdatedHash(
                    $"https://{CDN_URL}/js-module/{Config.Branch}/{system}/update.json",
                    $"{(Config.Windows ? "modules/js-module/libnode.dll" : "modules/js-module/libnode.so.108")}"))
            {
                await Utils.DownloadFile(
                    $"https://{CDN_URL}/js-module/{Config.Branch}/{system}/modules/js-module/{(Config.Windows ? "libnode.dll" : "libnode.so.108")}",
                    $"{Config.OutputPath}/modules/js-module/{(Config.Windows ? "libnode.dll" : "libnode.so.108")}");
                
                ConsoleWrapper.WriteLine("libnode.dll is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine($"{(Config.Windows ? "libnode.dll" : "libnode.so.108")} is up to date and skipped", LogType.Info, true);
            
            ConsoleWrapper.WriteLine("Finish js download", LogType.Success, true);
        }

        private static async Task DownloadJsByteCode(string system)
        {
            ConsoleWrapper.WriteLine("Starting js-bytecode download", LogType.Info, true);

            if (await Utils.CalculateHash(
                    $"{Config.OutputPath}/{(Config.Windows ? "modules/js-bytecode-module.dll" : "modules/libjs-bytecode-module.so")}") !=
                await Utils.GetUpdatedHash(
                    $"https://{CDN_URL}/js-bytecode-module/{Config.Branch}/{system}/update.json",
                    $"{(Config.Windows ? "modules/js-bytecode-module.dll" : "modules/libjs-bytecode-module.so")}"))
            {
                await Utils.DownloadFile(
                    $"https://{CDN_URL}/js-bytecode-module/{Config.Branch}/{system}/modules/{(Config.Windows ? "js-bytecode-module.dll" : "libjs-bytecode-module.so")}",
                    $"{Config.OutputPath}/modules/{(Config.Windows ? "js-bytecode-module.dll" : "libjs-bytecode-module.so")}");
                
                ConsoleWrapper.WriteLine("js-bytecode-module.dll is downloaded", LogType.Success, true);
            }
            else
                ConsoleWrapper.WriteLine(
                    $"{(Config.Windows ? "js-bytecode-module.dll" : "libjs-bytecode-module.so")} is up to date and skipped", LogType.Info, true);
            
            ConsoleWrapper.WriteLine("Finish js-bytecode download", LogType.Success, true);
        }
    }
}
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ImprovedConsole;

namespace altVPackage
{
    public class Program
    {
        public class Config
        {
            public string Branch { get; set; } = "release";
            public bool Windows { get; set; }
            public bool Server { get; set; }
            public bool Voice { get; set; }
            public bool CSharp { get; set; }
            public bool Js { get; set; }
            public bool JsByteCode { get; set; }
            public string OutputPath { get; set; } = "./";
        }

        private static readonly List<string> Branches = new() { "release", "rc", "dev" };
        private const string CDN_URL = "cdn.alt-mp.com";

        public static async Task Main(string[] args)
        {
            ConsoleWrapper.WriteLine("alt:V Package started", LogType.Success, true);
            var config = new Config();

            if (!File.Exists("./config.json"))
            {
                ConsoleWrapper.WriteLine("Config file not found", LogType.Error, true);
                ConsoleWrapper.WriteLine("Please enter branch name");

                var branch = Console.ReadLine()?.ToLower() ?? "release";
                config.Branch = !Branches.Contains(branch) ? Branches.First() : branch;

                ConsoleWrapper.WriteLine("Windows build? (y/n)");
                config.Windows = Console.ReadLine()?.ToLower() == "y";

                ConsoleWrapper.WriteLine("Do you want server build? (y/n)");
                config.Server = Console.ReadLine()?.ToLower() == "y";

                ConsoleWrapper.WriteLine("Do you want voice build? (y/n)");
                config.Voice = Console.ReadLine()?.ToLower() == "y";

                ConsoleWrapper.WriteLine("Do you want CSharp build? (y/n)");
                config.CSharp = Console.ReadLine()?.ToLower() == "y";

                ConsoleWrapper.WriteLine("Do you want JS build? (y/n)");
                config.Js = Console.ReadLine()?.ToLower() == "y";

                if (config.Branch == "release")
                {
                    ConsoleWrapper.WriteLine("Do you want JS bytecode build? (y/n)");
                    config.JsByteCode = Console.ReadLine()?.ToLower() == "y";
                }

                ConsoleWrapper.WriteLine("Input output path");
                config.OutputPath = Console.ReadLine() ?? "./";

                await File.WriteAllTextAsync("./config.json", JsonSerializer.Serialize(config));
                ConsoleWrapper.WriteLine("Config file created\n", LogType.Success, true);
            }
            else
            {
                config = JsonSerializer.Deserialize<Config>(await File.ReadAllTextAsync("./config.json"));
                if (config is null)
                {
                    ConsoleWrapper.WriteLine("Config file is invalid, remove it and run the program again\n", LogType.Error, true);
                    return;
                }
            }

            ConsoleWrapper.WriteLine("Begin packages download\n", LogType.Info, true);

            if (config.Server) Directory.CreateDirectory($@"{config.OutputPath}/data");
            if (config.CSharp) Directory.CreateDirectory($@"{config.OutputPath}/modules");
            if (config.Js) Directory.CreateDirectory($@"{config.OutputPath}/modules/js-module");

            var system = config.Windows ? "x64_win32" : "x64_linux";

            if (config.Server) await DownloadServer(config, system);

            if (config.Voice) await DownloadVoice(config, system);

            if (config.CSharp) await DownloadCsharp(config, system);

            if (config.Js) await DownloadJs(config, system);

            if (config is { JsByteCode: true, Branch: "release" }) await DownloadJsByteCode(config, system);

            ConsoleWrapper.WriteLine("Download finished\n", LogType.Success, true);
            Console.ReadKey();
        }

        private static async Task DownloadServer(Config config, string system)
        {
            ConsoleWrapper.WriteLine("Starting server download\n", LogType.Info, true);

            if (await CalculateHash($"{config.OutputPath}/{(config.Windows ? "altv-server.exe" : "altv-server")}") !=
                await GetUpdatedHash($"https://{CDN_URL}/server/{config.Branch}/{system}/update.json",
                    $"{(config.Windows ? "altv-server.exe" : "altv-server")}"))
            {
                await DownloadFile(
                    $"https://{CDN_URL}/server/{config.Branch}/{system}/{(config.Windows ? "altv-server.exe" : "altv-server")}",
                    $"{config.OutputPath}/{(config.Windows ? "altv-server.exe" : "altv-server")}");
                
                ConsoleWrapper.WriteLine("Server is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine("Server is up to date and skipped", LogType.Info, true);

            if (await CalculateHash($"{config.OutputPath}/data/vehmodels.bin") !=
                await GetUpdatedHash($"https://{CDN_URL}/data/{config.Branch}/update.json",
                    $"vehmodels.bin"))
            {
                await DownloadFile($"https://{CDN_URL}/data/{config.Branch}/data/vehmodels.bin",
                    $"{config.OutputPath}/data/vehmodels.bin");
                
                ConsoleWrapper.WriteLine("vehmodels.bin is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine("vehmodels.bin is up to date and skipped", LogType.Info, true);

            if (await CalculateHash($"{config.OutputPath}/data/vehmods.bin") !=
                await GetUpdatedHash($"https://{CDN_URL}/data/{config.Branch}/update.json",
                    $"vehmods.bin"))
            {
                await DownloadFile($"https://{CDN_URL}/data/{config.Branch}/data/vehmods.bin",
                    $"{config.OutputPath}/data/vehmods.bin");
                
                ConsoleWrapper.WriteLine("vehmods.bin is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine("vehmods.bin is up to date and skipped", LogType.Info, true);

            if (await CalculateHash($"{config.OutputPath}/data/clothes.bin") !=
                await GetUpdatedHash($"https://{CDN_URL}/data/{config.Branch}/update.json",
                    $"clothes.bin"))
            {
                await DownloadFile($"https://{CDN_URL}/data/{config.Branch}/data/clothes.bin",
                    $"{config.OutputPath}/data/clothes.bin");
                
                ConsoleWrapper.WriteLine("clothes.bin is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine("clothes.bin is up to date and skipped", LogType.Info, true);

            if (await CalculateHash($"{config.OutputPath}/data/pedmodels.bin") !=
                await GetUpdatedHash($"https://{CDN_URL}/data/{config.Branch}/update.json",
                    $"pedmodels.bin"))
            {
                await DownloadFile($"https://{CDN_URL}/data/{config.Branch}/data/pedmodels.bin",
                    $"{config.OutputPath}/data/pedmodels.bin");
                
                ConsoleWrapper.WriteLine("pedmodels.bin is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine("pedmodels.bin is up to date and skipped", LogType.Info, true);

            Console.WriteLine();
            ConsoleWrapper.WriteLine("Finish server download\n", LogType.Success, true);
        }

        private static async Task DownloadVoice(Config config, string system)
        {
            ConsoleWrapper.WriteLine("Starting voice server download\n", LogType.Info, true);

            if (await CalculateHash(
                    $"{config.OutputPath}/{(config.Windows ? "altv-voice-server.exe" : "altv-voice-server")}") !=
                await GetUpdatedHash($"https://{CDN_URL}/voice-server/{config.Branch}/{system}/update.json",
                    $"{(config.Windows ? "altv-voice-server.exe" : "altv-voice-server")}"))
            {
                await DownloadFile(
                    $"https://{CDN_URL}/voice-server/{config.Branch}/{system}/{(config.Windows ? "altv-voice-server.exe" : "altv-voice-server")}",
                    $"{config.OutputPath}/{(config.Windows ? "altv-voice-server.exe" : "altv-voice-server")}");
                
                ConsoleWrapper.WriteLine("altv-voice-server.exe is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine("Voice is up to date and skipped", LogType.Info, true);

            Console.WriteLine();
            ConsoleWrapper.WriteLine("Finish voice server download\n", LogType.Success, true);
        }

        private static async Task DownloadCsharp(Config config, string system)
        {
            ConsoleWrapper.WriteLine("Starting csharp download\n", LogType.Info, true);

            if (await CalculateHash($"{config.OutputPath}/AltV.Net.Host.dll") != await GetUpdatedHash(
                    $"https://{CDN_URL}/coreclr-module/{config.Branch}/{system}/update.json",
                    "AltV.Net.Host.dll"))
            {
                await DownloadFile($"https://{CDN_URL}/coreclr-module/{config.Branch}/{system}/AltV.Net.Host.dll",
                    $"{config.OutputPath}/AltV.Net.Host.dll");
                
                ConsoleWrapper.WriteLine("AltV.Net.Host.dll is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine("AltV.Net.Host.dll is up to date and skipped", LogType.Info, true);

            if (await CalculateHash($"{config.OutputPath}/AltV.Net.Host.runtimeconfig.json") != await GetUpdatedHash(
                    $"https://{CDN_URL}/coreclr-module/{config.Branch}/{system}/update.json",
                    "AltV.Net.Host.runtimeconfig.json"))
            {
                await DownloadFile(
                    $"https://{CDN_URL}/coreclr-module/{config.Branch}/{system}/AltV.Net.Host.runtimeconfig.json",
                    $"{config.OutputPath}/AltV.Net.Host.runtimeconfig.json");
                
                ConsoleWrapper.WriteLine("AltV.Net.Host.runtimeconfig.json is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine("AltV.Net.Host.runtimeconfig.json is up to date and skipped", LogType.Info, true);

            if (await CalculateHash(
                    $"{config.OutputPath}/{(config.Windows ? "modules/csharp-module.dll" : "modules/libcsharp-module.so")}") !=
                await GetUpdatedHash(
                    $"https://{CDN_URL}/coreclr-module/{config.Branch}/{system}/update.json",
                    $"{(config.Windows ? "modules/csharp-module.dll" : "modules/libcsharp-module.so")}"))
            {
                await DownloadFile(
                    $"https://{CDN_URL}/coreclr-module/{config.Branch}/{system}/modules/{(config.Windows ? "csharp-module.dll" : "libcsharp-module.so")}",
                    $"{config.OutputPath}/modules/{(config.Windows ? "csharp-module.dll" : "libcsharp-module.so")}");
                
                ConsoleWrapper.WriteLine("csharp-module.dll is downloaded", LogType.Success, true);
            }
            else
                ConsoleWrapper.WriteLine(
                    $"{(config.Windows ? "csharp-module.dll" : "libcsharp-module.so")} is up to date and skipped", LogType.Info, true);

            Console.WriteLine();
            ConsoleWrapper.WriteLine("Finish csharp download\n", LogType.Success, true);
        }

        private static async Task DownloadJs(Config config, string system)
        {
            ConsoleWrapper.WriteLine("Starting js download\n", LogType.Info, true);

            if (await CalculateHash(
                    $"{config.OutputPath}/{(config.Windows ? "modules/js-module/js-module.dll" : "modules/js-module/libjs-module.so")}") !=
                await GetUpdatedHash(
                    $"https://{CDN_URL}/js-module/{config.Branch}/{system}/update.json",
                    $"{(config.Windows ? "modules/js-module/js-module.dll" : "modules/js-module/libjs-module.so")}"))
            {
                await DownloadFile(
                    $"https://{CDN_URL}/js-module/{config.Branch}/{system}/modules/js-module/{(config.Windows ? "js-module.dll" : "libjs-module.so")}",
                    $"{config.OutputPath}/modules/js-module/{(config.Windows ? "js-module.dll" : "libjs-module.so")}");
                
                ConsoleWrapper.WriteLine("js-module.dll is downloaded", LogType.Success, true);
            }
            else
                ConsoleWrapper.WriteLine(
                    $"{(config.Windows ? "js-module.dll" : "libjs-module.so")} is up to date and skipped", LogType.Info, true);

            if (await CalculateHash(
                    $"{config.OutputPath}/{(config.Windows ? "modules/js-module/libnode.dll" : "modules/js-module/libnode.so.108")}") !=
                await GetUpdatedHash(
                    $"https://{CDN_URL}/js-module/{config.Branch}/{system}/update.json",
                    $"{(config.Windows ? "modules/js-module/libnode.dll" : "modules/js-module/libnode.so.108")}"))
            {
                await DownloadFile(
                    $"https://{CDN_URL}/js-module/{config.Branch}/{system}/modules/js-module/{(config.Windows ? "libnode.dll" : "libnode.so.108")}",
                    $"{config.OutputPath}/modules/js-module/{(config.Windows ? "libnode.dll" : "libnode.so.108")}");
                
                ConsoleWrapper.WriteLine("libnode.dll is downloaded", LogType.Success, true);
            }
            else ConsoleWrapper.WriteLine($"{(config.Windows ? "libnode.dll" : "libnode.so.108")} is up to date and skipped", LogType.Info, true);

            Console.WriteLine();
            ConsoleWrapper.WriteLine("Finish js download\n", LogType.Success, true);
        }

        private static async Task DownloadJsByteCode(Config config, string system)
        {
            ConsoleWrapper.WriteLine("Starting js-bytecode download\n", LogType.Info, true);

            if (await CalculateHash(
                    $"{config.OutputPath}/{(config.Windows ? "modules/js-bytecode-module.dll" : "modules/libjs-bytecode-module.so")}") !=
                await GetUpdatedHash(
                    $"https://{CDN_URL}/js-bytecode-module/{config.Branch}/{system}/update.json",
                    $"{(config.Windows ? "modules/js-bytecode-module.dll" : "modules/libjs-bytecode-module.so")}"))
            {
                await DownloadFile(
                    $"https://{CDN_URL}/js-bytecode-module/{config.Branch}/{system}/modules/{(config.Windows ? "js-bytecode-module.dll" : "libjs-bytecode-module.so")}",
                    $"{config.OutputPath}/modules/{(config.Windows ? "js-bytecode-module.dll" : "libjs-bytecode-module.so")}");
                
                ConsoleWrapper.WriteLine("js-bytecode-module.dll is downloaded", LogType.Success, true);
            }
            else
                ConsoleWrapper.WriteLine(
                    $"{(config.Windows ? "js-bytecode-module.dll" : "libjs-bytecode-module.so")} is up to date and skipped", LogType.Info, true);

            Console.WriteLine();
            ConsoleWrapper.WriteLine("Finish js-bytecode download\n", LogType.Success, true);
        }

        private static async Task DownloadFile(string url, string fileName)
        {
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);
            byte[] fileBytes;
            try
            {
                fileBytes = await httpClient.GetByteArrayAsync(url);
            }
            catch
            {
                ConsoleWrapper.WriteLine($"Connection timeout on {fileName}", LogType.Error, true);
                Console.ReadLine();
                return;
            }
            await File.WriteAllBytesAsync(fileName, fileBytes);
        }

        private static async Task<string> GetUpdatedHash(string url, string file)
        {
            var httpClient = new HttpClient();
            var fileBytes = await httpClient.GetByteArrayAsync(url);
            var json = Encoding.UTF8.GetString(fileBytes, 0, fileBytes.Length);
            return json.Substring(json.IndexOf($"{file}", StringComparison.Ordinal) + file.Length + 3, 40);
        }

        private static async Task<string> CalculateHash(string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;
            await using var fileStream = File.OpenRead($"{filePath}");
            {
                return Convert.ToHexString(await SHA1.Create().ComputeHashAsync(fileStream)).ToLower();
            }
        }
    }
}
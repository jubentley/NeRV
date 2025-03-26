/*
    NeRV, .Net Runtime Version
    
    Justin Bentley 2024-25 v1.2

    Will inform if .Net Core Runtime is current
    Will only check the .Net Major Number (X.0.0) that this program is compiled in
    Being designed for a low RAM Server, compiled in x86 is preferable
    String literals """ start support from .Net 7, can do .Net 5-6 if refactored (or deleted)

    Note: With Auto Updates configurable on the server (youtu.be/UcACNMGSByc) this program is 
    largely redundant, though can still be useful, from time to time (also, dont forget to like and
    subscribe to my youtube channel (above) im currently at 32 subscibers but I want to get to 100!)
*/

using System.Text.Json;

namespace NeRV
{
    public class ReleaseDetails
    {
        // Match to JSON
        public int id { get; set; }
        public string tag_name { get; set; }
        public string name { get; set; }
        public bool draft { get; set; }
        public bool prerelease { get; set; }
        public string body { get; set; }
        public DateTime created_at { get; set; }
        public DateTime published_at { get; set; }

        // Below not provided by API
        public string remoteVersion { get; set; }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {
            ReleaseDetails releaseDetails = await GetNetDetailsAPI(
                "https://api.github.com/repos/dotnet/runtime/releases",
                Environment.Version.Major
            );
                       // version that this program is compiled in
            string localVersion = Environment.Version.ToString(); 

            Console.Clear();

            string architecture = Environment.Is64BitProcess ? "x64" : "x86";

            Console.WriteLine($"NeRV (.Net Runtime Version) v1.2 {architecture} Justin Bentley 2025\n");

            Console.WriteLine(
                "~~ Latest GitHub Release ~~\n\n" +
                $"ID:          {releaseDetails.id}\n" +
                $"Tag Name:    {releaseDetails.tag_name}\n" +
                $"Name:        {releaseDetails.name}\n" +
                $"CompareVal:  {releaseDetails.remoteVersion}\n" +
                $"Draft?:      {releaseDetails.draft}\n" +
                $"Prerelease?: {releaseDetails.prerelease}\n" +
                $"Body:        {releaseDetails.body}\n" +
                $"Created:     {releaseDetails.created_at:yyyy-MM-ddTHH:mm:ssZ}\n" +
                $"Published:   {releaseDetails.published_at:yyyy-MM-ddTHH:mm:ssZ}\n" +
                $"TimeNow:     {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}\n\n" +
                "      ~~ Local ~~\n\n" +
                $"Current .NET Version: {localVersion}"
            );

            //compare remote .Net version (github) to local
            if (localVersion == releaseDetails.remoteVersion)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n       ** Match ** ");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n       ** Mismatch ** ");
                Console.ResetColor();
            }

            WaitForUserInput();
        }

        public static async Task<ReleaseDetails> GetNetDetailsAPI(string url, int netMajor)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "C# App");

                    string? response = await client.GetStringAsync(url); // throws HttpRequestException

                    if (string.IsNullOrWhiteSpace(response)) { 
                        throw new Exception("Empty response from API."); }                        

                    ReleaseDetails[] releases = JsonSerializer.Deserialize<ReleaseDetails[]>(response);

                    ReleaseDetails latestDotNetRelease = releases
                        .Where(
                            release => release.tag_name.StartsWith($"v{netMajor}.")
                            && !release.draft
                            && !release.prerelease
                        )
                        .OrderByDescending(
                            release => release.published_at
                        )
                        .Select(release => new ReleaseDetails
                        {
                            id = release.id,
                            tag_name = release.tag_name,
                            name = release.name,
                            draft = release.draft,
                            prerelease = release.prerelease,
                            body = release.body.Split('\n')[0],     // trim at newline | single line OK
                            created_at = release.created_at,
                            published_at = release.published_at,
                            remoteVersion = release.tag_name.Substring(1) // remove 'v' prefix
                        })
                        .FirstOrDefault()
                        ?? throw new Exception("No matching .NET release found.");
                    
                    return latestDotNetRelease;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.Write($"[NeRV] Network error: Could not reach API: {ex}");
                WaitForUserInput();
                throw new Exception("[NeRV] Network error: Could not reach API.", ex);
            }
            catch (Exception ex)
            {
                Console.Write($"[NeRV] Error: {ex}");
                WaitForUserInput();
                throw new Exception("[NeRV] Error: ", ex);
            }
        }

        public static bool TEST_MODE = false;
        private static void WaitForUserInput()
        {
            if (!TEST_MODE) { Console.ReadKey(true); }
        }
    }
}



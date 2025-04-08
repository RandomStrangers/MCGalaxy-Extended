/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
    
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    https://opensource.org/license/ecl-2-0/
    https://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
using System.IO;
using System.Net;
using MCGalaxy.Network;
using MCGalaxy.Tasks;

namespace MCGalaxy
{
    /// <summary> Checks for and applies software updates. </summary>
    public static class Updater 
    {    
        public static string SourceURL = "https://github.com/ClassiCube/MCGalaxy";
        public const string BaseURL    = "https://raw.githubusercontent.com/ClassiCube/MCGalaxy/master/";
        public const string UploadsURL = "https://github.com/ClassiCube/MCGalaxy/tree/master/Uploads";        
        const string CurrentVersionURL = BaseURL + "Uploads/current_version.txt";
        const string CHANGELOG_URL     = BaseURL + "Changelog.txt";
        
        const string CDN_URL  = "https://cdn.classicube.net/client/mcg/{0}/";
#if NET8_0
        const string CDN_BASE = CDN_URL + "net80/";
#elif NET6_0
        const string CDN_BASE = CDN_URL + "net60/";
#elif NET_20
        const string CDN_BASE = CDN_URL + "net20/";
#else
        const string CDN_BASE = CDN_URL + "net40/";
#endif

#if MCG_STANDALONE
        static string DLL_URL = CDN_URL  + IOperatingSystem.DetectOS().StandaloneName;
#elif TESTING_BLOCKS
        const string DLL_URL = "https://github.com/RandomStrangers/" +
            "MCGalaxy-Extended/raw/master/Uploads/MCGalaxy_Extended.dll";
#elif TEN_BIT_BLOCKS
        const string DLL_URL  = CDN_BASE + "MCGalaxy_infid.dll";
#else
        const string DLL_URL  = CDN_BASE + "MCGalaxy_.dll";
#endif
        const string GUI_URL  = CDN_BASE + "MCGalaxy.exe";
        const string CLI_URL  = CDN_BASE + "MCGalaxyCLI.exe";

        public static event EventHandler NewerVersionDetected;
        
        public static void UpdaterTask(SchedulerTask task) {
            UpdateCheck();
            task.Delay = TimeSpan.FromHours(2);
        }

        static void UpdateCheck() {
            if (!Server.Config.CheckForUpdates) return;

            try {
                if (!NeedsUpdating()) {
                    Logger.Log(LogType.SystemActivity, "No update found!");
                } else if (NewerVersionDetected != null) {
                    NewerVersionDetected(null, EventArgs.Empty);
                }
            } catch (Exception ex) {
                Logger.LogError("Error checking for updates", ex);
            }
        }
        
        public static bool NeedsUpdating() {
            using (WebClient client = HttpUtil.CreateWebClient()) {
                string latest = client.DownloadString(CurrentVersionURL);
                return new Version(latest) > new Version(Server.Version);
            }
        }
        

        // Backwards compatibility
        public static void PerformUpdate() { PerformUpdate(true); }
        
        public static void PerformUpdate(bool release) {
            try {
                try {
                    DeleteFiles("Changelog.txt", "MCGalaxy_.update", "MCGalaxy.update", "MCGalaxyCLI.update",
                                "prev_MCGalaxy_.dll", "prev_MCGalaxy.exe", "prev_MCGalaxyCLI.exe");
                } catch {
                }
        		
                string mode = release ? "release" : "latest";
                WebClient client = HttpUtil.CreateWebClient();
#if TESTING_BLOCKS
                Logger.Log(LogType.SystemActivity, "Downloading extended update files");
                DownloadFile(client, DLL_URL, "MCGalaxy_.update");
#else
                Logger.Log(LogType.SystemActivity, "Downloading {0} update files", mode);               
                DownloadFile(client, DLL_URL.Replace("{0}", mode), "MCGalaxy_.update");
#endif
#if MCG_STANDALONE
                // Self contained executable, no separate CLI or GUI to download
#elif MCG_DOTNET
                DownloadFile(client, CLI_URL.Replace("{0}", mode), "MCGalaxyCLI.update");
#else
                DownloadFile(client, GUI_URL.Replace("{0}", mode), "MCGalaxy.update");
                DownloadFile(client, CLI_URL.Replace("{0}", mode), "MCGalaxyCLI.update");
#endif
                DownloadFile(client, CHANGELOG_URL, "Changelog.txt");

                Server.SaveAllLevels();
                Player[] players = PlayerInfo.Online.Items;
                foreach (Player pl in players) pl.SaveStats();
                
                string serverDLL = Server.GetServerDLLPath();
                string serverGUI = "MCGalaxy.exe";
#if !MCG_DOTNET
                string serverCLI = "MCGalaxyCLI.exe";
#else
                string serverCLI = Server.GetServerExePath();
#endif
                
                // Move current files to previous files (by moving instead of copying, 
                //  can overwrite original the files without breaking the server)
                AtomicIO.TryMove(serverDLL, "prev_MCGalaxy_.dll");
                AtomicIO.TryMove(serverGUI, "prev_MCGalaxy.exe");
                AtomicIO.TryMove(serverCLI, "prev_MCGalaxyCLI.exe");

                // Move update files to current files
                AtomicIO.TryMove("MCGalaxy_.update",   serverDLL);
                AtomicIO.TryMove("MCGalaxy.update",    serverGUI);
                AtomicIO.TryMove("MCGalaxyCLI.update", serverCLI);                             

                Server.Stop(true, "Updating server.");
            } catch (Exception ex) {
                Logger.LogError("Error performing update", ex);
            }
        }
        
        static void DownloadFile(WebClient client, string url, string dst) {
            Logger.Log(LogType.SystemActivity, "Downloading {0} to {1}", 
                       url, Path.GetFileName(dst));
            client.DownloadFile(url, dst);
        }
        
        static void DeleteFiles(params string[] paths) {
            foreach (string path in paths) { AtomicIO.TryDelete(path); }
        }
    }
}

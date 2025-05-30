﻿/*
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
using System.Collections.Generic;
using System.Net;
using MCGalaxy.DB;
using MCGalaxy.Generator;
using MCGalaxy.Network;
using MCGalaxy.Tasks;

namespace MCGalaxy 
{
    public sealed partial class Server 
    {
        static void LoadMainLevel(SchedulerTask task) {
            try {
                mainLevel = LevelActions.Load(Player.Console, Config.MainLevel, false);
                if (mainLevel == null) GenerateMain();
            } catch (Exception ex) {
                Logger.LogError("Error loading main level", ex);
            }
        }
        
        static void GenerateMain() {
            Logger.Log(LogType.SystemActivity, "main level not found, generating..");
            mainLevel = new Level(Config.MainLevel, 128, 64, 128);
            
            MapGen.Find("Flat").Generate(Player.Console, mainLevel, "");
            mainLevel.Save();
            Level.LoadMetadata(mainLevel);
            LevelInfo.Add(mainLevel);
        }

        static void LoadAllPlugins(SchedulerTask task) { Plugin.LoadAll(); }
        
        static void InitPlayerLists(SchedulerTask task) {
            try {
                UpgradeTasks.UpgradeOldAgreed();
            } catch (Exception ex) {
                Logger.LogError("Error upgrading agreed list", ex);
            }
            
            LoadPlayerLists();
            ModerationTasks.QueueTasks();
        }
        
        internal static void LoadPlayerLists() {
            agreed = PlayerList.Load("ranks/agreed.txt");
            invalidIds = PlayerList.Load("extra/invalidids.txt");
            Player.Console.DatabaseID = NameConverter.InvalidNameID("(console)");
            
            hidden   = PlayerList.Load("ranks/hidden.txt");
            vip      = PlayerList.Load("text/vip.txt");
            noEmotes = PlayerList.Load("text/emotelist.txt");
            lockdown = PlayerList.Load("text/lockdown.txt");
            
            models = PlayerExtList.Load("extra/models.txt");
            skins  = PlayerExtList.Load("extra/skins.txt");
            reach  = PlayerExtList.Load("extra/reach.txt");
            rotations   = PlayerExtList.Load("extra/rotations.txt");
            modelScales = PlayerExtList.Load("extra/modelscales.txt");

            bannedIP  = PlayerExtList.Load("ranks/banned-ip.txt");
            muted     = PlayerExtList.Load("ranks/muted.txt");
            frozen    = PlayerExtList.Load("ranks/frozen.txt");
            tempRanks = PlayerExtList.Load(Paths.TempRanksFile);
            tempBans  = PlayerExtList.Load(Paths.TempBansFile);
            whiteList = PlayerList.Load("ranks/whitelist.txt");
        }
        
        static void LoadAutoloadMaps(SchedulerTask task) {
            AutoloadMaps = PlayerExtList.Load("text/autoload.txt", '=');
            List<string> maps = AutoloadMaps.AllNames();
            
            foreach (string map in maps) {
                if (map.CaselessEq(Config.MainLevel)) continue;
                LevelActions.Load(Player.Console, map, false);
            }
        }
        
        static void SetupSocket(SchedulerTask task) {        
            IPAddress ip;
            
            if (!IPAddress.TryParse(Config.ListenIP, out ip)) {
                Logger.Log(LogType.Warning, "Unable to parse listen IP config key, listening on any IP");
                ip = IPAddress.Any;
            }            
            Listener.Listen(ip, Config.Port);
        }
        
        static void InitHeartbeat(SchedulerTask task) {
            Heartbeat.Start();
        }
        
        static void InitTimers(SchedulerTask task) {
            MainScheduler.QueueRepeat(RandomMessage, null,
                                      Config.AnnouncementInterval);
            Critical.QueueRepeat(ServerTasks.UpdateEntityPositions, null,
                                 TimeSpan.FromMilliseconds(Config.PositionUpdateInterval));
        }
        
        static void InitRest(SchedulerTask task) {
            MainScheduler.QueueRepeat(BlockQueue.Loop, null, 
                                      TimeSpan.FromMilliseconds(BlockQueue.Interval));
            Critical.QueueRepeat(ServerTasks.TickPlayers, null,
                                 TimeSpan.FromMilliseconds(20));

            Logger.Log(LogType.SystemActivity, "Finished setting up server");
            SetupFinished = true;
        }
    }
}
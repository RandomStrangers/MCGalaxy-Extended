/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCGalaxy)
    
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
namespace MCGalaxy.Commands {
    public sealed class CmdAfk : Command {
        public override string name { get { return "afk"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return CommandTypes.Information; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }
        public static string keywords { get { return ""; } }
        public CmdAfk() { }

        public override void Use(Player p, string message) {
        	if (Player.IsSuper(p)) { MessageInGameOnly(p); return; }
            if (message == "list") {
                Player[] players = PlayerInfo.Online.Items;
                foreach (Player pl in players) {
                    if (!Entities.CanSee(p, pl) || !pl.IsAfk) continue;
                    Player.Message(p, p.name);
                }
                return;
            }
            ToggleAfk(p, message);
        }
        
        internal static void ToggleAfk(Player p, string message) {
            if (p.joker) message = "";
            p.AutoAfk = false;
            p.IsAfk = !p.IsAfk;
            p.afkMessage = p.IsAfk ? message : null;
            TabList.Update(p, true);
            p.LastAction = DateTime.UtcNow;

            bool send = !Server.chatmod && !p.muted;
            if (p.IsAfk) {
                if (send) {
                    Chat.MessageWhere("-{0}%S- is AFK {1}", 
            		                  pl => Entities.CanSee(pl, p), p.ColoredName, message);
                    Player.RaisePlayerAction(p, PlayerAction.AFK, message);
                } else {
                    Player.Message(p, "You are now marked as being AFK.");
                }
                
            	p.RaiseONAFK();
            	Player.RaiseAFK(p);
                OnPlayerAFKEvent.Call(p);
            } else {
                if (send) {
            		Chat.MessageWhere("-{0}%S- is no longer AFK", 
            		                  pl => Entities.CanSee(pl, p), p.ColoredName);
                    Player.RaisePlayerAction(p, PlayerAction.UnAFK, message);
                } else {
                    Player.Message(p, "You are no longer marked as being AFK.");
                }
            }
            p.CheckForMessageSpam();
        }
        
        public override void Help(Player p) {
            Player.Message(p, "%T/afk <reason>");
            Player.Message(p, "%HMarks yourself as AFK. Use again to mark yourself as back");
        }
    }
}

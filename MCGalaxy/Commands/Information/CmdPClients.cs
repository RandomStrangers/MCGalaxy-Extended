﻿/*
    Copyright 2015-2024 MCGalaxy
        
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
using System.Collections.Generic;
using System.Text;

namespace MCGalaxy.Commands.Info
{
    public sealed class CmdPClients : Command2 
    {
        public override string name { get { return "PClients"; } }
        public override string shortcut { get { return "Clients"; } }
        public override string type { get { return CommandTypes.Information; } }
        public override bool UseableWhenFrozen { get { return true; } }
        
        public override void Use(Player p, string message, CommandData data) {
            Dictionary<string, List<Player>> clients = new Dictionary<string, List<Player>>();
            Player[] online = PlayerInfo.Online.Items;
            
            foreach (Player pl in online) 
            {
                if (!p.CanSee(pl, data.Rank)) continue;
                string appName = pl.Session.ClientName();
                    
               List<Player> usingClient;
               if (!clients.TryGetValue(appName, out usingClient)) {
                    usingClient = new List<Player>();
                    clients[appName] = usingClient;
                }
                usingClient.Add(pl);
            }
            
            List<string> lines = new List<string>();
            lines.Add("Players using:");
            foreach (var kvp in clients) 
            {
                StringBuilder builder = new StringBuilder();
                List<Player> players  = kvp.Value;
                
                for (int i = 0; i < players.Count; i++) 
                {
                    string nick = Colors.StripUsed(p.FormatNick(players[i]));
                    builder.Append(nick);
                    if (i < players.Count - 1) builder.Append(", ");
                }
                lines.Add(string.Format("  {0}: &f{1}", kvp.Key, builder.ToString()));
            }
            //lines.Add(string.Format("Displayed {0} unique client name{1}.", clients.Count, clients.Count == 1 ? "" : "s"));
            p.MessageLines(lines);
        }

        public override void Help(Player p) {
            p.Message("&T/PClients");
            p.Message("&HLists the clients players are using, and who uses which client.");
        }
    }
}

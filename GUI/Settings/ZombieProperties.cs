﻿/*
    Copyright 2015 MCGalaxy
        
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
using System.Collections.Generic;
using System.ComponentModel;
using MCGalaxy.Games;

namespace MCGalaxy.Gui {
    public sealed class ZombieProperties {
        
        [Description("Comma separated list of levels that are never chosen for zombie survival. (e.g. main,spawn)")]
        [Category("Levels settings")]
        [DisplayName("Ignored level list")]
        public string IgnoredLevelsList { get; set; }
        
        [Description("Comma separated list of levels to use for zombie survival. (e.g. map1,map2,map3) " +
                     "If this is left blank, then all levels are used.")]
        [Category("Levels settings")]
        [DisplayName("Levels list")]
        public string LevelsList { get; set; }
        
        
        [Description("Whether players are allowed to pillar in zombie survival. " +
                     "Note this can be overriden for specific maps using /mset.")]
        [Category("General settings")]
        [DisplayName("Pillaring allowed")]
        public bool Pillaring { get; set; }
        
        [Description("Whether the main/spawn level is always set to the current level of zombie survival. " +
                     "You should set this to true if the server is purely for zombie survival. ")]
        [Category("General settings")]
        [DisplayName("Set main level")]
        public bool SetMainLevel { get; set; }
        
        [Description("Whether zombie survival should start when the server starts.")]
        [Category("General settings")]
        [DisplayName("Start immediately")]
        public bool StartImmediately { get; set; }    
        
        
        [Description("Max distance players are allowed to move between packets (for speedhack detection). " +
                     "32 units equals one block.")]
        [Category("Other settings")]
        [DisplayName("Max move distance")]
        public int MaxMoveDistance { get; set; }
        
        [Description("Distance between players before they are considered to have 'collided'. (for infecting). " +
                     "32 units equals one block.")]
        [Category("Other settings")]
        [DisplayName("Hitbox precision")]
        public int HitboxPrecision { get; set; }
        
        [Description("Whether the current map's name is included when a hearbeat is sent. " +
                     "This means it shows up in the server list as: \"Server name (current map name)\"")]
        [Category("Other settings")]
        [DisplayName("Include map in heartbeat")]
        public bool IncludeMapInHeartbeat { get; set; }
        

        [Description("Name to show above infected players. If this is left blank, then the player's name is used instead.")]
        [Category("Zombie settings")]
        [DisplayName("Name")]
        public string Name { get; set; }

        [Description("Model to use for infected players. If this is left blank, then 'zombie' model is used.")]
        [Category("Zombie settings")]
        [DisplayName("Model")]
        public string Model { get; set; }
        

        [Description("How many seconds an invisibility potion bought using /buy invisibility lasts.")]
        [Category("Human settings")]
        [DisplayName("Invisibility duration")]
        public int InvisibilityDuration { get; set; }
        
        [Description("Maximum number of invisibility potions a human is allowed to buy in a round.")]
        [Category("Human settings")]
        [DisplayName("Invisibility potions")]        
        public int InvisibilityPotions { get; set; }
        
        [Description("How many seconds an invisibility potion bought using /buy zinvisibility lasts.")]
        [Category("Zombie settings")]
        [DisplayName("Invisibility duration")]
        public int ZInvisibilityDuration { get; set; }
        
        [Description("Maximum number of invisibility potions a zombie is allowed to buy in a round.")]
        [Category("Zombie settings")]
        [DisplayName("Invisibility potions")]        
        public int ZInvisibilityPotions { get; set; }
        
        
        [Description("The percentage chance that a revive potion will actually disinfect a zombie.")]
        [Category("Revive settings")]
        [DisplayName("Chance")]
        public int Chance { get; set; }  
        
        [Description("The minimum number of seconds left in a round, below which /buy revive will not work.")]
        [Category("Revive settings")]
        [DisplayName("Insufficient time")]
        public int InsufficientTime { get; set; }
        
        [Description("Message shown when using /buy revive and the seconds left in a round is less than 'InsufficientTime'.")]
        [Category("Revive settings")]
        [DisplayName("Insufficient time message")]
        public string InsufficientTimeMessage { get; set; }
        
        [Description("The maximum number of seconds after a human is infected, after which /buy revive will not work.")]
        [Category("Revive settings")]
        [DisplayName("Expiry time")]
        public int ExpiryTime { get; set; }
        
        [Description("Message shown when a player uses /buy revive, and it actually disinfects them.")]
        [Category("Revive settings")]
        [DisplayName("Success message")]
        public string SuccessMessage { get; set; }
        
        [Description("Message shown when a player uses /buy revive, but does not disinfect them.")]
        [Category("Revive settings")]
        [DisplayName("Failure message")]
        public string FailureMessage { get; set; }
        
        public void LoadFromServer() {
            ZSConfig cfg = ZSGame.Config;
            IgnoredLevelsList = cfg.IgnoredLevelList.Join(",");
            LevelsList = cfg.LevelList.Join(",");
            
            Pillaring = !cfg.NoPillaring;
            SetMainLevel = cfg.SetMainLevel;
            StartImmediately = cfg.StartImmediately;
            
            MaxMoveDistance = cfg.MaxMoveDist;
            HitboxPrecision = cfg.HitboxDist;
            IncludeMapInHeartbeat = cfg.IncludeMapInHeartbeat;
            
            Name = cfg.ZombieName;
            Model = cfg.ZombieModel;
            InvisibilityDuration = cfg.InvisibilityDuration;
            InvisibilityPotions = cfg.InvisibilityPotions;
            ZInvisibilityDuration = cfg.ZombieInvisibilityDuration;
            ZInvisibilityPotions = cfg.ZombieInvisibilityPotions;
            
            Chance = cfg.ReviveChance;
            InsufficientTime = cfg.ReviveNoTime;
            InsufficientTimeMessage = cfg.ReviveNoTimeMessage;
            ExpiryTime = cfg.ReviveTooSlow;
            FailureMessage = cfg.ReviveFailureMessage;
            SuccessMessage = cfg.ReviveSuccessMessage;
        }
        
        public void ApplyToServer() {
            ZSConfig cfg = ZSGame.Config;
            string list = IgnoredLevelsList.Replace(" ", "");
            if (list.Length == 0) cfg.IgnoredLevelList = new List<string>();
            else cfg.IgnoredLevelList = new List<string>(list.Replace(" ", "").Split(','));
                
            list = LevelsList.Replace(" ", "");
            if (list.Length == 0) cfg.LevelList = new List<string>();
            else cfg.LevelList = new List<string>(list.Replace(" ", "").Split(','));
            
            cfg.NoPillaring = !Pillaring;
            cfg.SetMainLevel = SetMainLevel;
            cfg.StartImmediately = StartImmediately;
            
            cfg.MaxMoveDist = MaxMoveDistance;
            cfg.HitboxDist = HitboxPrecision;
            cfg.IncludeMapInHeartbeat = IncludeMapInHeartbeat;
            
            cfg.ZombieName = Name.Trim();
            cfg.ZombieModel = Model.Trim();
            if (cfg.ZombieModel.Length == 0)
                cfg.ZombieModel = "zombie";
            cfg.InvisibilityDuration = InvisibilityDuration;
            cfg.InvisibilityPotions = InvisibilityPotions;
            cfg.ZombieInvisibilityDuration = ZInvisibilityDuration;
            cfg.ZombieInvisibilityPotions = ZInvisibilityPotions; 
            
            cfg.ReviveChance = Chance;
            cfg.ReviveNoTime = InsufficientTime;
            cfg.ReviveNoTimeMessage = InsufficientTimeMessage;
            cfg.ReviveTooSlow = ExpiryTime;
            cfg.ReviveFailureMessage = FailureMessage;
            cfg.ReviveSuccessMessage = SuccessMessage;
        }
    }
}

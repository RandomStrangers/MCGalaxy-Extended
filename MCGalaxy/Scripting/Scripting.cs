﻿/*
    Copyright 2010 MCLawl Team - Written by Valek (Modified by MCGalaxy)

    Edited for use with MCGalaxy
 
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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MCGalaxy.Scripting 
{    
    /// <summary> Utility methods for loading assemblies, commands, and plugins </summary>
    public static class IScripting 
    {     
        public const string AutoloadFile = "text/cmdautoload.txt";
        public const string DllDir = "extra/commands/dll/";
        
        /// <summary> Returns the default .dll path for the custom command with the given name </summary>
        public static string CommandPath(string name) { return DllDir + "Cmd" + name + ".dll"; }
        /// <summary> Returns the default .dll path for the plugin with the given name </summary>
        public static string PluginPath(string name)  { return "plugins/" + name + ".dll"; }
        
        /// <summary> Constructs instances of all types which derive from T in the given assembly. </summary>
        /// <returns> The list of constructed instances. </returns>
        public static List<T> LoadTypes<T>(Assembly lib) {
            List<T> instances = new List<T>();
            
            foreach (Type t in lib.GetTypes()) 
            {
                if (t.IsAbstract || t.IsInterface || !t.IsSubclassOf(typeof(T))) continue;
                object instance = Activator.CreateInstance(t);
                
                if (instance == null) {
                    Logger.Log(LogType.Warning, "{0} \"{1}\" could not be loaded", typeof(T).Name, t.Name);
                    throw new BadImageFormatException();
                }
                instances.Add((T)instance);
            }
            return instances;
        }
        
        static byte[] GetDebugData(string path) {
            if (Server.RunningOnMono()) {
                // Cmdtest.dll -> Cmdtest.dll.mdb
                path += ".mdb";
            } else {
                // Cmdtest.dll -> Cmdtest.pdb
                path = Path.ChangeExtension(path, ".pdb");
            }
            
            if (!File.Exists(path)) return null;
            try {
                return File.ReadAllBytes(path);
            } catch (Exception ex) {
                Logger.LogError("Error loading .pdb " + path, ex);
                return null;
            }
        }
        
        /// <summary> Loads the given assembly from disc (and associated .pdb debug data) </summary>
        public static Assembly LoadAssembly(string path) {
            byte[] data  = File.ReadAllBytes(path);
            byte[] debug = GetDebugData(path);
            return Assembly.Load(data, debug);
        }
        
        
        public static void AutoloadCommands() {
            if (!File.Exists(AutoloadFile)) { File.Create(AutoloadFile); return; }
            string[] list = File.ReadAllLines(AutoloadFile);
            
            foreach (string cmdName in list) 
            {
                if (cmdName.IsCommentLine()) continue;
                string path  = CommandPath(cmdName);
                string error;
                List<Command> cmds = LoadCommands(path, out error);
                
                if (error != null) { 
                    Logger.Log(LogType.Warning, error);
                } else {
                    Logger.Log(LogType.SystemActivity, "AUTOLOAD: Loaded {0} from Cmd{1}.dll", 
                               cmds.Join(c => "/" + c.name), cmdName);
                }
            }
        }
        
        /// <summary> Loads and registers all the commands from the given .dll path </summary>
        /// <param name="error"> If an error occurs, set to a string describing the error </param>
        /// <returns> The list of commands loaded </returns>
        public static List<Command> LoadCommands(string path, out string error) {
            error = null;
            try {
                Assembly lib = LoadAssembly(path);
                List<Command> commands = LoadTypes<Command>(lib);
                if (commands.Count == 0) error = "&WNo commands in " + path;
                
                foreach (Command cmd in commands) 
                {
                    if (Command.Find(cmd.name) != null) {
                        error = "/" + cmd.name + " is already loaded";
                        return null;
                    }
                    
                    Command.Register(cmd);
                }
                return commands;
            } catch (Exception ex) {
                error = DescribeLoadError(path, ex);
                return null;
            }
        }
        
        static string DescribeLoadError(string path, Exception ex) {
            if (ex is FileNotFoundException)
                return "File &9" + path + " &Snot found.";
            
            Logger.LogError("Error loading commands from " + path, ex);
            string file = Path.GetFileName(path);
            
            if (ex is BadImageFormatException) {
                return "&W" + file + " is not a valid assembly, or has an invalid dependency. Details in the error log.";
            } else if (ex is FileLoadException) {
                return "&W" + file + " or one of its dependencies could not be loaded. Details in the error log.";
            }
            return "&WAn unknown error occured. Details in the error log.";
        }
        
        
        public static void AutoloadPlugins() {
            string[] files = AtomicIO.TryGetFiles("plugins", "*.dll");
            
            if (files != null) {
                foreach (string path in files) { LoadPlugin(path, true); }
            } else {
                Directory.CreateDirectory("plugins");
            }
        }
        
        /// <summary> Loads all plugins from the given .dll path. </summary>
        public static bool LoadPlugin(string path, bool auto) {
            try {
                Assembly lib = LoadAssembly(path);
                List<Plugin> plugins = IScripting.LoadTypes<Plugin>(lib);
                
                foreach (Plugin plugin in plugins) {
                    if (!Plugin.Load(plugin, auto)) return false;
                }
                return true;
            } catch (Exception ex) {
                Logger.LogError("Error loading plugins from " + path, ex);
                return false;
            }
        }
    }
}

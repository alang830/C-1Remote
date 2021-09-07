﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorPickerWPF.Code;
using Newtonsoft.Json;
using PRM.Core.Protocol;
using PRM.Core.Protocol.Putty.SSH;
using PRM.Core.Protocol.Runner;
using PRM.Core.Protocol.Runner.Default;

namespace PRM.Core.Service
{
    public class ProtocolConfigurationService
    {
        public Dictionary<string, ProtocolConfig> ProtocolConfigs { get; set; } = new Dictionary<string, ProtocolConfig>();

        public readonly string ProtocolFolderName;
        public ProtocolConfigurationService()
        {
            var appDateFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigurationService.AppName);
            ProtocolFolderName = Path.Combine(appDateFolder, "Protocols");
            if (Directory.Exists(ProtocolFolderName) == false)
                Directory.CreateDirectory(ProtocolFolderName);
            if (Directory.Exists(Path.Combine(ProtocolFolderName, "SSH")) == false)
                Directory.CreateDirectory(Path.Combine(ProtocolFolderName, "SSH"));
            if (Directory.Exists(Path.Combine(ProtocolFolderName, "RDP")) == false)
                Directory.CreateDirectory(Path.Combine(ProtocolFolderName, "RDP"));
            if (Directory.Exists(Path.Combine(ProtocolFolderName, "VNC")) == false)
                Directory.CreateDirectory(Path.Combine(ProtocolFolderName, "VNC"));
            if (Directory.Exists(Path.Combine(ProtocolFolderName, "TELNET")) == false)
                Directory.CreateDirectory(Path.Combine(ProtocolFolderName, "TELNET"));
            if (Directory.Exists(Path.Combine(ProtocolFolderName, "FTP")) == false)
                Directory.CreateDirectory(Path.Combine(ProtocolFolderName, "FTP"));
            if (Directory.Exists(Path.Combine(ProtocolFolderName, "SFTP")) == false)
                Directory.CreateDirectory(Path.Combine(ProtocolFolderName, "SFTP"));

            Load();
        }


        public void Load()
        {   
            //// reflect all the child class
            //lock (this)
            //{
            //    if (_baseList.Count == 0)
            //    {
            //        var assembly = typeof(ProtocolServerBase).Assembly;
            //        var types = assembly.GetTypes();
            //        _baseList = types.Where(x => x.IsSubclassOf(typeof(ProtocolRunner)) && !x.IsAbstract)
            //            .Select(type => (ProtocolRunner)Activator.CreateInstance(type)).ToList();
            //    }
            //}
            ProtocolConfigs.Clear();



            var di = new DirectoryInfo(ProtocolFolderName);
            LoadSSH();
            foreach (var directoryInfo in di.GetDirectories())
            {
                var protocolName = directoryInfo.Name;
                if(ProtocolConfigs.ContainsKey(protocolName))
                    continue;

                var cfgPath = Path.Combine(directoryInfo.FullName, $"{protocolName}.json");
                if (File.Exists(cfgPath))
                {
                    var c = JsonConvert.DeserializeObject<ProtocolConfig>(File.ReadAllText(cfgPath, Encoding.UTF8));
                    if (c != null)
                        ProtocolConfigs.Add(protocolName, c);
                }
            }
        }

        public ProtocolConfig LoadConfig(string protocolName)
        {
            protocolName = protocolName.ToUpper();
            if (Directory.Exists(Path.Combine(ProtocolFolderName, protocolName)) == false)
                Directory.CreateDirectory(Path.Combine(ProtocolFolderName, protocolName));
            var file = Path.Combine(ProtocolFolderName, protocolName, $"{protocolName}.json");
            if (File.Exists(file))
            {
                var c = JsonConvert.DeserializeObject<ProtocolConfig>(File.ReadAllText(file, Encoding.UTF8));
                if (c != null)
                    return c;
            }
            return null;
        }

        private void LoadSSH()
        {
            var protocolName = ProtocolServerSSH.ProtocolName;
            var c = LoadConfig(protocolName);

            if (c == null)
            {
                // if there is no config file for ssh, then init the ssh with the default runner.
                c = new ProtocolConfig(protocolName);
                c.Runners.Add(new SshDefaultRunner());
                var file = Path.Combine(ProtocolFolderName, protocolName, $"{protocolName}.json");
                File.WriteAllText(file, JsonConvert.SerializeObject(c, Formatting.Indented), Encoding.UTF8);
            }

            if (c.Runners.First() is SshDefaultRunner == false)
            {
                var d = c.Runners.First();
                var dd = d as SshDefaultRunner;
                c.Runners.Add(dd);
            }

            ProtocolConfigs.Add(protocolName, c);
        }
    }
}
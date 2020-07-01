using System;
using IniParser;
using IniParser.Model;
using System.Diagnostics;
using System.Windows;
using System.IO;

namespace ResumeDatabase.lib
{
    class configObj
    {
        private FileIniDataParser parser = new FileIniDataParser();
        private IniData cfg;
        public string file { set; get; }
        public string dbServer { get; set; }
        public string dbDatabase { get; set; }
        public string dbUser { get; set; }
        public string dbPasswd { get; set; }
        public string dbLogDate { get; set; }
        public string resumeFolderPath { get; set; }

        public configObj()
        {
                file = "config.ini";
            if (!File.Exists(file))
            {
                File.WriteAllText(file,"");
            }
            try
            {
                cfg = parser.ReadFile(file);
                dbServer = cfg["database"]["dbServer"];
                dbDatabase = cfg["database"]["dbDatabase"];
                dbUser = cfg["database"]["dbUser"];
                dbPasswd = cfg["database"]["dbPasswd"];
                dbLogDate = cfg["database"]["dbLogDate"];
                resumeFolderPath = cfg["other"]["resumeFolderPath"];
            }
            catch (Exception err) { }

        }
        public void save()
        {
            cfg["database"]["dbServer"] = dbServer;
            cfg["database"]["dbDatabase"] = dbDatabase;
            cfg["database"]["dbUser"] = dbUser;
            cfg["database"]["dbPasswd"] = dbPasswd;
            cfg["database"]["dbLogDate"] = dbLogDate;
            cfg["other"]["resumeFolderPath"] = resumeFolderPath;
            parser.WriteFile(file, cfg);
        }
    }
}

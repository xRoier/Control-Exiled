using System;
using System.IO;
using System.Net;
using Exiled.API.Features;
using Exiled.Loader;

namespace Control
{
    public class AutoUpdater
    {
        private readonly Plugin _plugin;
        public AutoUpdater(Plugin plugin) => this._plugin = plugin;
        
        public void CheckUpdates()
        {
            try
            {
                using (var client = new WebClient())
                {
                    string result = client.DownloadString("https://control.jesus-qc.es/api/version");
                    if (new Version(result) > _plugin.Version)
                        Update();
                }
            }
            catch (Exception)
            {
                Log.Warn("There was an issue checking for updates.");
            }
        }

        public void Update()
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile("https://github.com/Control-Plugin/Control-Exiled/releases/latest/download/Control-Exiled.dll", _plugin.GetPath());
                }
            }
            catch (Exception)
            {
                Log.Warn("There was an issue updating the plugin.");
            }
        }
    }
}
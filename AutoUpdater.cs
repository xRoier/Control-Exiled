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
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://control.jesus-qc.es/api/version");
                httpWebRequest.Method = "GET";
            
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                
                    if(new Version(result) > _plugin.Version)
                        Update();
                }
            }
            catch (Exception)
            {
                Log.Warn("There was an issue connecting to Control, the site may be under maintenance.");
            }
            
        }

        public void Update()
        {
            using (var client = new WebClient())
            {
                client.DownloadFile("https://github.com/Jesus-QC/Control-Exiled/releases/latest/download/Control.dll", _plugin.GetPath()); 
            }
        }
    }
}
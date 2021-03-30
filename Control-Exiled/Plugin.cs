﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Loader;
using MEC;
using WebSocketSharp;

namespace Control
{
    public class Plugin : Plugin<Config>
    {
        public override string Name { get; } = "Control";
        public override string Prefix { get; } = "Control";
        public override string Author { get; } = "Jesus-QC";
        public override Version Version { get; } = new Version(0, 0, 1, 1);
        public override Version RequiredExiledVersion { get; } = new Version(2, 8, 0);
        public override PluginPriority Priority { get; } = PluginPriority.Lower;

        public Dictionary<string, string> webhookList = new Dictionary<string, string>();
        public Dictionary<string, string> translations = new Dictionary<string, string>();

        public string directorypath = Path.Combine(Paths.Plugins, "control");
        public string translationpath = Path.Combine(Paths.Plugins, "control", "translations.yml");

        public int maxPlayers;
        public bool IsServerKeyValid;

        public AutoUpdater updater;

        private static List<CoroutineHandle> _coroutines = new List<CoroutineHandle>();
        private static WebSocket ws;
        public override void OnEnabled()
        {
            updater = new AutoUpdater(this);
            updater.CheckUpdates();

            if (!Directory.Exists(directorypath))
            {
                Directory.CreateDirectory(directorypath);
                File.WriteAllText(translationpath, Config.yamlTranslation);
            }
            else if (!File.Exists(translationpath))
            {
                File.WriteAllText(translationpath, Config.yamlTranslation);
            }

            translations = ConfigManager.Deserializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(translationpath));

            Exiled.Events.Handlers.Map.Decontaminating += OnDecontaminating;
            Exiled.Events.Handlers.Map.GeneratorActivated += OnGeneratorActivated;
            Exiled.Events.Handlers.Warhead.Detonated += OnDetonated;
            Exiled.Events.Handlers.Warhead.Starting += OnStarting;
            Exiled.Events.Handlers.Warhead.Stopping += OnStopping;

            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += OnSendingRemoteAdminCmd;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaiting;
            Exiled.Events.Handlers.Server.SendingConsoleCommand += OnSendingConsoleCommand;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawningTeam;
            Exiled.Events.Handlers.Server.ReportingCheater += OnReportingCheater;
            Exiled.Events.Handlers.Server.LocalReporting += OnLocalReporting;

            Exiled.Events.Handlers.Scp914.ChangingKnobSetting += OnChangingKnobSetting;
            Exiled.Events.Handlers.Player.MedicalItemUsed += OnMedicalItemUsed;
            Exiled.Events.Handlers.Scp079.InteractingTesla += OnInteractingTesla;
            Exiled.Events.Handlers.Player.PickingUpItem += OnPickingUpItem;
            Exiled.Events.Handlers.Player.InsertingGeneratorTablet += OnInsertingTable;
            Exiled.Events.Handlers.Player.EjectingGeneratorTablet += OnEjectingTable;
            Exiled.Events.Handlers.Player.UnlockingGenerator += OnUnlockingGenerator;
            Exiled.Events.Handlers.Player.OpeningGenerator += OnOpeningGenerator;
            Exiled.Events.Handlers.Player.ClosingGenerator += OnClosingGenerator;
            Exiled.Events.Handlers.Scp079.GainingLevel += OnGainingLevel;
            Exiled.Events.Handlers.Scp079.GainingExperience += OnGainingExperience;
            Exiled.Events.Handlers.Player.EscapingPocketDimension += OnEscapingPocket;
            Exiled.Events.Handlers.Player.EnteringPocketDimension += OnEnteringPocket;
            Exiled.Events.Handlers.Scp106.CreatingPortal += OnCreatingPortal;
            Exiled.Events.Handlers.Player.ActivatingWarheadPanel += OnActivatingWarheadPanel;
            Exiled.Events.Handlers.Player.TriggeringTesla += OnTriggeringTesla;
            Exiled.Events.Handlers.Player.ThrowingGrenade += OnThrowingGrenade;
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            Exiled.Events.Handlers.Player.Dying += OnDying;
            Exiled.Events.Handlers.Player.Kicked += OnKicked;
            Exiled.Events.Handlers.Player.Banned += OnBanned;
            Exiled.Events.Handlers.Player.InteractingDoor += OnInteractingDoor;
            Exiled.Events.Handlers.Player.InteractingElevator += OnInteractingElevator;
            Exiled.Events.Handlers.Player.InteractingLocker += OnInteractingLocker;
            Exiled.Events.Handlers.Player.IntercomSpeaking += OnIntercomSpeaking;
            Exiled.Events.Handlers.Player.Handcuffing += OnHandcuffing;
            Exiled.Events.Handlers.Player.RemovingHandcuffs += OnRemovingHandcuffs;
            Exiled.Events.Handlers.Scp106.Teleporting += OnTeleporting;
            Exiled.Events.Handlers.Player.ReloadingWeapon += OnReloadingWeapon;
            Exiled.Events.Handlers.Player.ItemDropped += OnItemDropped;
            Exiled.Events.Handlers.Player.Verified += OnVerified;
            Exiled.Events.Handlers.Player.Destroying += OnDestroying;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            Exiled.Events.Handlers.Player.ChangingGroup += OnChangingGroup;
            Exiled.Events.Handlers.Player.ChangingItem += OnChangingItem;
            Exiled.Events.Handlers.Scp914.Activating += OnActivating;
            Exiled.Events.Handlers.Scp106.Containing += OnContaining;
            Exiled.Events.Handlers.Server.RestartingRound += OnRestarting;

            InitializeControl();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Map.Decontaminating -= OnDecontaminating;
            Exiled.Events.Handlers.Map.GeneratorActivated -= OnGeneratorActivated;
            Exiled.Events.Handlers.Warhead.Detonated -= OnDetonated;
            Exiled.Events.Handlers.Warhead.Starting -= OnStarting;
            Exiled.Events.Handlers.Warhead.Stopping -= OnStopping;

            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= OnSendingRemoteAdminCmd;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaiting;
            Exiled.Events.Handlers.Server.SendingConsoleCommand -= OnSendingConsoleCommand;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawningTeam;
            Exiled.Events.Handlers.Server.ReportingCheater -= OnReportingCheater;
            Exiled.Events.Handlers.Server.LocalReporting -= OnLocalReporting;

            Exiled.Events.Handlers.Scp914.ChangingKnobSetting -= OnChangingKnobSetting;
            Exiled.Events.Handlers.Player.MedicalItemUsed -= OnMedicalItemUsed;
            Exiled.Events.Handlers.Scp079.InteractingTesla -= OnInteractingTesla;
            Exiled.Events.Handlers.Player.PickingUpItem -= OnPickingUpItem;
            Exiled.Events.Handlers.Player.InsertingGeneratorTablet -= OnInsertingTable;
            Exiled.Events.Handlers.Player.EjectingGeneratorTablet -= OnEjectingTable;
            Exiled.Events.Handlers.Player.UnlockingGenerator -= OnUnlockingGenerator;
            Exiled.Events.Handlers.Player.OpeningGenerator -= OnOpeningGenerator;
            Exiled.Events.Handlers.Player.ClosingGenerator -= OnClosingGenerator;
            Exiled.Events.Handlers.Scp079.GainingLevel -= OnGainingLevel;
            Exiled.Events.Handlers.Scp079.GainingExperience -= OnGainingExperience;
            Exiled.Events.Handlers.Player.EscapingPocketDimension -= OnEscapingPocket;
            Exiled.Events.Handlers.Player.EnteringPocketDimension -= OnEnteringPocket;
            Exiled.Events.Handlers.Scp106.CreatingPortal -= OnCreatingPortal;
            Exiled.Events.Handlers.Player.ActivatingWarheadPanel -= OnActivatingWarheadPanel;
            Exiled.Events.Handlers.Player.TriggeringTesla -= OnTriggeringTesla;
            Exiled.Events.Handlers.Player.ThrowingGrenade -= OnThrowingGrenade;
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            Exiled.Events.Handlers.Player.Dying -= OnDying;
            Exiled.Events.Handlers.Player.Kicked -= OnKicked;
            Exiled.Events.Handlers.Player.Banned -= OnBanned;
            Exiled.Events.Handlers.Player.InteractingDoor -= OnInteractingDoor;
            Exiled.Events.Handlers.Player.InteractingElevator -= OnInteractingElevator;
            Exiled.Events.Handlers.Player.InteractingLocker -= OnInteractingLocker;
            Exiled.Events.Handlers.Player.IntercomSpeaking -= OnIntercomSpeaking;
            Exiled.Events.Handlers.Player.Handcuffing -= OnHandcuffing;
            Exiled.Events.Handlers.Player.RemovingHandcuffs -= OnRemovingHandcuffs;
            Exiled.Events.Handlers.Scp106.Teleporting -= OnTeleporting;
            Exiled.Events.Handlers.Player.ReloadingWeapon -= OnReloadingWeapon;
            Exiled.Events.Handlers.Player.ItemDropped -= OnItemDropped;
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            Exiled.Events.Handlers.Player.Destroying -= OnDestroying;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            Exiled.Events.Handlers.Player.ChangingGroup -= OnChangingGroup;
            Exiled.Events.Handlers.Player.ChangingItem -= OnChangingItem;
            Exiled.Events.Handlers.Scp914.Activating -= OnActivating;
            Exiled.Events.Handlers.Scp106.Containing -= OnContaining;
            Exiled.Events.Handlers.Server.RestartingRound -= OnRestarting;

            ws?.Close();
            
            base.OnDisabled();
        }

        private void OnRestarting()
        {
            maxPlayers = 0;

            ws?.Send("QUICKINFO 0/0/0");

            if (!IsServerKeyValid)
                InitializeControl();
        }

        private void OnDecontaminating(DecontaminatingEventArgs ev)
        {
            CreateMessage("Decontaminating");
        }
        private void OnGeneratorActivated(GeneratorActivatedEventArgs ev)
        {
            CreateMessage("GeneratorActivated");
        }
        private void OnDetonated()
        {
            CreateMessage("Detonated");
        }
        private void OnStarting(StartingEventArgs ev)
        {
            CreateMessage("Starting", "(someone)", ev.Player.Nickname);
        }
        private void OnStopping(StoppingEventArgs ev)
        {
            CreateMessage("Stopping", "(someone)", ev.Player.Nickname);
        }
        private void OnSendingRemoteAdminCmd(SendingRemoteAdminCommandEventArgs ev)
        {
            CreateMessage("SendingRemoteAdminCommand", "(someone)", ev.Sender.Nickname, "(command)", ev.Name);
        }
        private void OnSendingConsoleCommand(SendingConsoleCommandEventArgs ev)
        {
            CreateMessage("SendingConsoleCommand", "(someone)", ev.Player.Nickname, "(command)", ev.Name);
        }
        private void OnWaiting()
        {
            CreateMessage("WaitingForPlayers");
        }
        private void OnRoundStarted()
        {
            CreateMessage("RoundStarted");
        }
        private void OnRoundEnded(RoundEndedEventArgs ev)
        {
            CreateMessage("RoundEnded");
        }
        private void OnRespawningTeam(RespawningTeamEventArgs ev)
        {
            CreateMessage("RespawningTeam", "(team)", ev.NextKnownTeam.ToString());
        }
        private void OnReportingCheater(ReportingCheaterEventArgs ev)
        {
            CreateMessage("ReportingCheater", "(someone)", ev.Reporter.Nickname, "(somebody)", ev.Reported.Nickname);
        }
        private void OnLocalReporting(LocalReportingEventArgs ev)
        {
            CreateMessage("LocalReporting", "(someone)", ev.Issuer.Nickname, "(somebody)", ev.Target.Nickname);
        }
        private void OnChangingKnobSetting(ChangingKnobSettingEventArgs ev)
        {
            CreateMessage("ChangingKnobSetting", "(someone)", ev.Player.Nickname, "(newsetting)", ev.KnobSetting.ToString());
        }
        private void OnMedicalItemUsed(UsedMedicalItemEventArgs ev)
        {
            CreateMessage("MedicalItemUsed", "(someone)", ev.Player.Nickname, "(item)", ev.Item.ToString());
        }
        private void OnInteractingTesla(InteractingTeslaEventArgs ev)
        {
            CreateMessage("InteractingTesla", "(someone)", ev.Player.Nickname);
        }
        private void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            CreateMessage("PickingUpItem", "(someone)", ev.Player.Nickname, "(item)", ev.Pickup.itemId.ToString());
        }
        private void OnInsertingTable(InsertingGeneratorTabletEventArgs ev)
        {
            CreateMessage("InsertingGeneratorTablet", "(someone)", ev.Player.Nickname);
        }
        private void OnEjectingTable(EjectingGeneratorTabletEventArgs ev)
        {
            CreateMessage("EjectingGeneratorTablet", "(someone)", ev.Player.Nickname);
        }
        private void OnUnlockingGenerator(UnlockingGeneratorEventArgs ev)
        {
            CreateMessage("UnlockingGenerator", "(someone)", ev.Player.Nickname);
        }
        private void OnOpeningGenerator(OpeningGeneratorEventArgs ev)
        {
            CreateMessage("OpeningGenerator", "(someone)", ev.Player.Nickname);
        }
        private void OnClosingGenerator(ClosingGeneratorEventArgs ev)
        {
            CreateMessage("ClosingGenerator", "(someone)", ev.Player.Nickname);
        }
        private void OnGainingLevel(GainingLevelEventArgs ev)
        {
            CreateMessage("GainingLevel", "(someone)", ev.Player.Nickname);
        }
        private void OnGainingExperience(GainingExperienceEventArgs ev)
        {
            CreateMessage("GainingExperience", "(someone)", ev.Player.Nickname);
        }
        private void OnEscapingPocket(EscapingPocketDimensionEventArgs ev)
        {
            CreateMessage("EscapingPocketDimension", "(someone)", ev.Player.Nickname);
        }
        private void OnEnteringPocket(EnteringPocketDimensionEventArgs ev)
        {
            CreateMessage("EnteringPocketDimension", "(someone)", ev.Player.Nickname);
        }
        private void OnCreatingPortal(CreatingPortalEventArgs ev)
        {
            CreateMessage("CreatingPortal", "(someone)", ev.Player.Nickname);
        }
        private void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            CreateMessage("ActivatingWarheadPanel", "(someone)", ev.Player.Nickname);
        }
        private void OnTriggeringTesla(TriggeringTeslaEventArgs ev)
        {
            CreateMessage("TriggeringTesla", "(someone)", ev.Player.Nickname);
        }
        private void OnThrowingGrenade(ThrowingGrenadeEventArgs ev)
        {
            CreateMessage("ThrowingGrenade", "(someone)", ev.Player.Nickname);
        }
        private void OnHurting(HurtingEventArgs ev)
        {
            CreateMessage("Hurting", "(someone)", ev.Target.Nickname, "(somebody)", ev.Attacker.Nickname);
        }
        private void OnDying(DyingEventArgs ev)
        {
            CreateMessage("Dying", "(someone)", ev.Target.Nickname, "(somebody)", ev.Killer.Nickname);
        }
        private void OnKicked(KickedEventArgs ev)
        {
            CreateMessage("Kicked", "(someone)", ev.Target.Nickname);
        }
        private void OnBanned(BannedEventArgs ev)
        {
            CreateMessage("Banned", "(someone)", ev.Target.Nickname);
        }
        private void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            CreateMessage("InteractingDoor", "(someone)", ev.Player.Nickname);
        }
        private void OnInteractingElevator(InteractingElevatorEventArgs ev)
        {
            CreateMessage("InteractingElevator", "(someone)", ev.Player.Nickname);
        }
        private void OnInteractingLocker(InteractingLockerEventArgs ev)
        {
            CreateMessage("InteractingLocker", "(someone)", ev.Player.Nickname);
        }
        private void OnIntercomSpeaking(IntercomSpeakingEventArgs ev)
        {
            CreateMessage("IntercomSpeaking", "(someone)", ev.Player.Nickname);
        }
        private void OnHandcuffing(HandcuffingEventArgs ev)
        {
            CreateMessage("Handcuffing", "(someone)", ev.Cuffer.Nickname, "(somebody)", ev.Target.Nickname);
        }
        private void OnRemovingHandcuffs(RemovingHandcuffsEventArgs ev)
        {
            CreateMessage("RemovingHandcuffs", "(someone)", ev.Target.Nickname);
        }
        private void OnTeleporting(TeleportingEventArgs ev)
        {
            CreateMessage("Teleporting", "(someone)", ev.Player.Nickname);
        }
        private void OnReloadingWeapon(ReloadingWeaponEventArgs ev)
        {
            CreateMessage("ReloadingWeapon", "(someone)", ev.Player.Nickname);
        }
        private void OnItemDropped(ItemDroppedEventArgs ev)
        {
            CreateMessage("ItemDropped", "(someone)", ev.Player.Nickname);
        }
        private void OnVerified(VerifiedEventArgs ev)
        {
            if (maxPlayers < Player.List.Count())
            {
                maxPlayers = Player.List.Count();
                ws?.Send($"MAXPLY {maxPlayers}");
            }
            ws?.Send($"ADDPLY {ev.Player.UserId}");
            ws?.Send(GetQuickInfo());
            CreateMessage("Verified", "(someone)", ev.Player.Nickname);
        }
        private void OnDestroying(DestroyingEventArgs ev)
        {
            CreateMessage("Left", "(someone)", ev.Player.Nickname);
            Timing.CallDelayed(1.0f, () => ws?.Send(GetQuickInfo()));
        }
        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            CreateMessage("ChangingRole", "(someone)", ev.Player.Nickname, "(somerole)", ev.NewRole.ToString());
        }
        private void OnChangingGroup(ChangingGroupEventArgs ev)
        {
            CreateMessage("ChangingGroup", "(someone)", ev.Player.Nickname, "(somegroup)", ev.NewGroup.BadgeText);
        }
        private void OnChangingItem(ChangingItemEventArgs ev)
        {
            CreateMessage("ChangingItem", "(someone)", ev.Player.Nickname, "(something)", ev.NewItem.id.ToString());
        }
        private void OnActivating(ActivatingEventArgs ev)
        {
            CreateMessage("Activating", "(someone)", ev.Player.Nickname);
        }
        private void OnContaining(ContainingEventArgs ev)
        {
            CreateMessage("Containing", "(someone)", ev.Player.Nickname);
        }


        // Discord message
        public class Message
        {
            public string username { get; set; }
            public string avatar_url { get; set; }
            public string content { get; set; }
            public bool tts { get; set; }
            public DiscordMessageEmbed[] embeds { get; set; }
        }
        public class DiscordMessageEmbed
        {
            public int? color { get; set; }
            public DiscordMessageEmbedAuthor author { get; set; }
            public string title { get; set; }
            public string url { get; set; }
            public string description { get; set; }
            public DiscordMessageEmbedImage image { get; set; }
            public DiscordMessageEmbedFooter footer { get; set; }
        }
        public class DiscordMessageEmbedAuthor
        {
            public string name { get; set; }
            public string url { get; set; }
            public string icon_url { get; set; }
        }
        public class DiscordMessageEmbedImage
        {
            public string url { get; set; }
        }
        public class DiscordMessageEmbedFooter
        {
            public string text { get; set; }
            public string icon_url { get; set; }
        }

        public void InitializeControl()
        {
            try
            {
                RefreshWebhooks(Config.SecretKey);

                if (IsServerKeyValid)
                {
                    ws = new WebSocket("ws://controlws.jesus-qc.es:888/Websocket");

                    ws.OnMessage += OnWebsocketMessage;
                    ws.OnError += (sender, eventArgs) =>
                    {
                        ws?.Close();
                    };
                    ws.OnClose += (e, sender) =>
                    {
                        IsServerKeyValid = false;
                        Log.Warn("Disconnected from websocket | Trying to reconnect on next round restart");
                    };

                    ws.Connect();
                    ws.Send("CONNECT " + Config.SecretKey);
                }
            }
            catch (Exception)
            {
                Log.Error("An error occurred trying to connect to Control");
            }
        }

        public void RefreshWebhooks(string secretkey, bool devmode = false)
        {
            var httpWebRequest = !devmode ? (HttpWebRequest)WebRequest.Create("https://control.jesus-qc.es/api/servers?serverkey=" + secretkey) : (HttpWebRequest)WebRequest.Create("https://localhost:5001/api/servers?serverkey=" + secretkey);
            httpWebRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                if (result.Contains("error"))
                {
                    Log.Warn("Invalid secret key, webhooks disabled");
                    RefreshWebhooks("default");
                    IsServerKeyValid = false;
                    return;
                }

                IsServerKeyValid = true;
                webhookList = Utf8Json.JsonSerializer.Deserialize<Dictionary<string, string>>(result);
            }
        }

        public void CreateMessage(string Event, string replaced1 = "", string replace1 = "", string replaced2 = "", string replace2 = "")
        {
            if (!IsServerKeyValid)
            {
                Log.Info($"Invalid server key");
                return;
            }

            if (webhookList[Event] == "https://discord.com/api/webhooks/samplewebhookurl")
            {
                Log.Info($"The webhook for the event {Event} isn't configured.");
                return;
            }

            string desc = translations[Event];
            if (!string.IsNullOrEmpty(replaced1))
            {
                desc = desc.Replace(replaced1, $"**{replace1}**");
                if (!string.IsNullOrEmpty(replaced2))
                {
                    desc = desc.Replace(replaced2, $"**{replace2}**");
                }
            }

            var msg = new Message
            {
                username = Config.Username,
                avatar_url = Config.AvatarUrl,
                content = " ",
                tts = false,
                embeds = new DiscordMessageEmbed[]
                {
                    new DiscordMessageEmbed()
                    {
                        author = new DiscordMessageEmbedAuthor()
                        {
                            name = "Control",
                            icon_url = "https://imgur.com/ZvHGf6D.png",
                            url = "https://control.jesus-qc.es",
                        },
                        color = int.Parse("#FFFFFF".Replace("#", ""), System.Globalization.NumberStyles.HexNumber),
                        description = desc,
                        footer = new DiscordMessageEmbedFooter()
                        {
                            icon_url = "",
                            text = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"),
                        },
                        image = new DiscordMessageEmbedImage()
                        {
                            url = Config.EmbedImage,
                        },
                        title = Event,
                    }
                }
            };

            SendMessage(msg, webhookList[Event]);
        }

        public void SendMessage(Message message, string url)
        {
            WebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.ContentType = "application/json";
            webRequest.Method = "POST";

            using (var sendWebhook = new StreamWriter(webRequest.GetRequestStream()))
            {
                var webhook = System.Text.Encoding.UTF8.GetString(Utf8Json.JsonSerializer.Serialize(message));
                sendWebhook.Write(webhook);
            }

            var response = (HttpWebResponse)webRequest.GetResponse();
        }

        public string GetQuickInfo()
        {
            return $"QUICKINFO {Player.List.Count()}/{Player.List.Count(x => Config.staffGroups.Contains(x.GroupName))}/{Player.List.Count(x => Config.adminGroups.Contains(x.GroupName))}";
        }

        public void OnWebsocketMessage(object sender, MessageEventArgs e)
        {
            var message = e.Data;
            Console.WriteLine(message);

            if (message == "QUICKINFO")
            {
                ws.Send(GetQuickInfo());
            }
            else if (message.Contains("UPDATE"))
            {
                if (message.Contains("WEB"))
                {
                    ws.Close();
                    Log.Info("Control - Web update received");
                    Timing.CallDelayed(30.0f, () => InitializeControl());
                }
                else
                {
                    ws.Close();
                    Log.Info("Control - Update received");
                    updater.CheckUpdates();
                }
            }
        }
    }
}
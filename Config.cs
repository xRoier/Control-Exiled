using System.ComponentModel;
using Exiled.API.Interfaces;

namespace Control {
public sealed class Config : IConfig {
  public bool IsEnabled {
    get;
    set;
  }
  = true;
  public bool AreDebugLogsEnabled {
    get;
    set;
  }
  = false;
  public bool IsTheAutoupdaterEnabled {
    get;
    set;
  }
  = false;

  [Description("Get your server key in the website (Keep this secret!)")]
  public string SecretKey {
    get;
    set;
  }
  = "0000000000000000000000000000000";

  [Description("Dashboard Settings")]
  public string[] staffGroups {
    get;
    set;
  }
  = {"moderator", "admin", "owner"};
  public string[] adminGroups {
    get;
    set;
  }
  = {"admin", "owner"};

  [Description("Webhook configs")]
  public string Username {
    get;
    set;
  }
  = "Control Logs";
  public string AvatarUrl {
    get;
    set;
  }
  = "https://imgur.com/ZvHGf6D.png";

    public string yamlTranslation = @"Decontaminating: Light zone is in decontamination.
GeneratorActivated: A generator was activated!
Detonated: The warhead was detonated!
Starting: (someone) started the warhead!
Stopping: (someone) stopped the warhead!
SendingRemoteAdminCommand: (someone) ran the command (command)
WaitingForPlayers: The server is waiting for players!
SendingConsoleCommand: (someone) ran the console command (command)
RoundStarted: Round has started!
RoundEnded: Round as ended!
RespawningTeam: (team) have respawned!
ReportingCheater: (someone) reported (somebody) for cheating!
LocalReporting: (someone) reported (somebody)!
ChangingKnobSetting: (someone) has change the 914 Knob Settings to (newsetting)
MedicalItemUsed: (someone) has used a (item)
InteractingTesla: (someone) interacted with a tesla!
PickingUpItem: (someone) picked up a (item)
InsertingGeneratorTablet: (someone) inserted a tablet in a generator!
EjectingGeneratorTablet: (someone) ejected a tablet of a generator!
UnlockingGenerator: (someone) unlocked a generator!
OpeningGenerator: (someone) opened a generator!
ClosingGenerator: (someone) closed a generator!
GainingLevel: (someone) gained level as SCP-079
GainingExperience: (someone) gained experience as SCP-079
EscapingPocketDimension: (someone) escaped from the pocket dimension!
EnteringPocketDimension: (someone) entered the pocket dimension!
CreatingPortal: (someone) created a portal!
ActivatingWarheadPanel: (someone) activated the warhead panel!
TriggeringTesla: (someone) triggered a tesla!
ThrowingGrenade: (someone) threw a grenade!
Hurting: (someone) was hurt by (something)
Dying: (someone) was killed by (somebody).
Kicked: (someone) was kicked.
Banned: (someone) was banned.
InteractingDoor: (someone) interacted with a door
InteractingElevator: (someone) interacted with an elevator
InteractingLocker: (someone) interacted with a locker
IntercomSpeaking: (someone) is speaking in the intercom!
Handcuffing: (someone) handcuffed (somebody)
RemovingHandcuffs: (someone) was uncuffed.
Teleporting: (someone) was teleported!
ReloadingWeapon: (someone) reloaded his weapon!
ItemDropped: (someone) dropped an item!
Verified: (someone) joined the server!
Left: (someone) left the server!
ChangingRole: (someone) changed of role to (somerole)
ChangingGroup: (someone) changed of group to (somegroup)
ChangingItem: (someone) changed of item to (something)
Activating: (someone) activated the 914
Containing: (someone) was contained as SCP-106"
                                    ;
}
}

using BepInEx;
using BepInEx.Logging;
using SPT.Killfeed.Patches;
using SPT.Killfeed.Utilities;

namespace SPT.Killfeed;

internal static class PluginMetadata
{
    public const string Guid = "com.jvsup.killfeed";
    public const string Name = "SPT Killfeed";
    public const string Version = "4.1.0";
    public const string SptDependencyGuid = "com.SPT.custom";
    public const string SptDependencyVersion = "4.1.5";
}

[BepInPlugin(PluginMetadata.Guid, PluginMetadata.Name, PluginMetadata.Version)]
[BepInDependency(PluginMetadata.SptDependencyGuid, PluginMetadata.SptDependencyVersion)]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; }

    public void Awake()
    {
        Logger ??= BepInEx.Logging.Logger.CreateLogSource("SPT.Killfeed");
        Settings.Init(Config);
        PatchManager.EnablePatches();
    }
}

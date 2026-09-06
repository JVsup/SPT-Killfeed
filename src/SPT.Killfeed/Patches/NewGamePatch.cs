using System.Reflection;
using Comfort.Common;
using EFT;
using SPT.Killfeed.Features;
using SPT.Reflection.Patching;

namespace SPT.Killfeed.Patches;

internal class NewGamePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
    }

    [PatchPrefix]
    private static void Prefix()
    {
        var gameWorld = Singleton<GameWorld>.Instance;
        if (gameWorld == null)
        {
            return;
        }

        gameWorld.gameObject.AddComponent<KillfeedController>();
    }
}


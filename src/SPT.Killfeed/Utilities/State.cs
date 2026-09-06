using Comfort.Common;
using EFT;

namespace SPT.Killfeed.Utilities;

public static class State
{
    public static GameWorld World => Singleton<GameWorld>.Instance;

    public static Player LocalPlayer => World?.MainPlayer;
}


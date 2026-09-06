using System.IO;
using System.Reflection;
using EFT;
using UnityEngine;

namespace SPT.Killfeed.Utilities;

public static class TextureBank
{
    private static string _baseUiDirectory;
    private static Texture2D _usec;
    private static Texture2D _bear;
    private static Texture2D _scav;
    private static Texture2D _unknown;

    private static void EnsureIcons()
    {
        if (_baseUiDirectory != null)
        {
            return;
        }

        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        _baseUiDirectory = Path.Combine(assemblyDirectory, "UI");
    }

    private static Texture2D LoadPng(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Bilinear
            };
            return texture.LoadImage(bytes) ? texture : null;
        }
        catch
        {
            return null;
        }
    }

    public static Texture2D Faction(EPlayerSide side)
    {
        EnsureIcons();

        switch (side)
        {
            case EPlayerSide.Usec:
                return _usec ??= LoadPng(Path.Combine(_baseUiDirectory, "Faction_USEC.png"));
            case EPlayerSide.Bear:
                return _bear ??= LoadPng(Path.Combine(_baseUiDirectory, "Faction_BEAR.png"));
            case EPlayerSide.Savage:
                return _scav ??= LoadPng(Path.Combine(_baseUiDirectory, "Faction_Scav.png"));
            default:
                return _unknown ??= LoadPng(Path.Combine(_baseUiDirectory, "Faction_Unknown.png"));
        }
    }
}


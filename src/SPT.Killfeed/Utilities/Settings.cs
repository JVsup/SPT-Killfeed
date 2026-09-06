using BepInEx.Configuration;
using UnityEngine;

namespace SPT.Killfeed.Utilities;

public enum KillfeedAnchor
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    BottomCenter
}

public static class Settings
{
    public static ConfigEntry<bool> KillfeedEnabled { get; private set; }
    public static ConfigEntry<bool> KillfeedShowOtherKills { get; private set; }
    public static ConfigEntry<KillfeedAnchor> KillfeedCorner { get; private set; }
    public static ConfigEntry<Vector2> KillfeedMargin { get; private set; }
    public static ConfigEntry<int> KillfeedFontSize { get; private set; }
    public static ConfigEntry<float> KillfeedLineSeconds { get; private set; }
    public static ConfigEntry<float> KillfeedLineHeight { get; private set; }
    public static ConfigEntry<Color> KillfeedColor { get; private set; }
    public static ConfigEntry<string> KillfeedTemplate { get; private set; }
    public static ConfigEntry<bool> KillfeedShowFactionIcon { get; private set; }
    public static ConfigEntry<Color> KillfeedFactionTint { get; private set; }
    public static ConfigEntry<int> KillfeedMaxLines { get; private set; }

    public static Font Font { get; private set; }

    public static void Init(ConfigFile config)
    {
        var order = 1000;

        KillfeedEnabled = config.Bind(
            "Killfeed",
            "Show Killfeed",
            true,
            new ConfigDescription(
                "If true, display kills in a corner of the screen.",
                null,
                new ConfigurationManagerAttributes { Order = order-- }));

        KillfeedShowOtherKills = config.Bind(
            "Killfeed",
            "Show kills by others",
            false,
            new ConfigDescription(
                "If true, show kills made by other players and bots.",
                null,
                new ConfigurationManagerAttributes { Order = order-- }));

        KillfeedMaxLines = config.Bind(
            "Killfeed",
            "Max list Lines",
            10,
            new ConfigDescription(
                "Maximum number of killfeed lines to show at once.",
                null,
                new ConfigurationManagerAttributes { Order = order-- }));

        KillfeedCorner = config.Bind(
            "Killfeed",
            "Screen Corner",
            KillfeedAnchor.TopLeft,
            new ConfigDescription(
                "Where the killfeed appears along the screen edges.",
                null,
                new ConfigurationManagerAttributes { Order = order-- }));

        KillfeedMargin = config.Bind(
            "Killfeed",
            "Screen Margin",
            new Vector2(24f, 24f),
            new ConfigDescription(
                "X is horizontal, Y is vertical distance from edges in pixels. X is ignored for BottomCenter.",
                null,
                new ConfigurationManagerAttributes { Order = order-- }));

        KillfeedFontSize = config.Bind(
            "Killfeed",
            "Font Size",
            16,
            new ConfigDescription(
                "Size for killfeed lines in pixels.",
                null,
                new ConfigurationManagerAttributes { Order = order-- }));

        KillfeedLineSeconds = config.Bind(
            "Killfeed",
            "Line Visible Time",
            10f,
            new ConfigDescription(
                "Lifetime per killfeed line in seconds.",
                null,
                new ConfigurationManagerAttributes { Order = order-- }));

        KillfeedLineHeight = config.Bind(
            "Killfeed",
            "Line Height",
            24f,
            new ConfigDescription(
                "Vertical spacing between lines in pixels.",
                null,
                new ConfigurationManagerAttributes { Order = order-- }));

        KillfeedColor = config.Bind(
            "Killfeed",
            "Text Color",
            new Color(0.5f, 1f, 0.5f, 1f),
            new ConfigDescription(
                "Color for killfeed text.",
                null,
                new ConfigurationManagerAttributes { Order = order-- }));

        KillfeedTemplate = config.Bind(
            "Killfeed",
            "Text Template",
            "{attacker} → {victim} - [{bp}] - [{weapon} - {ammo}] - [{dist}m]",
            new ConfigDescription(
                "Tokens: {attacker} {victim} {bp} {weapon} {ammo} {dist}",
                null,
                new ConfigurationManagerAttributes { Order = order-- }));

        KillfeedShowFactionIcon = config.Bind(
            "Killfeed",
            "Show Faction Icon",
            true,
            new ConfigDescription(
                "Show the attacker Faction icon.",
                null,
                new ConfigurationManagerAttributes { Order = order-- }));

        KillfeedFactionTint = config.Bind(
            "Killfeed",
            "Faction Icon Color",
            new Color(0.25f, 0.75f, 1f, 1f),
            new ConfigDescription(
                "The color of the Faction Icon.",
                null,
                new ConfigurationManagerAttributes { Order = order-- }));

        Font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}

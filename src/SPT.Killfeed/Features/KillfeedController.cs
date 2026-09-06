using System.Collections.Generic;
using System.Linq;
using SPT.Killfeed.Models;
using SPT.Killfeed.Utilities;
using UnityEngine;

namespace SPT.Killfeed.Features;

public class KillfeedController : MonoBehaviour
{
    private static Texture2D _texture1x1;
    private readonly List<KillEntry> _kills = new();

    private void OnEnable()
    {
        EventBus.OnKill += OnKill;
    }

    private void OnDisable()
    {
        EventBus.OnKill -= OnKill;
    }

    private void Update()
    {
        if (_kills.Count == 0)
        {
            return;
        }

        var now = Time.unscaledTime;
        _kills.RemoveAll(kill => now - kill.Event.Time > Settings.KillfeedLineSeconds.Value);
    }

    private void OnGUI()
    {
        if (!Settings.KillfeedEnabled.Value || Event.current.type != EventType.Repaint)
        {
            return;
        }

        var anchor = Settings.KillfeedCorner.Value;
        var leftCorner = anchor == KillfeedAnchor.TopLeft
            || anchor == KillfeedAnchor.BottomLeft;
        var topCorner = anchor == KillfeedAnchor.TopLeft
            || anchor == KillfeedAnchor.TopRight;
        var bottomCenter = anchor == KillfeedAnchor.BottomCenter;

        var startX = bottomCenter
            ? Screen.width * 0.5f
            : leftCorner
                ? Settings.KillfeedMargin.Value.x
                : Screen.width - Settings.KillfeedMargin.Value.x;
        var startY = topCorner
            ? Settings.KillfeedMargin.Value.y
            : Screen.height - Settings.KillfeedMargin.Value.y;

        var now = Time.unscaledTime;
        var orderedKills = _kills.OrderBy(kill => kill.Event.Time).ToList();

        for (var index = 0; index < orderedKills.Count; index++)
        {
            var damageEvent = orderedKills[index].Event;
            var age = now - damageEvent.Time;
            var fade = 1f - Mathf.Clamp01(age / Settings.KillfeedLineSeconds.Value);
            var color = Settings.KillfeedColor.Value;
            color.a *= fade;

            var lineHeight = Settings.KillfeedLineHeight.Value;
            var fontSize = Settings.KillfeedFontSize.Value;
            var y = topCorner
                ? startY + index * lineHeight
                : startY - index * lineHeight;

            var distance = Mathf.RoundToInt(damageEvent.DistanceMeters);
            var text = Settings.KillfeedTemplate.Value
                .Replace("{attacker}", damageEvent.AttackerName)
                .Replace("{victim}", damageEvent.VictimName)
                .Replace("{bp}", damageEvent.BodyPart ?? string.Empty)
                .Replace("{weapon}", damageEvent.WeaponLabel ?? string.Empty)
                .Replace("{ammo}", damageEvent.AmmoName ?? string.Empty)
                .Replace("{dist}", distance.ToString());

            var faction = Settings.KillfeedShowFactionIcon.Value
                ? TextureBank.Faction(damageEvent.AttackerSide)
                : null;

            var iconHeight = fontSize + 6f;
            var iconWidth = iconHeight;
            const float padding = 6f;

            var textWidth = GLDrawer.MeasureTextWidth(text, fontSize, Settings.Font);
            var totalWidth = textWidth + (faction ? iconWidth + padding : 0f);
            var drawX = bottomCenter
                ? startX - totalWidth * 0.5f
                : leftCorner
                    ? startX
                    : startX - totalWidth;

            var previousColor = GUI.color;
            var backgroundRect = new Rect(drawX - 6f, y - 2f, totalWidth + 12f, iconHeight + 4f);
            GUI.color = new Color(0f, 0f, 0f, color.a * 0.25f);
            GUI.DrawTexture(backgroundRect, Texture1x1());
            GUI.color = previousColor;

            if (faction)
            {
                var iconRect = new Rect(drawX, y, iconWidth, iconHeight);
                var tint = Settings.KillfeedFactionTint.Value;
                if (tint.a <= 0f)
                {
                    tint.a = color.a;
                }

                var colorBeforeIcon = GUI.color;
                GUI.color = new Color(tint.r, tint.g, tint.b, color.a);
                GUI.DrawTexture(iconRect, faction, ScaleMode.ScaleToFit, true);
                GUI.color = colorBeforeIcon;
                drawX += iconWidth + padding;
            }

            GLDrawer.DrawText(
                text,
                new Vector2(drawX, y + iconHeight * 0.5f),
                fontSize,
                color,
                TextAnchor.MiddleLeft,
                Settings.Font,
                false);
        }
    }

    private void OnKill(DamageEvent damageEvent)
    {
        if (!damageEvent.IsLocalAttacker
            && !(Settings.KillfeedShowOtherKills.Value && !damageEvent.IsLocalVictim))
        {
            return;
        }

        _kills.Add(new KillEntry { Event = damageEvent });

        var capacity = Mathf.Max(0, Settings.KillfeedMaxLines.Value);
        if (capacity > 0)
        {
            while (_kills.Count > capacity)
            {
                _kills.RemoveAt(0);
            }
        }
    }

    private static Texture2D Texture1x1()
    {
        if (_texture1x1)
        {
            return _texture1x1;
        }

        _texture1x1 = new Texture2D(1, 1, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point
        };
        _texture1x1.SetPixel(0, 0, Color.white);
        _texture1x1.Apply();
        return _texture1x1;
    }

    private sealed class KillEntry
    {
        public DamageEvent Event;
    }
}

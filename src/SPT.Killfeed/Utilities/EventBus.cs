using System;
using SPT.Killfeed.Models;
using UnityEngine;

namespace SPT.Killfeed.Utilities;

public static class EventBus
{
    public static event Action<DamageEvent> OnKill;

    public static void RaiseKill(DamageEvent damageEvent)
    {
        damageEvent.Time = Time.unscaledTime;
        OnKill?.Invoke(damageEvent);
    }
}


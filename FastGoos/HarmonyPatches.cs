using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SamEngine;

namespace FastGoos
{
    [HarmonyPatch(typeof(Time), nameof(Time.TickTime))]
    internal class TimeTickTime
    {
        public static bool Prefix()
        {
            Time.time = ((float)Time.timeStopwatch.Elapsed.TotalSeconds) * (float)Math.Pow(GoosMod.TimeMultiplier, GoosMod.RealTimeMultiplier);
            return false;
        }
    }
}

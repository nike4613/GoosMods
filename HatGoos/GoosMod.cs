using GoosUtil;
using Harmony;
using LoadBins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HatGoos
{
    [Plugin]
    public static class GoosMod
    {
        internal static HarmonyInstance harmony;
        internal static Assembly goosAssembly;

        internal const string Name = "HatGoos";

        [Plugin]
        public static void Init(List<MethodInfo> entries)
        {
            HarmonyInstance.DEBUG = true;
            harmony = HarmonyInstance.Create(Name);
            harmony.PatchAll();

            GoosConfig.TryRead += TryReadConfigValue;
            GoosConfig.Stringify += StringifyConfigOptions;
            GoosConfig.Init += InInit;

            goosAssembly = entries.First().DeclaringType.Assembly;
        }

        public static bool TryReadConfigValue(KeyValuePair<string, string> pair)
        {
            switch (pair.Key)
            {
                /*case Name + "." + nameof(TimeMultiplier):
                    TimeMultiplier = Convert.ToSingle(pair.Value);
                    return true;
                case Name + "." + nameof(RealTimeMultiplier):
                    RealTimeMultiplier = Convert.ToSingle(pair.Value);
                    return true;*/
                default: return false;
            }
        }

        public static string StringifyConfigOptions() => ""
            /*$"{Name}.{nameof(TimeMultiplier)}={Convert.ToString(TimeMultiplier)}\n" +
            $"{Name}.{nameof(RealTimeMultiplier)}={Convert.ToString(RealTimeMultiplier)}\n"*/;

        internal static void InInit()
        {

        }
    }
}

using Harmony;
using LoadBins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GooseDesktop;
using System.Reflection.Emit;
using SamEngine;
using GoosMods;

namespace FastGoos
{
    [Plugin]
    public class GoosMod
    {
        internal static HarmonyInstance harmony;
        internal static Assembly goosAssembly;

        internal const string Name = "FastGoos";
        public static float TimeMultiplier = 5f;
        public static float RealTimeMultiplier = 0.75f;

        [Plugin]
        public static void Init(List<MethodInfo> entries)
        {
            HarmonyInstance.DEBUG = true;
            harmony = HarmonyInstance.Create(Name);
            harmony.PatchAll();

            GooseConfigReadPatch.ConfigRead = TryReadConfigValue;
            GooseConfigWritePatch.ConfigStringify = StringifyConfigOptions;
            InitPatch.Init = ApplyPatch;

            goosAssembly = entries.First().DeclaringType.Assembly;
        }

        public static bool TryReadConfigValue(KeyValuePair<string, string> pair)
        {
            switch (pair.Key)
            {
                case Name + "." + nameof(TimeMultiplier):
                    TimeMultiplier = Convert.ToSingle(pair.Value);
                    return true;
                case Name + "." + nameof(RealTimeMultiplier):
                    RealTimeMultiplier = Convert.ToSingle(pair.Value);
                    return true;
                default: return false;
            }
        }

        public static string StringifyConfigOptions() => 
            $"{Name}.{nameof(TimeMultiplier)}={Convert.ToString(TimeMultiplier)}\n" +
            $"{Name}.{nameof(RealTimeMultiplier)}={Convert.ToString(RealTimeMultiplier)}\n";

        internal static void ApplyPatch()
        {
            var theGoos = goosAssembly.GetType("GooseDesktop.TheGoose");
            var goosTick = theGoos.GetMethod("Tick");
            var goosNab = theGoos.GetMethod("RunNabMouse", BindingFlags.Static | BindingFlags.NonPublic);

            var transpiler = new HarmonyMethod(typeof(GoosMod).GetMethod(nameof(GoosTickTranspiler)));
            harmony.Patch(goosTick, transpiler: transpiler);
            harmony.Patch(goosNab, transpiler: transpiler);
        }

        public static IEnumerable<CodeInstruction> GoosTickTranspiler(IEnumerable<CodeInstruction> il_)
        {
            var il = new List<CodeInstruction>(il_);
            foreach (var inst in il)
            {
                if (inst.opcode == OpCodes.Ldc_R4 && (float)inst.operand == Time.deltaTime)
                    inst.operand = Time.deltaTime * TimeMultiplier;
            }
            return il;
        }
    }
}

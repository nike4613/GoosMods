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

namespace FastGoos
{
    [Plugin]
    public class GoosMod
    {
        internal static HarmonyInstance harmony;
        internal static Assembly goosAssembly;

        [Plugin]
        public static void Init(List<MethodInfo> entries)
        {
            HarmonyInstance.DEBUG = true;
            harmony = HarmonyInstance.Create("FastGoos");
            harmony.PatchAll();

            goosAssembly = entries.First().DeclaringType.Assembly;

            var goosTick = goosAssembly.GetType("GooseDesktop.TheGoose").GetMethod("Tick");

            var transpiler = new HarmonyMethod(typeof(GoosMod).GetMethod(nameof(GoosTickTranspiler)));
            harmony.Patch(goosTick, transpiler: transpiler);
        }

        public static IEnumerable<CodeInstruction> GoosTickTranspiler(IEnumerable<CodeInstruction> il_)
        {
            var il = new List<CodeInstruction>(il_);
            foreach (var inst in il)
            {
                if (inst.opcode == OpCodes.Ldc_R4 && (float)inst.operand == Time.deltaTime)
                    inst.operand = Time.deltaTime * 5;
            }
            return il;
        }
    }
}

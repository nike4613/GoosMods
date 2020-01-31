using GooseDesktop;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GoosMods
{
    [HarmonyPatch(typeof(GooseConfig.ConfigSettings), nameof(GooseConfig.ConfigSettings.ReadFileIntoConfig))]
    internal class GooseConfigReadPatch
    {
        public delegate bool TryRead(KeyValuePair<string, string> kvp);
        public static TryRead ConfigRead = null;
        public static bool TryReadConfigValue(KeyValuePair<string, string> kvp)
            => ConfigRead?.Invoke(kvp) ?? false;

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> il_)
        {
            var il = new List<CodeInstruction>(il_);
            CodeInstruction lastBranch = default;

            var tryReadConfig = typeof(GooseConfigReadPatch).GetMethod(nameof(TryReadConfigValue), BindingFlags.Static | BindingFlags.Public);

            for (int i = 0; i < il.Count; ++i)
            {
                var inst = il[i];
                if (inst.opcode == OpCodes.Br)
                    lastBranch = inst;

                if (inst.opcode == OpCodes.Call && lastBranch != null)
                {
                    var method = inst.operand as MethodInfo;
                    if (method.Name == "get_Current" && method.DeclaringType == typeof(Dictionary<string, string>.Enumerator))
                    { // this is near our injection spot
                        var branchTarget = lastBranch.operand;
                        if (il[++i].opcode == OpCodes.Stloc_S)
                        { // here is where we really care about
                            var ldTarget = il[i].operand;

                            var load = new CodeInstruction(OpCodes.Ldloc, ldTarget);
                            var call = new CodeInstruction(OpCodes.Call, tryReadConfig);
                            var bran = new CodeInstruction(OpCodes.Brtrue, branchTarget);

                            il.InsertRange(i + 1, new[] { load, call, bran });
                            break;
                        }
                    }
                }
            }
            return il;
        }
    }

    [HarmonyPatch(typeof(GooseConfig.ConfigSettings), nameof(GooseConfig.ConfigSettings.GenerateTextFromSettings))]
    internal class GooseConfigWritePatch
    {
        public delegate string Stringify();
        public static Stringify ConfigStringify = null;

        public static void Postfix(ref string __result)
        {
            __result += ConfigStringify?.Invoke() ?? "";
        }
    }

    [HarmonyPatch(typeof(MainGame), nameof(MainGame.Init))]
    internal class InitPatch
    {
        public delegate void InitDelegate();
        public static InitDelegate Init = null;
        public static void Postfix()
        {
            Init?.Invoke();
        }
    }
}

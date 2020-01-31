using GooseDesktop;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GoosUtil
{
    public static class GoosConfig
    {
        static GoosConfig() => GoosInit.Init();

        public delegate bool TryReadDelegate(KeyValuePair<string, string> kvp);
        public static event TryReadDelegate TryRead;
        internal static TryReadDelegate ReadEvent => TryRead;

        public delegate string StringifyDelegate();
        public static event StringifyDelegate Stringify;
        internal static StringifyDelegate StringifyEvent => Stringify;

        public delegate void InitDelegate();
        public static event InitDelegate Init;
        internal static InitDelegate InitEvent => Init;
    }

    [HarmonyPatch(typeof(GooseConfig.ConfigSettings), nameof(GooseConfig.ConfigSettings.ReadFileIntoConfig))]
    internal class GooseConfigReadPatch
    {
        public static bool TryReadConfigValue(KeyValuePair<string, string> kvp)
        {
            if (GoosConfig.ReadEvent == null) return false;

            foreach (var invoke in GoosConfig.ReadEvent.GetInvocationList().Cast<GoosConfig.TryReadDelegate>())
                if (invoke(kvp)) return true;
            return false;
        }

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> il_)
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

        public static void Postfix(ref string __result)
        {
            if (GoosConfig.StringifyEvent == null) return;

            foreach (var invoke in GoosConfig.StringifyEvent.GetInvocationList().Cast<GoosConfig.StringifyDelegate>())
                __result += invoke() ?? "";
        }
    }

    [HarmonyPatch(typeof(MainGame), nameof(MainGame.Init))]
    internal class InitPatch
    {
        public static void Postfix()
        {
            GoosConfig.InitEvent?.Invoke();
        }
    }
}

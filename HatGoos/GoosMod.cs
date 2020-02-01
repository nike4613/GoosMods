using GoosUtil;
using Harmony;
using LoadBins;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

        internal const string Name = "HatGoos";
        
        public enum HatType
        { Default, None, Custom }
        public static HatType HatMode = HatType.Default;
        public static float HorizontalSize = 1.5f;
        public static float HatPosition = .6f;
        public static string CustomHatPath = "";

        internal static Bitmap hatImage = null;

        [Plugin]
        public static void Init(List<MethodInfo> entries)
        {
            HarmonyInstance.DEBUG = true;
            harmony = HarmonyInstance.Create(Name);
            harmony.PatchAll();
            GoosRenderPatch.Apply(harmony);

            GoosConfig.TryRead += TryReadConfigValue;
            GoosConfig.Stringify += StringifyConfigOptions;
            GoosConfig.Init += InInit;
        }

        public static bool TryReadConfigValue(KeyValuePair<string, string> pair)
        {
            switch (pair.Key)
            {
                case Name + "." + nameof(HatMode):
                    return Enum.TryParse(pair.Value, out HatMode);
                case Name + "." + nameof(HorizontalSize):
                    return float.TryParse(pair.Value, out HorizontalSize);
                case Name + "." + nameof(HatPosition):
                    return float.TryParse(pair.Value, out HatPosition);
                case Name + "." + nameof(CustomHatPath):
                    if ((HatMode == HatType.Custom && File.Exists(pair.Value)) || HatMode != HatType.Custom)
                    {
                        CustomHatPath = pair.Value;
                        return true;
                    }
                    else return false;
                default: return false;
            }
        }

        public static string StringifyConfigOptions() =>
            $"{Name}.{nameof(HatMode)}={Convert.ToString(HatMode)}\n" +
            $"{Name}.{nameof(HorizontalSize)}={Convert.ToString(HorizontalSize)}\n" +
            $"{Name}.{nameof(HatPosition)}={Convert.ToString(HatPosition)}\n" +
            $"{Name}.{nameof(CustomHatPath)}={CustomHatPath}\n";

        internal static void InInit()
        {
            switch (HatMode)
            {
                case HatType.None:
                    hatImage = new Bitmap(1, 1);
                    hatImage.SetPixel(0, 0, Color.Transparent);
                    break;
                case HatType.Default:
                    hatImage = Resources.Default;
                    break;
                case HatType.Custom:
                    hatImage = new Bitmap(CustomHatPath);
                    break;
            }
        }
    }
}

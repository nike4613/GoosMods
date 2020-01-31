using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoosUtil
{
    internal class GoosInit
    {
        private static int initialized = 0;
        private static HarmonyInstance harmony = HarmonyInstance.Create("GoosUtil");

        public static void Init()
        {
            if (Interlocked.CompareExchange(ref initialized, 1, 0) < 1)
            { // initialize
                harmony.PatchAll();
            }
        }
    }
}

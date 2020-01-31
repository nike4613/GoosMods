using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastGoos
{
    [HarmonyPatch(typeof(CodeTranspiler), nameof(CodeTranspiler.ConvertInstructionsAndUnassignedValues))]
    internal class FixHarmony
    {
        public static bool Prefix(Type type, IEnumerable enumerable, ref Dictionary<object, Dictionary<string, object>> unassignedValues,
            ref IEnumerable __result)
        {
			var enumerableAssembly = type.GetGenericTypeDefinition().Assembly;
			var genericListType = enumerableAssembly.GetType(typeof(List<>).FullName);
			var elementType = type.GetGenericArguments()[0];
			var listType = /*enumerableAssembly.GetType(*/genericListType.MakeGenericType(new Type[] { elementType })/*.FullName)*/;
			var list = Activator.CreateInstance(listType);
			var listAdd = list.GetType().GetMethod("Add");
			unassignedValues = new Dictionary<object, Dictionary<string, object>>();
			foreach (var op in enumerable)
			{
				var elementTo = CodeTranspiler.ConvertInstruction(elementType, op, out var unassigned);
				unassignedValues.Add(elementTo, unassigned);
				listAdd.Invoke(list, new object[] { elementTo });
				// cannot yield return 'elementTo' here because we have an out parameter in the method
			}
			__result = list as IEnumerable;

			return false;
        }
    }
}

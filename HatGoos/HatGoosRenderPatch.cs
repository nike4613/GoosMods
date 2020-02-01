using Harmony;
using SamEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace HatGoos
{
    internal class HatGoosRenderPatch
    {
        private static Func<Vector2> GetNeckHeadPoint;
        private static Func<float> GetDirection;
        private static int HeadRadius;

        public static void Apply(HarmonyInstance inst)
        {
            var theGoos = typeof(Time).Assembly.GetType("GooseDesktop.TheGoose");
            var theGoosRig = theGoos.GetNestedType("Rig", BindingFlags.NonPublic);
            HeadRadius = (int)theGoosRig.GetField("HeadRadius1", BindingFlags.Public | BindingFlags.Static).GetValue(null);

            var goosRigField = theGoos.GetField("gooseRig", BindingFlags.NonPublic | BindingFlags.Static);
            var goosDirectionField = theGoos.GetField("direction", BindingFlags.NonPublic | BindingFlags.Static);
            var rigHeadPointField = theGoosRig.GetField("neckHeadPoint", BindingFlags.Public | BindingFlags.Instance);

            GetNeckHeadPoint = GenerateGetter<Vector2>(goosRigField, rigHeadPointField);
            GetDirection = GenerateGetter<float>(goosDirectionField);

            var render = theGoos.GetMethod("Render", BindingFlags.Public | BindingFlags.Static);

            var patch = new HarmonyMethod(typeof(HatGoosRenderPatch), nameof(Render));
            inst.Patch(render, postfix: patch);
        }

        private static Func<T> GenerateGetter<T>(FieldInfo first, params FieldInfo[] fields)
        {
            var dynMethod = new DynamicMethod($"get {string.Join(".", fields.Prepend(first).Select(f => f.Name))}", typeof(T), Type.EmptyTypes, true);
            var il = dynMethod.GetILGenerator();
            il.Emit(OpCodes.Ldsfld, first);
            foreach (var fld in fields)
                il.Emit(OpCodes.Ldfld, fld);
            il.Emit(OpCodes.Ret);

            return (Func<T>)dynMethod.CreateDelegate(typeof(Func<T>));
        }

        public static void Render(Graphics g)
        {
            if (GoosMod.hatImage == null) return;

            var direction = GetDirection() + 90f; ;
            var headPoint = GetNeckHeadPoint();

            var vertBase = (HeadRadius / 2) * GoosMod.HatPosition;

            var bmp = GoosMod.hatImage;
            var baseOffset = HeadRadius * GoosMod.HorizontalSize / 2;
            var vertOffset = (((float)bmp.Height) / (float)bmp.Width) * baseOffset * 2;

            var set = new[] {
                new Vector2(-baseOffset, vertBase + vertOffset),
                new Vector2(baseOffset, vertBase + vertOffset),
                new Vector2(-baseOffset, vertBase)
            };

            float? sin = null, cos = null;
            var rotated = set.Select(v => Rotate(v, direction, ref sin, ref cos));
            var translated = rotated.Select(v => v + headPoint);
            var asPoints = translated.Select(ToPoint).ToArray();

            g.DrawImage(GoosMod.hatImage, asPoints);
        }

        private static Vector2 Rotate(Vector2 point, float degrees, ref float? sin, ref float? cos)
        {
            if (sin == null) sin = Sin(degrees);
            if (cos == null) cos = Cos(degrees);
            return new Vector2(point.x * cos.Value - point.y * sin.Value,
                               point.y * cos.Value + point.x * sin.Value);
        }

        private static float Sin(float deg)
            => (float)Math.Sin(deg * (Math.PI / 180d));
        private static float Cos(float deg)
            => (float)Math.Cos(deg * (Math.PI / 180d));

        private static Point ToPoint(Vector2 vector)
            => new Point((int)vector.x, (int)vector.y);
    }
}

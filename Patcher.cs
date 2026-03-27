using HarmonyLib;

namespace ThreadingFix
{
    public static class Patcher
    {
        private const string HarmonyId = "com.threadingfix.csmod";
        private static bool patched = false;

        public static void PatchAll()
        {
            if (patched) return;
            patched = true;
            var harmony = new Harmony(HarmonyId);
            harmony.PatchAll(typeof(Patcher).Assembly);
        }

        public static void UnpatchAll()
        {
            if (!patched) return;
            var harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId);
            patched = false;
        }
    }
}

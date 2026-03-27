using CitiesHarmony.API;
using ICities;

namespace ThreadingFix
{
    public class Mod : IUserMod
    {
        public string Name => "Asset Import Threading Fix";
        public string Description => "Fixes thread leak in the asset editor's LOD generation.";

        public void OnEnabled()
        {
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled)
                Patcher.UnpatchAll();
        }
    }
}

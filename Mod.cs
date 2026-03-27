using CitiesHarmony.API;
using ICities;

namespace ThreadingFix
{
    public class Mod : IUserMod
    {
        public string Name => "Asset Import Threading Fix";
        public string Description => "Fixes TaskDistributor thread leak in the asset editor's LOD generation that causes 'too many threads' during bulk imports.";

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

using ColossalFramework.Threading;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace ThreadingFix
{
    /// <summary>
    /// Fixes a thread leak in ImportAssetLodded.BuildLOD() and BakeLODTextures().
    ///
    /// Both methods create a new TaskDistributor("TaskDistributor") as a local variable,
    /// which spawns Environment.ProcessorCount * 2 worker threads. These TaskDistributors
    /// are never disposed, so the worker threads leak permanently. After a few import
    /// cycles the accumulated threads hit the OS/runtime limit causing "too many threads".
    ///
    /// Fix: Transpiler replaces "new TaskDistributor(string)" with ThreadHelper.taskDistributor,
    /// reusing the existing global thread pool instead of creating disposable ones.
    /// </summary>
    [HarmonyPatch(typeof(ImportAssetLodded), "BuildLOD")]
    public static class BuildLODTranspiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return TaskDistributorFix.PatchNewTaskDistributor(instructions, "BuildLOD");
        }
    }

    [HarmonyPatch(typeof(ImportAssetLodded), "BakeLODTextures")]
    public static class BakeLODTexturesTranspiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return TaskDistributorFix.PatchNewTaskDistributor(instructions, "BakeLODTextures");
        }
    }

    public static class TaskDistributorFix
    {
        /// <summary>
        /// Replaces:
        ///   ldstr "TaskDistributor"
        ///   newobj instance void TaskDistributor::.ctor(string)
        /// With:
        ///   nop
        ///   call TaskDistributor ThreadHelper::get_taskDistributor()
        /// </summary>
        public static IEnumerable<CodeInstruction> PatchNewTaskDistributor(
            IEnumerable<CodeInstruction> instructions, string methodName)
        {
            var codes = instructions.ToList();
            var taskDistributorCtor = typeof(TaskDistributor).GetConstructor(new[] { typeof(string) });
            var globalDistributorGetter = AccessTools.PropertyGetter(typeof(ThreadHelper), "taskDistributor");

            bool patched = false;

            for (int i = 0; i + 1 < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr
                    && codes[i + 1].opcode == OpCodes.Newobj
                    && codes[i + 1].operand is ConstructorInfo ctor
                    && ctor == taskDistributorCtor)
                {
                    codes[i].opcode = OpCodes.Nop;
                    codes[i].operand = null;
                    codes[i + 1].opcode = OpCodes.Call;
                    codes[i + 1].operand = globalDistributorGetter;
                    patched = true;
                }
            }

            if (!patched)
            {
                Debug.LogWarning($"[ThreadingFix] No TaskDistributor constructor found in {methodName} — patch may be outdated");
            }

            return codes;
        }
    }
}

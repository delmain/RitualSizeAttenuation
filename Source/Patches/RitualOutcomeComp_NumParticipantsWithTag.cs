using HarmonyLib;
using RimWorld;
using System;

namespace Rachek128.RitualAttenuation.Patches
{
    [HarmonyPatch(typeof(RitualOutcomeComp_NumParticipantsWithTag))]
    [HarmonyPatch(nameof(RitualOutcomeComp_NumParticipantsWithTag.GetQualityFactor))]
    static class RitualOutcomeComp_NumParticipantsWithTag_GetQualityFactor
    {
        static void Postfix(RitualOutcomeComp_NumParticipantsWithTag __instance, RitualRoleAssignments assignments, ref QualityFactor __result)
        {
            var data = RitualExtendedDataManager.Instance.GetOrCreateFor(__instance, assignments, (d) =>
            {
                int required = PopulationUtility.GetScaledAttendanceFor(assignments, __instance);
                d.RequiredAttendance = required;
                d.BaseAttendance = PopulationUtility.GetBaseAttendanceFor(__instance);
                d.MaxQuality = PopulationUtility.GetBaseQualityFor(__instance);
            });

            data.ScheduledAttendance = PopulationUtility.GetCurrentAttendanceFor(assignments);

            __result.count = $"{data.ScheduledAttendance} / {data.RequiredAttendance}";
            __result.quality = PopulationUtility.EvaluateQualityFor(data, __instance);
            __result.positive = __result.quality > 0f;
            __result.qualityChange = Tools.ExpectedOffsetDesc(__instance, __result.positive, __result.quality);
        }
    }

    [HarmonyPatch(typeof(RitualOutcomeComp_NumParticipantsWithTag))]
    [HarmonyPatch(nameof(RitualOutcomeComp_NumParticipantsWithTag.Count))]
    static class RitualOutcomeComp_NumParticipantsWithTag_Count
    {
        static void Postfix(RitualOutcomeComp_NumParticipantsWithTag __instance, LordJob_Ritual ritual, RitualOutcomeComp_Data data, ref float __result)
        {
            var ext = RitualExtendedDataManager.Instance.GetFor(ritual, __instance);

            if (ext == null)
            {
                ext = RitualExtendedDataManager.Instance.GetFor(ritual.lord, __instance);
                if (ext == null)
                {
                    return;
                }
            }

            int present = 0;
            foreach (var pawn in ritual.assignments.Participants)
            {
                if (ritual.PawnTagSet(pawn, __instance.tag))
                    ++present;
            }
            __result = present;

            ext.Data.ActualAttendance = present;
        }
    }
}
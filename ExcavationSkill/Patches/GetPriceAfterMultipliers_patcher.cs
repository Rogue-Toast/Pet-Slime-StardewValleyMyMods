using HarmonyLib;
using StardewValley;
using SpaceCore;
using StardewModdingAPI;
using MoonShared.Patching;
using MoonShared;
using System;
using xTile.Dimensions;
using static SpaceCore.Skills.Skill;
using System.Reflection;

namespace ExcavationSkill
{
    internal class GetPriceAfterMultipliers_patcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<StardewValley.Object>("getPriceAfterMultipliers"),
                postfix: this.GetHarmonyMethod(nameof(After_GetPriceAfterMultipliers))
            );
        }


        /*********
        ** Private methods
        *********/
        /// Post Fix to make it so the player gets more money with the Antiquary profession

        [HarmonyLib.HarmonyPostfix]
        private static void After_GetPriceAfterMultipliers(
        StardewValley.Object __instance, ref float __result, float startPrice, long specificPlayerID)
        {
            float saleMultiplier = 1f;
            try
            {
                foreach (var farmer in Game1.getAllFarmers())
                {
                    if (Game1.player.useSeparateWallets)
                    {
                        if (specificPlayerID == -1)
                        {
                            if (farmer.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID || !farmer.isActive())
                            {
                                continue;
                            }
                        }
                        else if (farmer.UniqueMultiplayerID != specificPlayerID)
                        {
                            continue;
                        }
                    }
                    else if (!farmer.isActive())
                    {
                        continue;
                    }

                    if (farmer.HasCustomProfession(ExcavationSkill.Excavation10a2) && __instance.name.Contains("moonslime"))
                    {
                        Log.Trace("Excavation Skill: Player has Antiquary profession, adjusting item price");
                        if (ModEntry.MargoLoaded && Game1.player.HasCustomPrestigeProfession(ExcavationSkill.Excavation10a2))
                        {
                            saleMultiplier += 2f;
                        }
                        else
                        {
                            saleMultiplier += 1f;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {MethodBase.GetCurrentMethod()?.Name}:\n{ex}");
            }
            __result *= saleMultiplier;
        }
    }
}

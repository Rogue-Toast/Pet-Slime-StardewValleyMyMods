using HarmonyLib;
using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Locations;
using SpaceCore;
using Microsoft.Xna.Framework;
using StardewValley.Tools;
using MoonShared;
using System.Globalization;
using System.Linq;
using StardewModdingAPI;
using MoonShared.Patching;
using ExcavationSkill.Objects;

namespace ExcavationSkill
{
    internal class PierreShopTest_patch : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Utility>("getFishShopStock"),
                postfix: this.GetHarmonyMethod(nameof(After_Gain_EXP))
            );


        }


        /*********
        ** Private methods
        *********/

        
        /// Post Fix to make it so the player can get EXp. Also the extra loot chance when digging.
        [HarmonyLib.HarmonyPostfix]
        private static void After_Gain_EXP(Dictionary<ISalable, int[]> __result)
        {

            __result.Add(new ShifterObject(new Vector2(710, 1)), new int[2] { 2, 2147483647 });
        }
    }
}

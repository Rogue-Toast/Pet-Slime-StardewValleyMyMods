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

namespace ExcavationSkill
{
    internal class DigUpArtifactSpot_Patcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<GameLocation>("digUpArtifactSpot"),
                postfix: this.GetHarmonyMethod(nameof(After_Gain_EXP))
            );

            harmony.Patch(
                original: this.RequireMethod<GameLocation>("digUpArtifactSpot"),
                postfix: this.GetHarmonyMethod(nameof(After_Profession_Extra_Loot))
            );
        }


        /*********
        ** Private methods
        *********/


        /// Post Fix to make it so that the player gets extra loot with the Antiquarian Profession
        [HarmonyLib.HarmonyPostfix]
        private static void After_Profession_Extra_Loot(
        GameLocation __instance, int xLocation, int yLocation, Farmer who)
        {
            //Does The player have the Antiquarian Profession?
            if (Game1.player.HasCustomProfession(ExcavationSkill.Excavation10a1))
            {
                Log.Trace("Excavation skill: Player has Antiquarian");
                Random random = new Random(who.getTileX() * (int)who.DailyLuck * 2000 + who.getTileY() + (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed);
                if (ModEntry.MargoLoaded && Game1.player.HasCustomPrestigeProfession(ExcavationSkill.Excavation10a1))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Game1.createDebris(ModEntry.ArtifactLootTable[random.Next(ModEntry.ArtifactLootTable.Count)], xLocation, yLocation, random.Next(3));
                    }
                }
                else
                {

                    Game1.createDebris(ModEntry.ArtifactLootTable[random.Next(ModEntry.ArtifactLootTable.Count)], xLocation, yLocation, random.Next(3));
                }
            }
        }


        /// Post Fix to make it so the player can get EXp. Also the extra loot chance when digging.
        [HarmonyLib.HarmonyPostfix]
        private static void After_Gain_EXP(GameLocation __instance, int xLocation, int yLocation, Farmer who)
        {
            ModEntry.AddEXP(Game1.getFarmer(who.uniqueMultiplayerID), 10);
            Utilities.ApplySpeedBoost(Game1.getFarmer(who.uniqueMultiplayerID));

            double test = Utilities.GetLevel() * 0.05;
            bool bonusLoot = false;
            if (Game1.random.NextDouble() < test)
            {
                bonusLoot = true;
            }
            if (bonusLoot)
            {
                Log.Trace("excavation Skll, you won the extra loot chance!");
                int ObjectID = ModEntry.ArtifactLootTable[Game1.random.Next(ModEntry.ArtifactLootTable.Count)];
                Game1.createMultipleObjectDebris(ObjectID, xLocation, yLocation, 1, who.UniqueMultiplayerID);
            }
        }
    }
}

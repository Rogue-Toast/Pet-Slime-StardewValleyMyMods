using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using MoonShared;
using System.Reflection;
using System.Reflection.Emit;
using StardewValley.Tools;
using StardewValley.Menus;
using StardewValley.Objects;
using GrowableGiantCrops.Framework;
using Microsoft.Xna.Framework.Graphics;
using static StardewValley.Menus.CharacterCustomization;

namespace ShovelToolUpgrades
{
    [HarmonyPatch(typeof(Utility), nameof(Utility.getBlacksmithUpgradeStock))]
    class Utility_GetBlacksmithUpgradeStock
    {
        public static void Postfix(
            Dictionary<ISalable, int[]> __result,
            Farmer who)
        {
            try
            {
                UpgradeableShovel.AddToShopStock(itemPriceAndStock: __result, who: who);
            }
            catch (Exception e)
            {
                Log.Error($"Failed in {MethodBase.GetCurrentMethod().DeclaringType}\n{e}");
            }
        }
    }


    [HarmonyPatch(typeof(Farmer), nameof(Farmer.showHoldingItem))]
    class Farmer_ShowHoldingItem
    {
        public static bool Prefix(
            Farmer who)
        {
            try
            {
                Item mrg = who.mostRecentlyGrabbedItem;
                if (mrg is UpgradeableShovel)
                {
                    Rectangle r = UpgradeableShovel.IconSourceRectangle((who.mostRecentlyGrabbedItem as Tool).UpgradeLevel);
                    switch (mrg)
                    {
                        case UpgradeableShovel:
                            r = UpgradeableShovel.IconSourceRectangle((who.mostRecentlyGrabbedItem as Tool).UpgradeLevel);
                            break;
                    }
                    Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(
                        textureName: ModEntry.Assets.ToolSpritesPath,
                        sourceRect: r,
                        animationInterval: 2500f,
                        animationLength: 1,
                        numberOfLoops: 0,
                        position: who.Position + new Vector2(0f, -124f),
                        flicker: false,
                        flipped: false,
                        layerDepth: 1f,
                        alphaFade: 0f,
                        color: Color.White,
                        scale: 4f,
                        scaleChange: 0f,
                        rotation: 0f,
                        rotationChange: 0f)
                    {
                        motion = new Vector2(0f, -0.1f)
                    });
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed in {MethodBase.GetCurrentMethod().DeclaringType}\n{e}");
            }
            return true;
        }
    }
}

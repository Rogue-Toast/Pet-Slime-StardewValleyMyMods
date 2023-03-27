using MoonShared;
using MoonShared.Config;
using MoonShared.Command;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using MoonShared.Asset;
using MoonShared.APIs;
using HarmonyLib;
using System.IO;
using StardewValley.Menus;
using StardewValley;
using AtraBase.Models.Result;
using GrowableGiantCrops.Framework;
using System.Collections.Generic;

namespace ShovelToolUpgrades
{
    internal class ModEntry : Mod
    {
        public static ModEntry Instance;
        public static Config Config;
        public static Assets Assets;

        internal static bool RadiationTier => ModEntry.Instance.Helper.ModRegistry.IsLoaded("spacechase0.MoonMisadventures");


        internal static int MythicitePlaceholder = 852;

        public static IJsonAssetsApi JsonAssets;
        public static ISpaceCore SpaceCore;

        internal ITranslationHelper I18n => this.Helper.Translation;

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            Log.Init(this.Monitor);

            Config = helper.ReadConfig<Config>();

            Assets = new Assets();
            new AssetClassParser(this, Assets).ParseAssets();

            this.Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            SpaceCore = this.Helper.ModRegistry
                .GetApi<ISpaceCore>
                ("spacechase0.SpaceCore");
            if (SpaceCore is null)
            {
                Log.Error("Can't access the SpaceCore API. Is the mod installed correctly?");
                return;
            }



            new ConfigClassParser(this, Config).ParseConfigs();
            new Harmony(this.ModManifest.UniqueID).PatchAll();
            new CommandClassParser(this.Helper.ConsoleCommands, new Command()).ParseCommands();

            SpaceCore.RegisterSerializerType(typeof(UpgradeableShovel));


            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.Display.MenuChanged += OnMenuChanged;

        }

        //Run once
        //Method to replace any default shovels with the new upgradable shovels if the player has it in their inventory at the start of a day
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.player.hasItemInInventoryNamed("Shovel"))
            {
                Log.Debug("patch fied");
                var Farmer = Game1.player;
                string name = "Shovel";
                for (int i = 0; i < Farmer.Items.Count; i++)
                {
                    if (Farmer.Items[i] != null && Farmer.Items[i].Name != null && Farmer.Items[i].Name.Equals(name))
                    {
                        var test = Farmer.Items[i].GetType();
                        Log.Debug(test.ToString());
                        if (test.ToString() == "GrowableGiantCrops.Framework.ShovelTool")
                        {
                            Item newShovel = new UpgradeableShovel(0);
                            Farmer.Items[i] = newShovel;
                        }
                    }
                }
            }

            Helper.Events.GameLoop.DayStarted -= OnDayStarted;
        }

        //Replace the normal shovel that is sold with the upgradable shovel
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is StardewValley.Menus.ShopMenu shop)
            {
                if (shop.storeContext == "atravita.GiantCropShop")
                {
                    for (int i = 0; i < shop.forSale.Count; i++)
                    {
                        if (shop.forSale[i].Name == "Shovel")
                        {
                            Item upgradableShovel = new UpgradeableShovel(0);
                            shop.forSale[i] = upgradableShovel;
                            shop.itemPriceAndStock.Add(upgradableShovel, new int[2] { 5000, 1 });
                        }
                    }
                }
            }
        }

        public static int PriceForToolUpgradeLevel(int level)
        {
            return level switch
            {
                1 => 2000,
                2 => 5000,
                3 => 10000,
                4 => 25000,
                _ => 2000,
            };
        }

        public static int IndexOfExtraMaterialForToolUpgrade(int level)
        {
            return level switch
            {
                1 => 334,
                2 => 335,
                3 => 336,
                4 => 337,
                _ => 334,
            };
        }
    }
}

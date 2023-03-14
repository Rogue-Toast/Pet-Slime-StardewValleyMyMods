using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using StardewValley.Tools;
using StardewValley.TerrainFeatures;
using StardewValley;
using StardewValley.Objects;
using SOBject = StardewValley.Object;
using StardewValley.Locations;
using MoonShared;

namespace ExcavationSkill.Objects
{

    [XmlType("Mods_moonslime_ExcavationSkill_ShifterObject")]
    public class ShifterObject : SOBject
    {
        public const int LidFlapTimerInterval = 120;

        private float YBob;

        [XmlElement("DirectionOffset")]
        public readonly NetVector2 DirectionOffset = new NetVector2();

        [XmlElement("Bait")]
        public readonly NetRef<SOBject> Bait = new NetRef<SOBject>();

        public int TileIndexToShow;

        private bool LidFlapping;

        private bool LidClosing;

        private float LidFlapTimer;

        private float ShakeTimer;

        private Vector2 Shake;

        public ShifterObject()
        {
        }


        public override string DisplayName { get => ModEntry.Instance.I18n.Get("moonslime.excavation.test.name"); set { } }

        protected override void initNetFields()
        {
            base.initNetFields();
            base.NetFields.AddFields(DirectionOffset, Bait);
        }

        public override string getDescription()
        {
            return ModEntry.Instance.I18n.Get("moonslime.excavation.test.description");
        }

        public List<Vector2> GetOverlayTiles(GameLocation location)
        {
            List<Vector2> list = new List<Vector2>();
            if (this.DirectionOffset.Y < 0f)
            {
                AddOverlayTilesIfNecessary(location, (int)this.TileLocation.X, (int)this.TileLocation.Y, list);
            }

            AddOverlayTilesIfNecessary(location, (int)this.TileLocation.X, (int)this.TileLocation.Y + 1, list);
            if (this.DirectionOffset.X < 0f)
            {
                AddOverlayTilesIfNecessary(location, (int)this.TileLocation.X - 1, (int)this.TileLocation.Y + 1, list);
            }

            if (this.DirectionOffset.X > 0f)
            {
                AddOverlayTilesIfNecessary(location, (int)this.TileLocation.X + 1, (int)this.TileLocation.Y + 1, list);
            }

            return list;
        }

        protected void AddOverlayTilesIfNecessary(GameLocation location, int tile_x, int tile_y, List<Vector2> tiles)
        {
            if (location == Game1.currentLocation && location.getTileIndexAt(tile_x, tile_y, "Buildings") >= 0 && location.doesTileHaveProperty(tile_x, tile_y + 1, "Back", "Water") == null)
            {
                tiles.Add(new Vector2(tile_x, tile_y));
            }
        }

        public void AddOverlayTiles(GameLocation location)
        {
            if (location != Game1.currentLocation)
            {
                return;
            }

            foreach (Vector2 overlayTile in GetOverlayTiles(location))
            {
                if (!Game1.crabPotOverlayTiles.ContainsKey(overlayTile))
                {
                    Game1.crabPotOverlayTiles[overlayTile] = 0;
                }

                Game1.crabPotOverlayTiles[overlayTile]++;
            }
        }

        public new string Name = ModEntry.Instance.I18n.Get("");

        public void RemoveOverlayTiles(GameLocation location)
        {
            if (location != Game1.currentLocation)
            {
                return;
            }
            
            foreach (Vector2 overlayTile in GetOverlayTiles(location))
            {
                if (Game1.crabPotOverlayTiles.ContainsKey(overlayTile))
                {
                    Game1.crabPotOverlayTiles[overlayTile]--;
                    if (Game1.crabPotOverlayTiles[overlayTile] <= 0)
                    {
                        Game1.crabPotOverlayTiles.Remove(overlayTile);
                    }
                }
            }
        }

        public ShifterObject(Vector2 tileLocation, int stack = 1)
            : base(tileLocation, 9, ModEntry.Instance.I18n.Get("moonslime.excavation.test.name"), canBeSetDown: true, canBeGrabbed: false, isHoedirt: false, isSpawnedObject: false)
        {

            this.Type = "interactive";
            this.TileIndexToShow = this.ParentSheetIndex;
        }

        public static bool IsValidCrabPotLocationTile(GameLocation location, int x, int y)
        {
            if (location is Caldera)
            {
                return false;
            }

            Vector2 key = new Vector2(x, y);
            bool flag = (location.doesTileHaveProperty(x + 1, y, "Water", "Back") != null && location.doesTileHaveProperty(x - 1, y, "Water", "Back") != null) || (location.doesTileHaveProperty(x, y + 1, "Water", "Back") != null && location.doesTileHaveProperty(x, y - 1, "Water", "Back") != null);
            if (location.objects.ContainsKey(key) || !flag || location.doesTileHaveProperty((int)key.X, (int)key.Y, "Water", "Back") == null || location.doesTileHaveProperty((int)key.X, (int)key.Y, "Passable", "Buildings") != null)
            {
                return false;
            }

            return true;
        }

        public override void actionOnPlayerEntry()
        {
            this.UpdateOffset(Game1.currentLocation);
            this.AddOverlayTiles(Game1.currentLocation);
            base.actionOnPlayerEntry();
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            Vector2 vector = new Vector2(x / 64, y / 64);
            if (who != null)
            {
                this.owner.Value = who.UniqueMultiplayerID;
            }

            if (!IsValidCrabPotLocationTile(location, (int)vector.X, (int)vector.Y))
            {
                return false;
            }

            UpdateOffset(location);
            this.TileLocation = new Vector2(x / 64, y / 64);
            var shifterObject = new ShifterObject(TileLocation);
            shifterObject.DirectionOffset.Value = GetUpdateOffset(location);
            location.objects.Add(this.TileLocation, shifterObject);
            location.playSound("waterSlosh");
            DelayedAction.playSoundAfterDelay("slosh", 150);
            AddOverlayTiles(location);
            return true;
        }

        public void UpdateOffset(GameLocation location)
        {
            Vector2 zero = Vector2.Zero;
            if (CheckLocation(location, this.TileLocation.X - 1f, this.TileLocation.Y))
            {
                zero += new Vector2(32f, 0f);
            }

            if (CheckLocation(location, this.TileLocation.X + 1f, this.TileLocation.Y))
            {
                zero += new Vector2(-32f, 0f);
            }

            if (zero.X != 0f && CheckLocation(location, this.TileLocation.X + (float)Math.Sign(zero.X), this.TileLocation.Y + 1f))
            {
                zero += new Vector2(0f, -42f);
            }

            if (CheckLocation(location, this.TileLocation.X, this.TileLocation.Y - 1f))
            {
                zero += new Vector2(0f, 32f);
            }

            if (CheckLocation(location, this.TileLocation.X, this.TileLocation.Y + 1f))
            {
                zero += new Vector2(0f, -42f);
            }

            this.DirectionOffset.Value = zero;
        }
        public Vector2 GetUpdateOffset(GameLocation location)
        {
            Vector2 zero = Vector2.Zero;
            if (CheckLocation(location, this.TileLocation.X - 1f, this.TileLocation.Y))
            {
                zero += new Vector2(32f, 0f);
            }

            if (CheckLocation(location, this.TileLocation.X + 1f, this.TileLocation.Y))
            {
                zero += new Vector2(-32f, 0f);
            }

            if (zero.X != 0f && CheckLocation(location, this.TileLocation.X + (float)Math.Sign(zero.X), this.TileLocation.Y + 1f))
            {
                zero += new Vector2(0f, -42f);
            }

            if (CheckLocation(location, this.TileLocation.X, this.TileLocation.Y - 1f))
            {
                zero += new Vector2(0f, 32f);
            }

            if (CheckLocation(location, this.TileLocation.X, this.TileLocation.Y + 1f))
            {
                zero += new Vector2(0f, -42f);
            }

            return zero;
        }




        protected bool CheckLocation(GameLocation location, float tile_x, float tile_y)
        {
            if (location.doesTileHaveProperty((int)tile_x, (int)tile_y, "Water", "Back") == null || location.doesTileHaveProperty((int)tile_x, (int)tile_y, "Passable", "Buildings") != null)
            {
                return true;
            }

            return false;
        }

        public override bool canBePlacedInWater()
        {
            return true;
        }

        public override bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            if (CrabPot.IsValidCrabPotLocationTile(l, (int)tile.X, (int)tile.Y))
            {
                return true;
            }
            return false;
        }

        public override bool isPlaceable()
        {
            return true;
        }


        public override Item getOne()
        {
            ShifterObject @object = new ShifterObject(Vector2.Zero, 1);
            @object._GetOneFrom(this);
            return @object;
        }

        public override void _GetOneFrom(Item source)
        {
            orderData.Value = (source as ShifterObject).orderData.Value;
            owner.Value = (source as ShifterObject).owner.Value;
            base._GetOneFrom(source);
        }

        public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who)
        {
            SOBject @object = dropInItem as SOBject;
            if (@object == null)
            {
                return false;
            }

            Farmer farmer = Game1.getFarmer(this.owner.Value);
            if (@object.name == "Fiber" && this.Bait.Value == null && (farmer == null || !farmer.professions.Contains(11)))
            {
                if (!probe)
                {
                    if (who != null)
                    {
                        this.owner.Value = who.UniqueMultiplayerID;
                    }

                    this.Bait.Value = @object.getOne() as SOBject;
                    who.currentLocation.playSound("Ship");
                    this.LidFlapping = true;
                    this.LidFlapTimer = 60f;
                }

                return true;
            }

            return false;
        }

        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            if (this.TileIndexToShow == 13)
            {
                if (justCheckingForActivity)
                {
                    return true;
                }

                SOBject value = this.heldObject.Value;
                this.heldObject.Value = null;
                if (who.IsLocalPlayer && !who.addItemToInventoryBool(value))
                {
                    this.heldObject.Value = value;
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                    return false;
                }

                Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
                if (dictionary.ContainsKey(value.ParentSheetIndex))
                {
                    string[] array = dictionary[value.ParentSheetIndex].Split('/');
                    int minValue = ((array.Length <= 5) ? 1 : Convert.ToInt32(array[5]));
                    int num = ((array.Length > 5) ? Convert.ToInt32(array[6]) : 10);
                    who.caughtFish(value.ParentSheetIndex, Game1.random.Next(minValue, num + 1));
                }

                this.readyForHarvest.Value = false;
                this.TileIndexToShow = 9;
                this.LidFlapping = true;
                this.LidFlapTimer = 60f;
                this.Bait.Value = null;
                who.animateOnce(279 + who.FacingDirection);
                who.currentLocation.playSound("fishingRodBend");
                DelayedAction.playSoundAfterDelay("coin", 500);
                who.gainExperience(1, 5);
                this.Shake = Vector2.Zero;
                this.ShakeTimer = 0f;
                return true;
            }

            if (this.Bait.Value == null)
            {
                if (justCheckingForActivity)
                {
                    return true;
                }

                if (Game1.didPlayerJustClickAtAll(ignoreNonMouseHeldInput: true))
                {
                    if (Game1.player.addItemToInventoryBool(this.getOne()))
                    {

                        Log.Debug("Shifting net Test");
                        if (who.isMoving())
                        {
                            Game1.haltAfterCheck = false;
                        }

                        Game1.playSound("coin");
                        Game1.currentLocation.objects.Remove(this.TileLocation);
                        return true;
                    }

                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                }
            }

            return false;
        }

        public override void performRemoveAction(Vector2 tileLocation, GameLocation environment)
        {
            this.RemoveOverlayTiles(environment);
            base.performRemoveAction(tileLocation, environment);
        }

        public override void DayUpdate(GameLocation location)
        {
            bool flag = Game1.getFarmer(this.owner.Value) != null && Game1.getFarmer(this.owner.Value).professions.Contains(11);
            bool flag2 = Game1.getFarmer(this.owner.Value) != null && Game1.getFarmer(this.owner.Value).professions.Contains(10);
            if ((long)this.owner.Value == 0L && Game1.player.professions.Contains(11))
            {
                flag2 = true;
            }

            if (!(this.Bait.Value != null || flag) || this.heldObject.Value != null)
            {
                return;
            }

            this.TileIndexToShow = 13;
            this.readyForHarvest.Value = true;
            Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + (int)this.TileLocation.X * 1000 + (int)this.TileLocation.Y);
            Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
            List<int> list = new List<int>();
            double num = (flag2 ? 0.0 : 0.2);
            if (!flag2)
            {
                num += (double)location.getExtraTrashChanceForCrabPot((int)this.TileLocation.X, (int)this.TileLocation.Y);
            }

            if (random.NextDouble() > num)
            {
                foreach (KeyValuePair<int, string> item in dictionary)
                {
                    if (!item.Value.Contains("trap"))
                    {
                        continue;
                    }

                    bool flag3 = location is Beach || location.catchOceanCrabPotFishFromThisSpot((int)this.TileLocation.X, (int)this.TileLocation.Y);
                    string[] array = item.Value.Split('/');
                    if ((array[4].Equals("ocean") && !flag3) || (array[4].Equals("freshwater") && flag3))
                    {
                        continue;
                    }

                    if (flag2)
                    {
                        list.Add(item.Key);
                        continue;
                    }

                    double num2 = Convert.ToDouble(array[2]);
                    if (!(random.NextDouble() < num2))
                    {
                        continue;
                    }

                    this.heldObject.Value = new SOBject(item.Key, 1);
                    break;
                }
            }

            if (this.heldObject.Value == null)
            {
                if (flag2 && list.Count > 0)
                {
                    this.heldObject.Value = new SOBject(list[random.Next(list.Count)], 1);
                }
                else
                {
                    this.heldObject.Value = new SOBject(random.Next(168, 173), 1);
                }
            }
        }

        public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
        {
            if (this.LidFlapping)
            {
                this.LidFlapTimer -= time.ElapsedGameTime.Milliseconds;
                if (this.LidFlapTimer <= 0f)
                {
                    this.TileIndexToShow += ((!this.LidClosing) ? 1 : (-1));
                    if (this.TileIndexToShow >= 12 && !this.LidClosing)
                    {
                        this.LidClosing = true;
                        this.TileIndexToShow--;
                        if (this.TileIndexToShow <= 8)
                        {
                            this.LidFlapTimer = 0f;
                            this.LidFlapping = false;
                        }
                    }
                    else if (this.TileIndexToShow <= 8 && this.LidClosing)
                    {
                        this.LidClosing = false;
                        this.TileIndexToShow++;
                        this.LidFlapping = false;
                        if (this.TileIndexToShow >= 12)
                        {
                            this.LidFlapTimer = 0f;
                            this.LidFlapping = false;
                        }
                        if (this.Bait.Value != null)
                        {
                            this.TileIndexToShow = 12;
                        }
                    }

                    this.LidFlapTimer = 60f;
                }
            }

            if ((bool)this.readyForHarvest.Value && this.heldObject.Value != null)
            {
                this.ShakeTimer -= time.ElapsedGameTime.Milliseconds;
                if (this.ShakeTimer < 0f)
                {
                    this.ShakeTimer = Game1.random.Next(2800, 3200);
                }
            }

            if (this.ShakeTimer > 2000f)
            {
                this.Shake.X = Game1.random.Next(-1, 2);
            }
            else
            {
                this.Shake.X = 0f;
            }
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (this.heldObject.Value != null)
            {
                this.TileIndexToShow = 13;
            }
            else if (this.TileIndexToShow == 0)
            {
                this.TileIndexToShow = this.ParentSheetIndex;
            }

            this.YBob = (float)(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 500.0 + (double)(x * 64)) * 8.0 + 8.0);
            if (this.YBob <= 0.001f)
            {
                Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 8, 0, this.DirectionOffset.Value + new Vector2(x * 64 + 4, y * 64 + 32), flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f));
            }

            _ = Game1.currentLocation.Map.GetLayer("Buildings").Tiles[x, y];
            spriteBatch.Draw(ModEntry.Assets.tilesheet, Game1.GlobalToLocal(Game1.viewport, this.DirectionOffset.Value + new Vector2(x * 64, y * 64 + (int)this.YBob)) + this.Shake, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.TileIndexToShow, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, ((float)(y * 64) + this.DirectionOffset.Y + (float)(x % 4)) / 10000f);
            if (Game1.currentLocation.waterTiles != null && x < Game1.currentLocation.waterTiles.waterTiles.GetLength(0) && y < Game1.currentLocation.waterTiles.waterTiles.GetLength(1) && Game1.currentLocation.waterTiles.waterTiles[x, y].isWater)
            {
                if (Game1.currentLocation.waterTiles.waterTiles[x, y].isVisible)
                {
                    spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.DirectionOffset.Value + new Vector2(x * 64 + 4, y * 64 + 48)) + this.Shake, new Rectangle(Game1.currentLocation.waterAnimationIndex * 64, 2112 + (((x + y) % 2 != 0) ? ((!Game1.currentLocation.waterTileFlip) ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 128 : 0)), 56, 16 + (int)YBob), Game1.currentLocation.waterColor.Value, 0f, Vector2.Zero, 1f, SpriteEffects.None, ((float)(y * 64) + DirectionOffset.Y + (float)(x % 4)) / 9999f);
                }
                else
                {
                    Color a = new Color(135, 135, 135, 215);
                    a = Utility.MultiplyColor(a, Game1.currentLocation.waterColor.Value);
                    spriteBatch.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, DirectionOffset + new Vector2(x * 64 + 4, y * 64 + 48)) + Shake, null, a, 0f, Vector2.Zero, new Vector2(56f, 16 + (int)YBob), SpriteEffects.None, ((float)(y * 64) + DirectionOffset.Y + (float)(x % 4)) / 9999f);
                }
            }

            if ((bool)this.readyForHarvest.Value && this.heldObject.Value != null)
            {
                float num = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, DirectionOffset + new Vector2(x * 64 - 8, (float)(y * 64 - 96 - 16) + num)), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-06f + this.TileLocation.X / 10000f);
                spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, DirectionOffset + new Vector2(x * 64 + 32, (float)(y * 64 - 64 - 8) + num)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, heldObject.Value.ParentSheetIndex, 16, 16), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-05f + this.TileLocation.X / 10000f);
            }
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            if ((bool)f.ActiveObject.bigCraftable.Value)
            {
                spriteBatch.Draw(Game1.bigCraftableSpriteSheet, objectPosition, getSourceRectForBigCraftable(f.ActiveObject.ParentSheetIndex), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 3) / 10000f));
                return;
            }

            spriteBatch.Draw(ModEntry.Assets.tilesheet, objectPosition, GameLocation.getSourceRectForObject(f.ActiveObject.ParentSheetIndex), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 3) / 10000f));
            if (f.ActiveObject == null || !f.ActiveObject.Name.Contains("="))
            {
                return;
            }

            spriteBatch.Draw(ModEntry.Assets.tilesheet, objectPosition + new Vector2(32f, 32f), GameLocation.getSourceRectForObject(f.ActiveObject.ParentSheetIndex), Color.White, 0f, new Vector2(32f, 32f), 4f + Math.Abs(Game1.starCropShimmerPause) / 8f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 3) / 10000f));
            if (!(Math.Abs(Game1.starCropShimmerPause) <= 0.05f) || !(Game1.random.NextDouble() < 0.97))
            {
                Game1.starCropShimmerPause += 0.04f;
                if (Game1.starCropShimmerPause >= 0.8f)
                {
                    Game1.starCropShimmerPause = -0.8f;
                }
            }
        }

        public override void drawAsProp(SpriteBatch b)
        {
            if (this.isTemporarilyInvisible)
            {
                return;
            }

            int num = (int)this.TileLocation.X;
            int num2 = (int)this.TileLocation.Y;
            if ((bool)this.bigCraftable.Value)
            {
                Vector2 vector = getScale();
                vector *= 4f;
                Vector2 vector2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(num * 64, num2 * 64 - 64));
                b.Draw(destinationRectangle: new Microsoft.Xna.Framework.Rectangle((int)(vector2.X - vector.X / 2f), (int)(vector2.Y - vector.Y / 2f), (int)(64f + vector.X), (int)(128f + vector.Y / 2f)), texture: Game1.bigCraftableSpriteSheet, sourceRectangle: getSourceRectForBigCraftable(showNextIndex ? (base.ParentSheetIndex + 1) : base.ParentSheetIndex), color: Color.White, rotation: 0f, origin: Vector2.Zero, effects: SpriteEffects.None, layerDepth: Math.Max(0f, (float)((num2 + 1) * 64 - 1) / 10000f) + (((int)this.ParentSheetIndex == 105 || (int)this.ParentSheetIndex == 264) ? 0.0015f : 0f));
                if (Name.Equals("Loom") && (int)minutesUntilReady.Value > 0)
                {
                    b.Draw(Game1.objectSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(32f, 0f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435), Color.White, scale.X, new Vector2(32f, 32f), 1f, SpriteEffects.None, Math.Max(0f, (float)((num2 + 1) * 64 - 1) / 10000f + 0.0001f));
                }

                return;
            }

            if ((int)this.ParentSheetIndex != 590 && (int)this.ParentSheetIndex != 742)
            {
                b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)getBoundingBox(new Vector2(num, num2)).Bottom / 15000f);
            }

            Texture2D objectSpriteSheet = ModEntry.Assets.tilesheet;
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(num * 64 + 32, num2 * 64 + 32));
            Microsoft.Xna.Framework.Rectangle? sourceRectangle = GameLocation.getSourceRectForObject(this.ParentSheetIndex);
            Color white = Color.White;
            Vector2 origin = new Vector2(8f, 8f);
            _ = scale;
            b.Draw(objectSpriteSheet, position, sourceRectangle, white, 0f, origin, (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)getBoundingBox(new Vector2(num, num2)).Bottom / 10000f);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if ((bool)this.IsRecipe)
            {
                transparency = 0.5f;
                scaleSize *= 0.75f;
            }

            bool flag = ((drawStackNumber == StackDrawType.Draw && maximumStackSize() > 1 && Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && Stack != int.MaxValue;
            if (this.IsRecipe)
            {
                flag = false;
            }

            if ((bool)this.bigCraftable.Value)
            {
                Microsoft.Xna.Framework.Rectangle sourceRectForBigCraftable = getSourceRectForBigCraftable(this.ParentSheetIndex);
                spriteBatch.Draw(Game1.bigCraftableSpriteSheet, location + new Vector2(32f, 32f), sourceRectForBigCraftable, color * transparency, 0f, new Vector2(8f, 16f), 4f * (((double)scaleSize < 0.2) ? scaleSize : (scaleSize / 2f)), SpriteEffects.None, layerDepth);
                if (flag)
                {
                    Utility.drawTinyDigits(stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
                }
            }
            else
            {
                if ((int)this.ParentSheetIndex != 590 && drawShadow)
                {
                    spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), Game1.shadowTexture.Bounds, color * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
                }

                spriteBatch.Draw(ModEntry.Assets.tilesheet, location + new Vector2((int)(32f * scaleSize), (int)(32f * scaleSize)), Game1.getSourceRectForStandardTileSheet(ModEntry.Assets.tilesheet, parentSheetIndex, 16, 16), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
                if (flag)
                {
                    Utility.drawTinyDigits(stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 1f), 3f * scaleSize, 1f, color);
                }

                if (drawStackNumber != 0 && (int)this.Quality > 0)
                {
                    Microsoft.Xna.Framework.Rectangle value = (((int)this.Quality < 4) ? new Microsoft.Xna.Framework.Rectangle(338 + ((int)this.Quality - 1) * 8, 400, 8, 8) : new Microsoft.Xna.Framework.Rectangle(346, 392, 8, 8));
                    Texture2D mouseCursors = Game1.mouseCursors;
                    float num = (((int)this.Quality < 4) ? 0f : (((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1f) * 0.05f));
                    spriteBatch.Draw(mouseCursors, location + new Vector2(12f, 52f + num), value, color * transparency, 0f, new Vector2(4f, 4f), 3f * scaleSize * (1f + num), SpriteEffects.None, layerDepth);
                }

                if (base.Category == -22 && uses.Value > 0)
                {
                    float num2 = ((float)(FishingRod.maxTackleUses - uses.Value) + 0f) / (float)FishingRod.maxTackleUses;
                    spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)location.X, (int)(location.Y + 56f * scaleSize), (int)(64f * scaleSize * num2), (int)(8f * scaleSize)), Utility.getRedToGreenLerpColor(num2));
                }
            }

            if ((bool)this.IsRecipe)
            {
                spriteBatch.Draw(ModEntry.Assets.tilesheet, location + new Vector2(16f, 16f), Game1.getSourceRectForStandardTileSheet(ModEntry.Assets.tilesheet, 451, 16, 16), color, 0f, Vector2.Zero, 3f, SpriteEffects.None, layerDepth + 0.0001f);
            }
        }
    }
}

using MoonShared.Asset;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;

namespace RadiationTierTools
{
    [AssetClass]
    public class Assets
    {
        [AssetProperty("assets/tools-radioactive.png", Priority = AssetLoadPriority.Medium)]
        public static Texture2D RadioactiveTools { get; set; }
    }
}

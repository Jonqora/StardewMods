using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ExtendedEmotes
{
	internal class TileSheetEditor : IAssetEditor
	{
		/*********
        ** Accessors
        *********/
		protected static IModHelper Helper => ModEntry.Instance.Helper;
		protected static IMonitor Monitor => ModEntry.Instance.Monitor;


		/*********
        ** Public methods
        *********/
		/// <summary>Get whether this instance can edit the given asset.</summary>
		/// <typeparam name="_T">The asset Type.</typeparam>
		/// <param name="asset">Basic metadata about the asset being loaded.</param>
		/// <returns>true for asset TileSheets\Emotes, false otherwise</returns>
		public bool CanEdit<_T> (IAssetInfo asset)
		{
			return asset.AssetNameEquals($"TileSheets\\Emotes");
		}

		/// <summary>Extend the TileSheets\Emotes spritesheet downwards to add new images onto the bottom.</summary>
		/// <typeparam name="_T">The asset Type</typeparam>
		/// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it</param>
		public void Edit<_T> (IAssetData asset)
		{
			var editor = asset.AsImage();
			Texture2D sourceImage = Helper.Content.Load<Texture2D>("assets\\Grandpa.png", ContentSource.ModFolder);

			editor.ExtendImage(minWidth: 128, minHeight: 384);
			editor.PatchImage(sourceImage);

			Monitor.Log($"Edited {asset.AssetName}", LogLevel.Debug);
		}
	}
}
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace AngryGrandpa
{
	internal class PortraitEditor : IAssetEditor
	{
		protected static IModHelper Helper => ModEntry.Instance.Helper;
		protected static IMonitor Monitor => ModEntry.Instance.Monitor;
		protected static ModConfig Config => ModConfig.Instance;

		protected static ITranslationHelper i18n = Helper.Translation;

		public bool CanEdit<_T> (IAssetInfo asset)
		{
			return asset.AssetNameEquals($"Portraits\\Grandpa");
		}

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
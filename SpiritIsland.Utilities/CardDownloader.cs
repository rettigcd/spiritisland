using System.Drawing;
using System.Text.RegularExpressions;

namespace SpiritIsland.Tests.Core;

public static class CardDownloader {
	static public async Task<Bitmap> GetImage( string powerName ) {
		string preparedPowerName = powerName.Replace(" ","_");
		string cardUrl = $"https://spiritislandwiki.com/index.php?title=File:{preparedPowerName}.png";

		var client = new HttpClient();
		try {
			var html = await client.GetStringAsync(cardUrl);
			string url = new Regex( @"<img.*?alt=""File:.*src=""([^""]+)""[^>]*>" ).Match( html ).Groups[1].Value;

			var imgUrl = $"https://spiritislandwiki.com{url}";

			byte[] byteArrayIn = await client.GetByteArrayAsync(imgUrl);

			using var ms = new MemoryStream( byteArrayIn );
			return (Bitmap)Bitmap.FromStream( ms );
		}
		catch( Exception ex ) {
			throw new Exception($"Unable to download {powerName}.", ex);
		}
	}

}
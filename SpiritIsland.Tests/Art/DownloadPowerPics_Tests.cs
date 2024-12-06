using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;

namespace SpiritIsland.Tests.Art;

public class DownloadPowerPics_Tests {

	[Theory(Skip ="Slow")]
	[InlineData( AssemblyType.BaseGame )]
	[InlineData( AssemblyType.BranchAndClaw )]
	[InlineData( AssemblyType.JaggedEarth )]
	[InlineData( AssemblyType.FeatherAndFlame )]
	public async Task CanDownloadCardImages( string edition ) {
		ImageDiskCache cache = new ImageDiskCache();

		Type refObject = AssemblyType.GetEditionType( edition );
		List<PowerCard> cards = [.. refObject.ScanForMajors()];
		cards.AddRange( refObject.ScanForMinors() );
		var spirits = refObject.ScanForSpirits();
		cards.AddRange( spirits.SelectMany( s => s.Hand ) );

		var failed = new List<string>();

		PowerCardResources x = ResourceImages.Singleton;
		foreach(var card in cards) {
			try {
				using System.Drawing.Image image = await x.GetPowerCardImage( card );
			}
			catch(HttpRequestException) {
				failed.Add( card.Title );
			}
		}

		failed.Join( "\r\n" ).ShouldBe( "" );

	}

//	[Theory()]
	[Theory(Skip ="takes too long to run")]
	[InlineData( AssemblyType.BaseGame )]
	[InlineData( AssemblyType.BranchAndClaw )]
	[InlineData( AssemblyType.JaggedEarth )]
	[InlineData( AssemblyType.FeatherAndFlame )]
	[InlineData( AssemblyType.NatureIncarnate )]
	public void DrawSpiritTokenImages( string edition ) {
		const string folder = "C:\\Users\\rettigcd\\Desktop\\spiritisland misc\\test_generated";
		Type refObject = AssemblyType.GetEditionType( edition );
		Spirit[] spirits = refObject.ScanForSpirits();
		foreach(var spirit in spirits) {
			using var bitmap = SpiritMarkerBuilder.BuildSpiritMarker( spirit, Img.Token_Presence, ResourceImages.Singleton);
#pragma warning disable CA1416 // Validate platform compatibility
			ImageDiskCache.SaveBmp(bitmap, $"{folder}\\{spirit.SpiritName}_ps.png", System.Drawing.Imaging.ImageFormat.Png );
#pragma warning restore CA1416 // Validate platform compatibility
//			ImageDiskCache.SaveBmp( bitmap, $"{folder}\\{spirit.Text}_loch.png", ImageFormat.Png );
			//ImageDiskCache.SaveBmp( bitmap, $"{folder}\\{spirit.Text}.png", ImageFormat.Png );
		}
	}

	[Fact(Skip ="Don't need to build cards.")]
	public void Build_SaltDeposits() {
		var card = NatureIncarnate.HabsburgMiningExpedition.SaltDepositDeckBuilder.SaltDeposits();
		card.Flipped = true;
		using var img = ResourceImages.Singleton.GetInvaderCard(card);
	}

	[Theory]
	[InlineData(0, 0, 0, "0 0 0")]            // black
	[InlineData(128, 128, 128, "0 0 50")]    // gray
	[InlineData(255, 255, 255, "0 0 100")]      // white
	[InlineData(255, 0, 0, "0 100 50")]        // red
	[InlineData(0, 255, 0, "120 100 50")]      // green
	[InlineData(0, 0, 255, "240 100 50")]      // blue
	[InlineData(255, 255, 0, "60 100 50")]     // yellow
	[InlineData(0, 255, 255, "180 100 50")]    // cyan
	[InlineData(255, 0, 255, "300 100 50")]    // magenta
	[InlineData(0, 1, 1, "180 100 0")]          // almost black, had rounding error that blew up Hue
	public void Rgb2Hsl_test(int red, int green, int blue, string expected) {
		var orig = System.Drawing.Color.FromArgb(red, green, blue);

		HSL hsl = HSL.FromRgb(orig);
		hsl.ToString().ShouldBe(expected);

		var result = hsl.ToRgb();

		result.ToString().ShouldBe(orig.ToString());
		result.R.ShouldBe(orig.R);
		result.G.ShouldBe(orig.G);
		result.B.ShouldBe(orig.B);
	}


}
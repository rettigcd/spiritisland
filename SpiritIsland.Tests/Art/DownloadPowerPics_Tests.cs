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
				using Image image = await x.GetPowerCardImage( card );
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
			ImageDiskCache.SaveBmp(bitmap, $"{folder}\\{spirit.SpiritName}_ps.png", ImageFormat.Png );
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

}
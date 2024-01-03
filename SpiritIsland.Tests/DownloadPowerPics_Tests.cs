using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;

namespace SpiritIsland.Tests.Core;

public class DownloadPowerPics_Tests {

	[Theory(Skip ="Slow")]
	[InlineData( AssemblyType.BaseGame )]
	[InlineData( AssemblyType.BranchAndClaw )]
	[InlineData( AssemblyType.JaggedEarth )]
	[InlineData( AssemblyType.FeatherAndFlame )]
	public async Task CanDownloadCardImages( string edition ) {
		ImageDiskCache cache = new ImageDiskCache();

		Type refObject = AssemblyType.GetEditionType( edition );
		List<PowerCard> cards = refObject.ScanForMajors().ToList();
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
				failed.Add( card.Name );
			}
		}

		failed.Join( "\r\n" ).ShouldBe( "" );

	}

	[Theory(Skip = "Takes >8 seconds to run.")]
	[InlineData( AssemblyType.BaseGame )]
	[InlineData( AssemblyType.BranchAndClaw )]
	[InlineData( AssemblyType.JaggedEarth )]
	[InlineData( AssemblyType.FeatherAndFlame )]
	[InlineData( AssemblyType.NatureIncarnate )]
	public async Task GenerateAll( string edition ) {
		Type refObject = AssemblyType.GetEditionType( edition );
		List<PowerCard> cards = refObject.ScanForMajors().ToList();
		cards.AddRange( refObject.ScanForMinors() );
		var spirits = refObject.ScanForSpirits();
		cards.AddRange( spirits.SelectMany( s => s.Hand ) );

		await GenerateCards( cards );
	}

	[Fact(Skip ="slow")]
	public async Task DrawCard() {

		var cards = new[] {
			PowerCard.For(typeof(LureOfTheUnknown)),
			PowerCard.For(typeof(ProwlingPanthers)),
			PowerCard.For(typeof(MeltEarthIntoQuicksand)),
			PowerCard.For(typeof(WeepForWhatIsLost)),
			PowerCard.For(typeof(CallToBloodshed)),
			//PowerCard.For<StranglingFirevine>(),
			//PowerCard.For<CleansingFloods>(),
			//PowerCard.For<PyroclasticFlow>(),
			//PowerCard.For<TheJungleHungers>(),
			//PowerCard.For<DryWoodExplodesInSmolderingSplinters>(),
			//PowerCard.For<ThicketsEruptWithEveryTouchOfBreeze>(),
			//PowerCard.For<FlashFloods>(),
			//PowerCard.For<RiversBounty>(),
			//PowerCard.For<BoonOfVigor>(),
			//PowerCard.For<WashAway>(),
			//PowerCard.For<UnlockTheGatesOfDeepestPower>(),
			//PowerCard.For<SoftlyBeckonEverInward>(),
			//PowerCard.For<SacrosanctWilderness>(),
			//PowerCard.For<ManifestationOfPowerAndGlory>(),
			//PowerCard.For<CastDownIntoTheBrinyDeep>(),
			//PowerCard.For<DreamOfTheUntouchedLand>(),
		};
		await GenerateCards( cards );
	}

	static async Task GenerateCards( IEnumerable<PowerCard> cards ) {
		foreach(var card in cards) {
			using Bitmap image = (Bitmap)await PowerCardImageBuilder.Build( card, ResourceImages.Singleton );
#pragma warning disable CA1416 // Validate platform compatibility
			ImageDiskCache.SaveBmp( image, $"C:\\users\\rettigcd\\desktop\\cards\\{card.Name}.png", ImageFormat.Png );
#pragma warning restore CA1416 // Validate platform compatibility
		}
	}

//	[Theory()]
	[Theory(Skip ="takes too long to run")]
	[InlineData( AssemblyType.BaseGame )]
	[InlineData( AssemblyType.BranchAndClaw )]
	[InlineData( AssemblyType.JaggedEarth )]
	[InlineData( AssemblyType.FeatherAndFlame )]
	[InlineData( AssemblyType.NatureIncarnate )]
	public void DrawTokenImages( string edition ) {
		const string folder = "C:\\Users\\rettigcd\\Desktop\\spiritisland misc\\test_generated";
		Type refObject = AssemblyType.GetEditionType( edition );
		Spirit[] spirits = refObject.ScanForSpirits();
		foreach(var spirit in spirits) {
			using var bitmap = SpiritMarkerBuilder.BuildSpiritMarker( spirit, Img.Token_Presence, ResourceImages.Singleton);
#pragma warning disable CA1416 // Validate platform compatibility
			ImageDiskCache.SaveBmp(bitmap, $"{folder}\\{spirit.Text}_ps.png", ImageFormat.Png );
#pragma warning restore CA1416 // Validate platform compatibility
//			ImageDiskCache.SaveBmp( bitmap, $"{folder}\\{spirit.Text}_loch.png", ImageFormat.Png );
			//ImageDiskCache.SaveBmp( bitmap, $"{folder}\\{spirit.Text}.png", ImageFormat.Png );
		}
	}


}
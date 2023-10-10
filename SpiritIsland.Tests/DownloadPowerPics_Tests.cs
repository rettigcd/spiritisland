using SpiritIsland.Utilities.ImageMgmt;
using SpiritIsland.WinForms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;

namespace SpiritIsland.Tests.Core;

public class DownloadPowerPics_Tests {

	[Theory]
	[InlineData( AssemblyType.BaseGame )]
	[InlineData( AssemblyType.BranchAndClaw )]
	[InlineData( AssemblyType.JaggedEarth )]
	[InlineData( AssemblyType.FeatherAndFlame )]
	public async Task CanDownloadCardImages( string edition ) {
		ImageCache cache = new ImageCache();

		Type refObject = AssemblyType.GetEditionType( edition );
		List<PowerCard> cards = refObject.GetMajors().ToList();
		cards.AddRange( refObject.GetMinors() );
		var spirits = refObject.GetSpirits();
		cards.AddRange( spirits.SelectMany( s => s.Hand ) );

		var failed = new List<string>();

		foreach(var card in cards) {
			try {
				using Image image = await ResourceImages.Singleton.CardCardImage( card );
			}
			catch(HttpRequestException ex) {
				failed.Add( card.Name );
			}
		}

		failed.Join( "\r\n" ).ShouldBe( "" );

	}

	[Theory]
	[InlineData( AssemblyType.BaseGame )]
	[InlineData( AssemblyType.BranchAndClaw )]
	[InlineData( AssemblyType.JaggedEarth )]
	[InlineData( AssemblyType.FeatherAndFlame )]
	public async Task GenerateAll( string edition ) {
		Type refObject = AssemblyType.GetEditionType( edition );
		List<PowerCard> cards = refObject.GetMajors().ToList();
		cards.AddRange( refObject.GetMinors() );
		var spirits = refObject.GetSpirits();
		cards.AddRange( spirits.SelectMany( s => s.Hand ) );

		await GenerateCards( cards );
	}

	[Fact]
	public async Task DrawCard() {

		var cards = new[] {
			PowerCard.For<LureOfTheUnknown>(),
			PowerCard.For<ProwlingPanthers>(),
			PowerCard.For<MeltEarthIntoQuicksand>(),
			PowerCard.For<WeepForWhatIsLost>(),
			PowerCard.For<CallToBloodshed>(),
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
			using Bitmap image = (Bitmap)await PowerCardImageManager.GetImage( card );
			ImageCache.SaveBmp( image, $"C:\\users\\rettigcd\\desktop\\cards\\{card.Name}.png", ImageFormat.Png );
		}
	}
}
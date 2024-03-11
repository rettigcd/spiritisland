using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.Versioning;
using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Art;

[SupportedOSPlatform( "windows" )]
public class ArtGen_Tests {

	[Theory( Skip = "Only used to generate images." )]
	// [Theory]
	[InlineData( AssemblyType.BaseGame )]
	[InlineData( AssemblyType.BranchAndClaw )]
	[InlineData( AssemblyType.JaggedEarth )]
	[InlineData( AssemblyType.FeatherAndFlame )]
	[InlineData( AssemblyType.NatureIncarnate )]
	public void Growth( string edition ) {
		Type refObject = AssemblyType.GetEditionType( edition );
		List<Spirit> spirits = [.. refObject.ScanForSpirits()];

		var growthActions = spirits
			.SelectMany( spirit => spirit.GrowthTrack.Groups )
			.SelectMany( go => go.GrowthActions )
			.ToArray();

		foreach(SpiritGrowthAction action in growthActions.Cast<SpiritGrowthAction>()) {

			Rectangle bounds = new Rectangle( 0, 0, 150, 200 );
			using Bitmap image = new Bitmap( bounds.Width, bounds.Height );
			using Graphics graphics = Graphics.FromImage( image );
			graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			GrowthActionBuilder.GetGrowthPaintable( action.Cmd )
				.Paint(graphics, bounds);
			string filename = action.Cmd.Description.ToResourceName( ".png" );
			ImageDiskCache.SaveBmp( image, $"C:\\users\\rettigcd\\desktop\\growth\\"+filename, ImageFormat.Png );
		}
	}


	//[Theory( Skip = "Takes >8 seconds to run." )]
	[Theory]
	[InlineData(AssemblyType.BaseGame)]
	[InlineData(AssemblyType.BranchAndClaw)]
	[InlineData(AssemblyType.JaggedEarth)]
	[InlineData(AssemblyType.FeatherAndFlame)]
	[InlineData(AssemblyType.NatureIncarnate)]
	public void PresenceTrack(string edition) {
		Type refObject = AssemblyType.GetEditionType(edition);
		List<Spirit> spirits = [.. refObject.ScanForSpirits()];

		foreach(var spirit in spirits) {
			var energySlots = spirit.Presence.Energy.Slots.ToArray();
			foreach (var slot in energySlots)
				SaveTrackImage(slot);
			foreach (var slot in spirit.Presence.CardPlays.Slots)
				SaveTrackImage(slot);
		}

	}

	static void SaveTrackImage(Track slot) {
		Rectangle bounds = new Rectangle(0, 0, 60, 80);
		using var image = new Bitmap(bounds.Width, bounds.Height);
		using var graphics = Graphics.FromImage(image);
		graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		new ImgRect(slot.Icon).Paint(graphics, bounds);
		string filename = slot.Code.ToResourceName(".png");
		ImageDiskCache.SaveBmp(image, $"C:\\users\\rettigcd\\desktop\\track\\" + filename, ImageFormat.Png);
	}

	[Fact(Skip="too slow")]
	public void Blight_Card(){
		using var Img = ResourceImages.Singleton.GetHealthBlightCard();
		using( ResourceImages.Singleton.GetBlightCard( new TheBorderOfLifeAndDeath() )) {}
		using( ResourceImages.Singleton.GetBlightCard( new UntendedLandCrumbles() ) ) {}
	}

	[Theory( Skip = "Takes >8 seconds to run." )]
	//[Theory]
	[InlineData( AssemblyType.BaseGame )]
	[InlineData( AssemblyType.BranchAndClaw )]
	[InlineData( AssemblyType.JaggedEarth )]
	[InlineData( AssemblyType.FeatherAndFlame )]
	[InlineData( AssemblyType.NatureIncarnate )]
	public async Task PowerCards_All( string edition ) {
		Type refObject = AssemblyType.GetEditionType( edition );
		List<PowerCard> cards = [.. refObject.ScanForMajors()];
		cards.AddRange( refObject.ScanForMinors() );
		var spirits = refObject.ScanForSpirits();
		cards.AddRange( spirits.SelectMany( s => s.Hand ) );

		await GenerateCards( cards );
	}



	//	[Theory(Skip = "Only needed for generating images for MAUI")]
	[Theory]
	[InlineData(AssemblyType.BaseGame)]
	[InlineData(AssemblyType.BranchAndClaw)]
	[InlineData(AssemblyType.JaggedEarth)]
	[InlineData(AssemblyType.FeatherAndFlame)]
	[InlineData(AssemblyType.NatureIncarnate)]
	public void PowerCards_Parts(string edition) {
		Type refObject = AssemblyType.GetEditionType(edition);
		List<PowerCard> cards = [.. refObject.ScanForMajors()];
		cards.AddRange(refObject.ScanForMinors());
		var spirits = refObject.ScanForSpirits();
		cards.AddRange(spirits.SelectMany(s => s.Hand));

		foreach (var card in cards)
			GenerateParts(card);

	}

	[Theory(Skip = "Takes >8 seconds to run.")]
//	[Theory]
	[InlineData( "2 icons" )]
	[InlineData( "no terrain" )]
	[InlineData( "ss + range" )]
	[InlineData( "another spirit" )]
	[InlineData( "any spirit" )]
	[InlineData( "single target" )]
	[InlineData( "explorers" )]
	[InlineData( "source filter" )]
	[InlineData( "bw icon + presence" )]
	[InlineData( "misc" )]
	[InlineData( "coastal" )]
	public async Task PowerCards_Specified(string test) {

		Type[] cardTypes = test switch {
			"2 icons" => [
				typeof(ADreadfulTideOfScurryingFlesh), 
				typeof(BargainOfCoursingPaths),
				typeof(FleshrotFever),
				typeof(AcceleratedRot),
			],
			"no terrain"     => [typeof(AbsoluteStasis)], // "Not Ocean"
			"ss + range"     => [
				typeof(AsphyxiatingSmoke),
				typeof(QuickenTheEarthsStruggles)
			],
			"another spirit" => [typeof(AbsorbEssence)],
			"any spirit"     => [typeof(BlazingRenewal)],
			"single target"  => [typeof(BargainsOfPowerAndProtection)],
			"explorers"      => [typeof(BlindingGlare)],
			"source filter"  => [
				typeof(CleansingFloods),typeof(TigersHunting),
				typeof(TheJungleHungers),  // Icon on Terrain
			],
			"bw icon + presence"=> [ // icons should be side by side
				typeof(BloodWaterAndBloodlust),
				typeof(CallToVigilance),
				typeof(CoordinatedRaid),
			],
			"misc"           => [
				typeof(BombardWithBouldersAndStingingSeeds),    // 2 source criteria

				typeof(ReachFromTheInfiniteDarkness), // YOURSELF
				typeof(SoftlyBeckonEverInward), // INLAND
				typeof(ThreateningFlames), // Blight + Invaders

				typeof(TreesRadiateCelestialBrilliance), // Blight Icon is too small
			],
			"coastal" => [
				typeof(SeaMonsters), // coastal or wetland
				typeof(CallOfTheDeeps), // COASTAL
				typeof(PlagueShipsSailToDistantPorts), // Coastal City
			],
			// dash...
			_ => []
		};

			// PowerCard.For(typeof(LureOfTheUnknown)),
			// PowerCard.For(typeof(ProwlingPanthers)),
			// PowerCard.For(typeof(MeltEarthIntoQuicksand)),
			// PowerCard.For(typeof(WeepForWhatIsLost)),
			// PowerCard.For(typeof(CallToBloodshed)),
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
		//];
		await GenerateCards( cardTypes.Select(type=>PowerCard.For(type)) );
	}

#pragma warning disable CA1416 // Validate platform compatibility

	static async Task GenerateCards( IEnumerable<PowerCard> cards ) {
		foreach(var card in cards) {
			using Bitmap image = (Bitmap)await PowerCardImageBuilder.Build( card, ResourceImages.Singleton );
			ImageDiskCache.SaveBmp( image, $"C:\\users\\rettigcd\\desktop\\cards\\{card.Title}.png", ImageFormat.Png );
		}
	}

	static void GenerateParts( PowerCard card) {
//		SaveAttributeToFile(PowerHeaderDrawer.Col2_SourceRange(card.RangeText), card.RangeText );
		SaveAttributeToFile(PowerHeaderDrawer.Col3_Target(card.TargetFilter), card.TargetFilter);
	}

	static void SaveAttributeToFile( IPaintableRect rect, string partText) {
		Rectangle bounds = new Rectangle(0, 0, 75, 25);
		using var image = new Bitmap(bounds.Width, bounds.Height);
		using var graphics = Graphics.FromImage(image);
		graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		rect.Paint(graphics, bounds);
		string filename = partText.ToResourceName(".png");
		ImageDiskCache.SaveBmp(image, $"C:\\users\\rettigcd\\desktop\\parts\\attr_" + filename, ImageFormat.Png);
	}


#pragma warning restore CA1416 // Validate platform compatibility


}
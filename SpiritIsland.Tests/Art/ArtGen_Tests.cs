using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Art;

public class ArtGen_Tests {

	[Fact(Skip="too slow")]
	public void Blight_Card(){
		using var Img = ResourceImages.Singleton.GetHealthBlightCard();
		#pragma warning disable CS0642 // Possible mistaken empty statement
		using( ResourceImages.Singleton.GetBlightCard( new TheBorderOfLifeAndDeath() ) );
		using( ResourceImages.Singleton.GetBlightCard( new UntendedLandCrumbles() ) );
		#pragma warning restore CS0642 // Possible mistaken empty statement
	}

	[Theory(Skip = "Takes >8 seconds to run.")]
//	[Theory]
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

	static async Task GenerateCards( IEnumerable<PowerCard> cards ) {
		foreach(var card in cards) {
			using Bitmap image = (Bitmap)await PowerCardImageBuilder.Build( card, ResourceImages.Singleton );
#pragma warning disable CA1416 // Validate platform compatibility
			ImageDiskCache.SaveBmp( image, $"C:\\users\\rettigcd\\desktop\\cards\\{card.Name}.png", ImageFormat.Png );
#pragma warning restore CA1416 // Validate platform compatibility
		}
	}

}
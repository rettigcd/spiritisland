using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using SpiritIsland.JaggedEarth;
using SpiritIsland.FeatherAndFlame;
using SpiritIsland.NatureIncarnate;

namespace SpiritIsland;

public class SpiritMarkerBuilder {

	static public Bitmap BuildSpiritMarker( Spirit spirit, Img img, ImgSource src ) {
		Bitmap image = src.GetImg( img );
		image = img switch {
			Img.Token_Presence => BuildPresence( image, spirit ),
			Img.Defend         => AdjustHsl( image, spirit ),
			Img.Isolate        => AdjustHsl( image, spirit ),
			_ => throw new ArgumentException( "invalid img value" )
		};
		return image;
	}


	static Bitmap BuildPresence( Bitmap shape, Spirit spirit ) {

		TokenAppearance appearance = GetApearanceForSpirit(spirit);

		// presenceAppearance.HslAdjustment.Adjust( bitmap );
		using Bitmap pattern = ResourceImages.Singleton.LoadSpiritImage( spirit.Text );
		SimpleMods.ColorOverlay( shape, pattern, appearance.PatternOffset );

		SimpleMods.BrightnessContrast_Photoshop( shape, appearance.Brightness, appearance.Contrast );

		return shape;
	}

	static Bitmap AdjustHsl( Bitmap bitmap, Spirit spirit ) {
		GetApearanceForSpirit( spirit ).HslAdjustment.Adjust( bitmap );
		return bitmap;
	}

	static TokenAppearance GetApearanceForSpirit( Spirit spirit ) {

		return spirit.Text switch {
			ASpreadOfRampantGreen.Name           => new TokenAppearance( 114, .65f, .45f ),
			Bringer.Name                         => new TokenAppearance( 300, .6f ).Offset( 180, 10 ).BC(-.5f,.48f),
			LightningsSwiftStrike.Name           => new TokenAppearance( 55, .64f ),
			Ocean.Name                           => new TokenAppearance( 200, .5f, .4f ),
			RiverSurges.Name                     => new TokenAppearance( 209, .5f, .4f ).Offset( 60, 54 ).BC(-.2f,.5f),
			Shadows.Name                         => new TokenAppearance( 337, .3f, .35f ).Offset( 80, 10 ).BC(-.3f,.72f ),
			Thunderspeaker.Name                  => new TokenAppearance( 0, .6f ).Offset( 100, 80 ),
			VitalStrength.Name                   => new TokenAppearance( 22, .47f, .35f ).Offset( 20, 60 ).BC(0,.72f ),
			//
			SharpFangs.Name                      => new TokenAppearance( 0, .8f, .35f ).BC(-.5f,.6f ),
			Keeper.Name                          => new TokenAppearance( 30, .3f, .5f ).Offset( 100, 10 ).BC(0,.65f * 1.2f ),
			//
			FracturedDaysSplitTheSky.Name        => new TokenAppearance( 160, .9f, .35f ),
			GrinningTricksterStirsUpTrouble.Name => new TokenAppearance( 58, .3f ).BC( .65f, .8f )/*.Offset(40,10)*/,
			LureOfTheDeepWilderness.Name         => new TokenAppearance( 125, .44f, .35f ).Offset(0,60).BC(-.3f,.6f),
			ManyMindsMoveAsOne.Name              => new TokenAppearance( 326, .35f ),
			ShiftingMemoryOfAges.Name            => new TokenAppearance( 180, .55f, .5f ).Offset( 60, 140 ).BC(0,.72f ),
			ShroudOfSilentMist.Name              => new TokenAppearance( 196, .3f, .65f ),
			StonesUnyieldingDefiance.Name        => new TokenAppearance( 30, .16f ),
			StarlightSeeksItsForm.Name           => new TokenAppearance( 251, .78f ),
			VengeanceAsABurningPlague.Name       => new TokenAppearance( 15, .6f ),
			VolcanoLoomingHigh.Name              => new TokenAppearance( 56, 1.0f, .35f ).Offset( 120, 20).BC(.3f,.84f ),
			//
			DownpourDrenchesTheWorld.Name        => new TokenAppearance(210, .7f, .35f).Offset(5, 10),
			FinderOfPathsUnseen.Name	         => new TokenAppearance( 218, .5f, .4f ).Offset( 80, 40 ).BC(0,.6f ),
			HeartOfTheWildfire.Name              => new TokenAppearance( 20, .8f ),
			SerpentSlumbering.Name               => new TokenAppearance( 330, .3f ).Offset(70,160).BC(.2f,.72f ),
			//
			BreathOfDarknessDownYourSpine.Name   => new TokenAppearance(170, .16f, .21f).Offset(80, 70).BC(0,.72f ),
			DancesUpEarthquakes.Name             => new TokenAppearance( 30, .16f ).Offset( 30, 40 ).BC(.5f,.6f ),
			EmberEyedBehemoth.Name               => new TokenAppearance( 45,  .4f, .5f ).Offset( 120, 110 ).BC(0,.65f * 1.2f ),
			HearthVigil.Name                     => new TokenAppearance( 6,   .4f, .63f ).Offset( 40, 40 ).BC(0,.72f ),
			RelentlessGazeOfTheSun.Name          => new TokenAppearance( 60,  .4f ).Offset( 10, 40 ).BC(-.4f,.6f ),
			ToweringRootsOfTheJungle.Name        => new TokenAppearance( 135, .22f, .35f ),
			WoundedWatersBleeding.Name           => new TokenAppearance( 270, .22f ).Offset( 100, 50 ).BC(0,.72f ),
			WanderingVoiceKeensDelirium.Name     => new TokenAppearance( 300, .22f, .5f ).Offset( 50, 70 ).BC(0,.84f ),

			//
			_                                    => new TokenAppearance( 0, 0 ),
		};
	}

}
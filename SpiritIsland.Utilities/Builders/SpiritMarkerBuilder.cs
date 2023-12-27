using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using SpiritIsland.JaggedEarth;
using SpiritIsland.FeatherAndFlame;
using SpiritIsland.NatureIncarnate;
using System.Drawing;

namespace SpiritIsland.WinForms;

class SpiritMarkerBuilder {

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

		PresenceTokenAppearance presenceAppearance = GetApearanceForSpirit(spirit);

		// presenceAppearance.HslAdjustment.Adjust( bitmap );
		using Bitmap pattern = ResourceImages.Singleton.LoadSpiritImage( spirit.Text );
		SimpleMods.ColorOverlay( shape, pattern, presenceAppearance.PatternOffset );
		SimpleMods.Contrast( shape, presenceAppearance.Contrast );

		return shape;
	}

	static Bitmap AdjustHsl( Bitmap bitmap, Spirit spirit ) {
		GetApearanceForSpirit( spirit ).HslAdjustment.Adjust( bitmap );
		return bitmap;
	}

	static PresenceTokenAppearance GetApearanceForSpirit( Spirit spirit ) {

		return spirit.Text switch {
			LightningsSwiftStrike.Name           => new PresenceTokenAppearance( 55, .64f ),
			VitalStrength.Name                   => new PresenceTokenAppearance( 22, .47f, .35f ) { PatternOffset = new Point( 20, 60 ), Contrast = .8f },
			Shadows.Name                         => new PresenceTokenAppearance( 337, .3f, .35f ) { PatternOffset = new Point( 80, 10 ), Contrast = .2f },
			RiverSurges.Name                     => new PresenceTokenAppearance( 209, .5f, .4f ) { PatternOffset = new Point( 60, 54 ) },
			Thunderspeaker.Name                  => new PresenceTokenAppearance( 0, .6f ),
			ASpreadOfRampantGreen.Name           => new PresenceTokenAppearance( 114, .65f, .45f ),
			Ocean.Name                           => new PresenceTokenAppearance( 200, .5f, .4f ),
			Bringer.Name                         => new PresenceTokenAppearance( 300, .6f ) { PatternOffset = new Point( 180, 10 ), Contrast = .4f },
			//
			SharpFangs.Name                      => new PresenceTokenAppearance( 0, .8f, .35f ),
			Keeper.Name                          => new PresenceTokenAppearance( 30, .3f, .5f ) { PatternOffset = new Point( 100, 10 ), Contrast = .9f },
			//
			StonesUnyieldingDefiance.Name        => new PresenceTokenAppearance( 30, .16f ),
			VengeanceAsABurningPlague.Name       => new PresenceTokenAppearance( 15, .6f ),
			VolcanoLoomingHigh.Name              => new PresenceTokenAppearance( 56, 1.0f, .35f ),
			GrinningTricksterStirsUpTrouble.Name => new PresenceTokenAppearance( 58, .3f ) { Contrast = .7f },
			LureOfTheDeepWilderness.Name         => new PresenceTokenAppearance( 125, .44f, .35f ) { PatternOffset = new Point(0,50) },
			FracturedDaysSplitTheSky.Name        => new PresenceTokenAppearance( 160, .9f, .35f ),
			ShroudOfSilentMist.Name              => new PresenceTokenAppearance( 196, .3f, .65f ),
			ShiftingMemoryOfAges.Name            => new PresenceTokenAppearance( 180, .55f, .5f ) { PatternOffset = new Point( 60, 140 ), Contrast = .8f },
			StarlightSeeksItsForm.Name           => new PresenceTokenAppearance( 251, .78f ),
			ManyMindsMoveAsOne.Name              => new PresenceTokenAppearance( 326, .35f ),
			//
			SerpentSlumbering.Name               => new PresenceTokenAppearance( 330, .3f ),
			HeartOfTheWildfire.Name              => new PresenceTokenAppearance( 20, .8f ),
			DownpourDrenchesTheWorld.Name        => new PresenceTokenAppearance(210, .7f, .35f) { PatternOffset = new Point(5, 10) },
			FinderOfPathsUnseen.Name	         => new PresenceTokenAppearance( 218, .5f, .4f ),
			//
			BreathOfDarknessDownYourSpine.Name => new PresenceTokenAppearance(170, .16f, .21f) { PatternOffset = new Point(80, 70), Contrast = .25f },
			ToweringRootsOfTheJungle.Name        => new PresenceTokenAppearance( 135, .22f, .35f ),
			HearthVigil.Name                     => new PresenceTokenAppearance( 6,    .4f, .63f ),

			WoundedWatersBleeding.Name           => new PresenceTokenAppearance( 270, .22f ) { PatternOffset = new Point( 100, 50 ), Contrast = .7f },
			DancesUpEarthquakes.Name             => new PresenceTokenAppearance( 30, .16f ) { PatternOffset = new Point( 30, 40 ), Contrast = .15f },
			RelentlessGazeOfTheSun.Name          => new PresenceTokenAppearance( 60,  .4f ),
			WanderingVoiceKeensDelirium.Name     => new PresenceTokenAppearance( 300, .22f, .5f ) { PatternOffset = new Point( 100, 70 ), Contrast = .9f },
			EmberEyedBehemoth.Name               => new PresenceTokenAppearance( 45,  .4f, .5f ) { Contrast = .75f },

			//
			_                                    => new PresenceTokenAppearance( 0, 0 ),
		};
	}

}
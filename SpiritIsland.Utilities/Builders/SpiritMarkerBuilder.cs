using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using SpiritIsland.JaggedEarth;
using SpiritIsland.FeatherAndFlame;
using SpiritIsland.NatureIncarnate;
using System.Drawing;

namespace SpiritIsland.WinForms;

class SpiritMarkerBuilder {

	static public Image BuildPresence( Spirit spirit ) {
		Bitmap bitmap = ResourceImages.Singleton.GetPresenceImage( "red" );

		PresenceTokenAppearance presenceAppearance = GetApearanceForSpirit(spirit);

		presenceAppearance.Adjustment?.Adjust( bitmap );

		using Bitmap pattern = ResourceImages.Singleton.LoadSpiritImage( spirit.Text );
		ColorOverlay(bitmap, pattern);

		return bitmap;
	}

	static public Image BuildDefend( Spirit spirit ) 
		=> BuildMarker(Img.Defend, GetApearanceForSpirit(spirit).Adjustment );
	static public Image BuildIsolate( Spirit spirit ) 
		=> BuildMarker(Img.Isolate, GetApearanceForSpirit(spirit).Adjustment );

	static public Image BuildMarker( Img img, BitmapAdjustment? adjustment ) {
		Bitmap bitmap = ResourceImages.Singleton.GetImage( img );
		adjustment?.Adjust( bitmap );
		return bitmap;
	}

	// Modifies the shape bitmap by Overlaying the pattern Color & (muted) Lightness
	static void ColorOverlay( Bitmap shape, Bitmap pattern ) {
		for(int x = 0; x < shape.Width; ++x)
			for(int y = 0; y < shape.Height; ++y) {

				// Shape
				Color shapeRgb = shape.GetPixel( x, y );
				HSL shapeHsl = HSL.FromRgb(shapeRgb);

				// Pattern
				Color patternRgb = Average( 
					pattern.GetPixel( 100+x*2,   100+y*2 ), 
					pattern.GetPixel( 100+x*2+1, 100+y*2 ),
					pattern.GetPixel( 100+x*2,   100+y*2+1 ), 
					pattern.GetPixel( 100+x*2+1, 100+y*2+1 )
				);

				HSL patternHsl = HSL.FromRgb(patternRgb);
				float mutedPatternL = (patternHsl.L+2f)/3;

				HSL colorOverlayHsl = new HSL( patternHsl.H, patternHsl.S, mutedPatternL * shapeHsl.L );
				Color colorOverlayRgb = Color.FromArgb( shapeRgb.A, colorOverlayHsl.ToRgb() );

				shape.SetPixel(x,y,Weight(colorOverlayRgb,2,shapeRgb,1));
			}

		static Color Weight(Color a, int aW,Color b, int bW) => Color.FromArgb(
			(a.A*aW+b.A*bW)/(aW+bW),
			(a.R*aW+b.R*bW)/(aW+bW),
			(a.G*aW+b.G*bW)/(aW+bW),
			(a.B*aW+b.B*bW)/(aW+bW)
		);

		static Color Average(params Color[] colors) {
			int r=0,g=0,b=0,a=0;
			foreach(Color c in colors) {
				r+=c.R;
				g+=c.G;
				b+=c.B;
				a+=c.A;
			}
			return Color.FromArgb(a/colors.Length,r/colors.Length,g/colors.Length,b/colors.Length);
		};

	}


	static PresenceTokenAppearance GetApearanceForSpirit( Spirit spirit ) {

		return spirit.Text switch {
			LightningsSwiftStrike.Name           => new PresenceTokenAppearance( 55, .64f ),
			VitalStrength.Name                   => new PresenceTokenAppearance( 22, .47f, .35f ),
			Shadows.Name                         => new PresenceTokenAppearance( 337, .3f, .35f ),
			RiverSurges.Name                     => new PresenceTokenAppearance( 209, .5f, .4f ),
			Thunderspeaker.Name                  => new PresenceTokenAppearance( 0, .6f ),
			ASpreadOfRampantGreen.Name           => new PresenceTokenAppearance( 114, .65f, .45f ),
			Ocean.Name                           => new PresenceTokenAppearance( 200, .5f, .4f ),
			Bringer.Name                         => new PresenceTokenAppearance( 300, .6f ),
			//
			SharpFangs.Name                      => new PresenceTokenAppearance( 0, .8f, .35f ),
			Keeper.Name                          => new PresenceTokenAppearance( 30, .3f, .5f ),
			//
			StonesUnyieldingDefiance.Name        => new PresenceTokenAppearance( 30, .16f ),
			VengeanceAsABurningPlague.Name       => new PresenceTokenAppearance( 15, .6f ),
			VolcanoLoomingHigh.Name              => new PresenceTokenAppearance( 56, 1.0f, .35f ),
			GrinningTricksterStirsUpTrouble.Name => new PresenceTokenAppearance( 58, .3f ),
			LureOfTheDeepWilderness.Name         => new PresenceTokenAppearance( 125, .44f, .35f ),
			FracturedDaysSplitTheSky.Name        => new PresenceTokenAppearance( 160, .9f, .35f ),
			ShroudOfSilentMist.Name              => new PresenceTokenAppearance( 196, .3f, .65f ),
			ShiftingMemoryOfAges.Name            => new PresenceTokenAppearance( 180/*229*/, .55f, .5f ),
			StarlightSeeksItsForm.Name           => new PresenceTokenAppearance( 251, .78f ),
			ManyMindsMoveAsOne.Name              => new PresenceTokenAppearance( 326, .35f ),
			//
			SerpentSlumbering.Name               => new PresenceTokenAppearance( 330, .3f ),
			HeartOfTheWildfire.Name              => new PresenceTokenAppearance( 20, .8f ),
			DownpourDrenchesTheWorld.Name        => new PresenceTokenAppearance( 210, .7f, .35f ),
			FinderOfPathsUnseen.Name	         => new PresenceTokenAppearance( 218, .5f, .4f ),
			//
			ToweringRootsOfTheJungle.Name        => new PresenceTokenAppearance( 135, .22f, .35f ),
			BreathOfDarknessDownYourSpine.Name   => new PresenceTokenAppearance( 170, .16f, .21f ),
			HearthVigil.Name                     => new PresenceTokenAppearance( 6,    .4f, .63f ),

			WoundedWatersBleeding.Name           => new PresenceTokenAppearance( 270, .22f ),
			DancesUpEarthquakes.Name             => new PresenceTokenAppearance( 30, .16f ),
			RelentlessGazeOfTheSun.Name          => new PresenceTokenAppearance( 60,  .4f ),
			WanderingVoiceKeensDelirium.Name     => new PresenceTokenAppearance( 300, .22f, .5f ),
			EmberEyedBehemoth.Name               => new PresenceTokenAppearance( 45,  .4f, .5f ),

			//
			_                                    => new PresenceTokenAppearance( 0, 0 ),
		};
	}

}
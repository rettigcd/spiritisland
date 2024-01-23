using SpiritIsland.Basegame;
using SpiritIsland.JaggedEarth;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;


public static class PaintableActionFactory {

	public static IPaintableRect GetGrowthPaintable( IActOn<Spirit> action ) {
		if(action is JaggedEarth.RepeatableSelfCmd repeatableActionFactory
					&& repeatableActionFactory.Inner is not JaggedEarth.GainTime
				)
			action = repeatableActionFactory.Inner;

		return action switch {
			ReclaimAll => new ImgRect( Img.ReclaimAll ),
			ReclaimN => new ImgRect( Img.Reclaim1 ),
			ReclaimHalf => new ImgRect( Img.ReclaimHalf ),
			GainPowerCard => new ImgRect( Img.GainCard ),
			GainEnergy { Delta: int delta } => new GainEnergyRect( delta ),
			PlacePresence => PlacePresenceRect( action ),
			AddDestroyedPresence => PlacePresenceRect( action ),
			MovePresence { Range: int range } => MovePresenceRect( range ),
			PlayExtraCardThisTurn { Count: int count } => AdditionalPlay( count ),
			GainAllElements { ElementsToGain: var els } => GainAllElementsRect( els ),
			Gain1Element { ElementOptions: var els } => Gain1ElementRect( els ),
			_ => null,
		} ?? action.Description switch {
			"Add a Presence or Disease" => PlacePresenceRect( action ),
			"PlacePresenceAndBeast" => PlacePresenceRect( action ),
			// Wounded Waters Bleeding
			"Add Destroyed Presence - Range 1" => PlacePresenceRect( action ),
			// Ocean
			"PlaceInOcean" => PlacePresenceRect( action ),
			"Gather 1 Presence into EACH Ocean" => new ImgRect( Img.GatherToOcean ),
			"Push Presence from Ocean" => new ImgRect( Img.Pushfromocean ),
			// Heart of the WildFire
			"EnergyForFire" => new ImgRect( Img.Oneenergyfire ),
			// Fractured Dates
			"GainTime(2)" => new ImgRect( Img.FracturedDays_Gain2Time ),
			"GainTime(1)x2" => new ImgRect( Img.FracturedDays_Gain1Timex2 ),
			"GainTime(1)x3" => new ImgRect( Img.FracturedDays_Gain1Timex3 ),
			"DrawPowerCardFromDaysThatNeverWere" => new ImgRect( Img.FracturedDays_DrawDtnw ),
			// Starlight Seeks Its Form
			"MakePowerFast" => new ImgRect( Img.Icon_Fast ),
			// Grinning Trickster
			"GainEnergyEqualToCardPlays" => new ImgRect( Img.GainEnergyEqualToCardPlays ),
			// Many Minds
			"Gather1Token" => new ImgRect( Img.Land_Gather_Beasts ),
			"ApplyDamage" => new ImgRect( Img.Damage_2 ),
			"Discard 2 Power Cards" => new ImgRect( Img.Discard2 ),
			// Towering Roots
			"AddVitalityToIncarna" => AddVitalityToIncarna(),
			"ReplacePresenceWithIncarna" => ReplacePresenceWithIncarna(),
			// Finder
			"IgnoreRange" => Draw_IgnoreRange(),
			// Relentless Gaze of the Sun
			"Gain Energy an additional time" => GainEnergyAgain(),
			"Move up to 3 Presence together" => MoveUpTo3PresenceTogether(),
			// Dances up Earthquakes
			"AddPresenceOrGainMajor" => AddPresenceOrGainMajor(),
			"AccelerateOrDelay" => AccelerateOrDelay(),
			// Breath of Darkness
			"All pieces Escape" => PiecesEscape( int.MaxValue ),
			"1 pieces Escape" => PiecesEscape( 1 ),
			"2 pieces Escape" => PiecesEscape( 2 ),
			"Move Incarna anywhere" => AddOrMoveIncarnaAnywhere(),
			"Add or Move Incarna to Presence" => AddOrMoveIncarnaToPresence(),
			// Ember Eyed
			"Discard a Power Card with fire" => DiscardCardWithFire(),
			"Reclaim All with Fire" => ReclaimAllWithFire(),
			"Empower Incarna" => new ImgRect( Img.EEB_Incarna_Empowered ), // EEB is the only one in growth
			"Move Incarna - Range 1" => MovePresenceRect( 1, Img.Icon_Incarna ),
			_ => new TextRect( action.Description ),
		};
	}

	static IconDescriptorRect AddVitalityToIncarna() {
		var des = new IconDescriptor { ContentImg = Img.Icon_Vitality, Sub = new IconDescriptor { ContentImg = Img.Icon_Incarna } };
		return new IconDescriptorRect( des );
	}

	static IconDescriptorRect GainAllElementsRect( params Element[] elements ) {
		var descriptor = new IconDescriptor();
		if(0 < elements.Length)
			descriptor.ContentImg = elements[0].GetTokenImg();
		if(1 < elements.Length)
			descriptor.ContentImg2 = elements[1].GetTokenImg();
		return new IconDescriptorRect( descriptor );
	}

	static PoolRect DiscardCardWithFire() {
		return new PoolRect()
			.Float( new ImgRect( Img.Discard1 ), .05f,.05f,.9f,.9f)
			.Float( new ImgRect( Img.Token_Fire ), .6f,0f,.4f,.4f );
	}

	static PoolRect ReclaimAllWithFire() {
		return new PoolRect()
			.Float( new ImgRect( Img.ReclaimAll ), .0f, .0f, 1f, 1f )
			.Float( new ImgRect( Img.Token_Fire ), .6f, 0f, .4f, .4f );
	}

	static VerticalStackRect PiecesEscape( int number ) {
		return new VerticalStackRect(
			new NullRect(),
			new ImgRect( Img.Icon_EndlessDark ),
			new TextRect( number != int.MaxValue ? number.ToString() : "∞" ),
			new ImgRect( Img.MoveArrow )
		).SplitByWeight( .05f, .05f /*null*/, .5f /*presence*/, .3f/*number*/, .1f/*arrow*/, .05f );
	}

	static VerticalStackRect AddOrMoveIncarnaAnywhere() {
		return new VerticalStackRect(
			new NullRect(),
			new ImgRect( Img.Icon_Incarna ),
			new ImgRect( Img.MoveArrow )
		).SplitByWeight( .05f, .05f /*null*/, .8f /*incarna*/, .1f/*arrow*/, .05f );
	}

	static VerticalStackRect AddOrMoveIncarnaToPresence() {
		return new VerticalStackRect(
			new NullRect(),
			new ImgRect( Img.Icon_Incarna ),
			new ImgRect( Img.Icon_Presence ),
			new ImgRect( Img.MoveArrow )
		).SplitByWeight( .05f, .05f /*null*/, .6f /*presence*/, .2f/*filter*/, .1f/*arrow*/, .05f );
	}

	static PoolRect AddPresenceOrGainMajor() {
		return new PoolRect()
			.Float( new TextRect( "/" ), .2f, .2f, .6f, .6f )
			.Float( PlacePresenceRect( new PlacePresence( 2 ) ), 0f, 0f, .5f, .5f )
			.Float( new ImgRect( Img.GainCard ), .5f, .5f, .5f, .5f )
			.Float( new ImgRect( Img.Icon_Major ), .55f, .5f, .2f, .2f );
	}

	static PoolRect AccelerateOrDelay() {
		return new PoolRect()
			.Float( new ImgRect( Img.ImpendingCard ), .1f, .2f, .4f, .5f )
			.Float( new ImgRect( Img.ImpendingCard ), .5f, .2f, .4f, .5f )
			.Float( new ImgRect( Img.Coin ), .0f, .0f, .3f, .3f )
			.Float( new TextRect( "±1" ), .05f, .1f, .2f, .1f );
	}

	static PoolRect ReplacePresenceWithIncarna() {
		return new PoolRect()
			.Float( new ImgRect( Img.Icon_Incarna ), .1f, 0f, .8f, .8f )
			.Float( new ImgRect( Img.Icon_Presence ), .6f, .6f, .4f, .4f )
			.Float( new ImgRect( Img.DestroyedX ), .6f, .7f, .2f, .2f );
			// .Float( new ImgRect( Img.Icon_DestroyedPresence ), .6f, .6f, .4f, .4f );  // This indicates already-destroyed presence
	}

	static PoolRect GainEnergyAgain() {
		return new PoolRect()
			.Float( new ImgRect( Img.Coin ), .1f, .1f,.5f, .5f )
			.Float( new ImgRect( Img.Coin ), .4f, .1f, .5f, .5f )
			.Float( new TextRect( "x2" ), .0f, .25f, 1f, .25f );
	}

	static VerticalStackRect MoveUpTo3PresenceTogether() {
		return new VerticalStackRect(
			new NullRect(),
			new VerticalStackRect(
				new ImgRect( Img.Icon_Presence ),
				new HorizontalStackRect(
					new ImgRect( Img.Icon_Presence ),
					new ImgRect( Img.Icon_Presence )
				)
			),
			new NullRect(),
			new TextRect( 3 ),
			new ImgRect( Img.MoveArrow )
		).SplitByWeight( .0f, .1f/*spacer*/, .35f, .05f /*spacer*/, .35f /*number*/, .1f/*arrow*/, .1f );
	}

	static VerticalStackRect Add3DestroyedPresenceTogether() {
		return new VerticalStackRect(
			new NullRect(),
			new HorizontalStackRect(
				new TextRect("+"),
				new VerticalStackRect(
					new ImgRect( Img.Icon_DestroyedPresence ),
					new HorizontalStackRect(
						new ImgRect( Img.Icon_DestroyedPresence ),
						new ImgRect( Img.Icon_DestroyedPresence )
					)
				)
			).SplitByWeight(0f,.15f,.85f),
			new NullRect(),
			new TextRect( 1 ),
			new ImgRect( Img.RangeArrow )
		)	.SplitByWeight( .0f, .05f/*spacer*/, .4f, .05f /*spacer*/, .35f /*number*/, .1f/*arrow*/, .1f );
	}

	static PoolRect AdditionalPlay( int count ) {
		string txt = (count > 0)
			? ("+" + count.ToString())
			: ("\u2014" + (-count).ToString());
		return new PoolRect()
			.Float( new ImgRect(Img.CardPlayPlusN), 0f,0f,1f,1f)
			.Float( new TextRect(txt){ ScaleFont = .5f }, .2f,.2f,.6f,.6f);
	}

	static HorizontalStackRect Gain1ElementRect( params Element[] elements ) {
		return new HorizontalStackRect(
			elements.Select(el=>new ImgRect(el.GetIconImg())).ToArray()
		);
	}

	static IPaintableRect GetTargetFilterIcon( string filterEnum ) {

		string[] orParts = filterEnum.Split("Or");
		if(orParts.Length == 2) {
			return new PoolRect()
				.Float( GetTargetFilterIcon( orParts[0] ), 0f, 0f, .5f, 1f )
				.Float( new TextRect( "/" ), .4f, 0f, .2f, 1f )
				.Float( GetTargetFilterIcon( orParts[1] ), .5f, 0f, .5f, 1f );
		}

		Img img = GetImgEnum( filterEnum );
		return img == Img.None 
			? null 
			: new ImgRect( img );
	}

	static Img GetImgEnum( string filterEnum ) {
		Img img = filterEnum switch {
			Filter.Jungle							=> Img.Icon_Jungle,
			Filter.Presence							=> Img.Icon_Presence,
			Filter.Wetland							=> Img.Icon_Wetland,
			Filter.Mountain							=> Img.Icon_Mountain,
			Filter.Wilds							=> Img.Icon_Wilds,
			Filter.Beast							=> Img.Icon_Beast,		
			Filter.Dahan							=> Img.Icon_Dahan,
			Filter.Invaders							=> Img.Icon_Invaders,
			Filter.Coastal                         => Img.Icon_Coastal,
			Filter.NoBlight                        => Img.Icon_NoBlight,
			Filter.Ocean                           => Img.Icon_Ocean,
			_ => Img.None, // Inland, Any
		};
		return img;
	}

	static VerticalStackRect MovePresenceRect( int range, Img img = Img.Icon_Presence ) {
		return new VerticalStackRect(
			new NullRect(),
			new ImgRect( img ),
			new TextRect( range ),
			new ImgRect( Img.MoveArrow )
		)	.SplitByWeight(.05f, .1f, .3f, .35f, .15f, .1f);
	}

	static IPaintableRect PlacePresenceRect( IActOn<Spirit> growth ) {


		var (presImg, range, filterEnum, addOnIcon, num) = growth switch {
			PlaceInOcean            => (Img.Icon_Presence, null, Filter.Ocean, Img.None, 1),
			PlacePresenceAndBeast   => (Img.Icon_Presence, (int?)3, Filter.Any, Img.Beast, 1), // add an icon
			{ Description: string n } when n == "Add a Presence or Disease"
									=> (Img.Icon_Presence, (int?)1, Filter.Any, Img.Disease, 1),
			AddDestroyedPresence { Range: int r, NumToPlace: int ntp }
									=> (Img.Icon_DestroyedPresence, (int?)r, Filter.Any, Img.None, ntp),
			// generic, do last
			PlacePresence { Range: int r, FilterDescription: string f }
									=> (Img.Icon_Presence, (int?)r, f, Img.None, 1),
			PlacePresence { FilterDescription: string f }
									=> (Img.Icon_Presence, null, f, Img.None, 1),
			_ => throw new ArgumentException( "growth action factory not a place-presence", nameof( growth ) ),
		};

		if(presImg == Img.Icon_DestroyedPresence && num == 3) // "Add up to 3 Destroyed Presence - Range 1"
			return Add3DestroyedPresenceTogether(); //!! merge these methods together

		// Filter
		IPaintableRect filterImgRect = GetTargetFilterIcon( filterEnum );
		float filterWeight = .25f;
		if(filterImgRect == null){ filterWeight=.01f; filterImgRect = new NullRect(); }

		List<IPaintableRect> rects = [new NullRect(),PlacePresenceRect( presImg ), filterImgRect];
		List<float> weights = [0f,.35f,filterWeight];
		
		// Range
		if(range.HasValue)
			rects.Add( new TextRect( range.Value ) ); weights.Add(.3f);
		rects.Add( new ImgRect( Img.RangeArrow ) ); weights.Add(.1f);

		// Top/Bottom Margins
		float spacer = Math.Max(.15f, (1f - weights.Sum())*.5f);
		weights[0] = spacer;
		rects.Add( new NullRect() ); weights.Add(spacer);		

		IPaintableRect paintable = new VerticalStackRect([..rects]).SplitByWeight(0,[..weights]);

		return addOnIcon == Img.None ? paintable
			: new PoolRect()
				.Float(paintable,0f,0f,1f,1f)
				.Float(new ImgRect( addOnIcon ), .2f, .2f, .6f, .6f );
	}

	static HorizontalStackRect PlacePresenceRect(Img img) {
		return new HorizontalStackRect(
					new NullRect(),
					new TextRect( "+" ),
					new ImgRect( img )
				).SplitByWeight( 0f, .15f, .15f, .5f, .2f );
	}

	static VerticalStackRect Draw_IgnoreRange()  {
		return new VerticalStackRect(
			new NullRect(),
			new ImgRect(Img.Icon_Checkmark),
			new ImgRect(Img.RangeArrow)
		).SplitByWeight(0.05f,.1f,.8f,.1f,.1f);
	}

	class GainEnergyRect( int delta ) : IPaintableRect {
		readonly int _delta = delta;
		public Rectangle Paint( Graphics graphics, Rectangle bounds ) {
			var fitted = new ImgRect( Img.Coin ).Paint( graphics, bounds );

			// Text
			using Font coinFont = ResourceImages.Singleton.UseGameFont( fitted.Height * .5f );
			string txt = 0 < _delta
				? ("+" + _delta.ToString())
				: ("\u2014" + (-_delta).ToString());
			SizeF textSize = graphics.MeasureString( txt, coinFont );
			PointF textTopLeft = new PointF(
				bounds.X + (bounds.Width - textSize.Width) * .35f,
				bounds.Y + (bounds.Height - textSize.Height) * .60f
			);
			graphics.DrawString( txt, coinFont, Brushes.Black, textTopLeft );
			return fitted;
		}

	}

}
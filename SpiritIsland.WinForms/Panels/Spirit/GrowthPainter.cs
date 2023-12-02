using SpiritIsland.Basegame;
using SpiritIsland.JaggedEarth;
using System;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

public sealed class GrowthPainter : IDisposable{

	readonly GrowthLayout _layout;

	Graphics _graphics; // single-threaded variables
	IconDrawer _iconDrawer;
	Bitmap _cachedImageLayer;

	public GrowthPainter( GrowthLayout layout ) {
		_layout = layout;
	}

	public void Paint( Graphics graphics, bool addBackground ) {

		_cachedImageLayer ??= BuildBackgroundImage();

		if(addBackground) {
			using var bgBrush = new SolidBrush( Color.FromArgb( 220, Color.LightYellow ) );
			graphics.FillRectangle( bgBrush, _layout.Bounds);
			graphics.DrawRectangle( Pens.Black, _layout.Bounds );
		}

		graphics.DrawImage( _cachedImageLayer, _layout.Bounds );

	}

	Bitmap BuildBackgroundImage() {
		using var optionPen = new Pen( Color.Blue, 6f );

		var cachedImageLayer = new Bitmap( _layout.Bounds.Width, _layout.Bounds.Height );
		using var g = Graphics.FromImage( cachedImageLayer );
		_iconDrawer = new IconDrawer( g, new ImgMemoryCache() );
		g.TranslateTransform( -_layout.Bounds.X, -_layout.Bounds.Y );
		g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

		// Growth - Dividers
		bool first = true;
		foreach(var (opt, rect) in _layout.EachGrowth())
			if(first)
				first = false;
			else
				g.DrawLine( optionPen, rect.Left, rect.Top, rect.Left, rect.Bottom );

		_graphics = g;

		// Growth Actions
		foreach(var (action, rect) in _layout.EachAction())
			DrawAction( action, rect );
		return cachedImageLayer;
	}

	void DrawAction( IHelpGrow growthAction, RectangleF rect ) {
		if(growthAction is not SpiritGrowthAction sga) return;
		IActOn<Spirit> action = sga.Cmd;

		if(action is JaggedEarth.RepeatableSelfCmd repeatableActionFactory
			&& repeatableActionFactory.Inner is not JaggedEarth.GainTime
		)
			action = repeatableActionFactory.Inner; // !!! This won't draw until sga.Cmd is an IActOn<Spirit>

		var paintable = action switch {
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
			"Add a Presence or Disease"               => PlacePresenceRect( action ),
			"PlacePresenceAndBeast"                   => PlacePresenceRect( action ),
			// Wounded Waters Bleeding
			"Add Destroyed Presence - Range 1"        => PlacePresenceRect( action ),
			// Ocean
			"PlaceInOcean"                            => PlacePresenceRect( action ),
			"Gather 1 Presence into EACH Ocean"       => new ImgRect( Img.GatherToOcean ),
			"Push Presence from Ocean"                => new ImgRect( Img.Pushfromocean ),
			// Heart of the WildFire
			"EnergyForFire"                           => new ImgRect( Img.Oneenergyfire ),
			// Fractured Dates
			"GainTime(2)"                             => new ImgRect( Img.FracturedDays_Gain2Time ),
			"GainTime(1)x2"                           => new ImgRect( Img.FracturedDays_Gain1Timex2 ),
			"GainTime(1)x3"                           => new ImgRect( Img.FracturedDays_Gain1Timex3 ),
			"DrawPowerCardFromDaysThatNeverWere"      => new ImgRect( Img.FracturedDays_DrawDtnw ),
			// Starlight Seeks Its Form
			"MakePowerFast"                           => new ImgRect( Img.Icon_Fast ),
			// Grinning Trickster
			"GainEnergyEqualToCardPlays"              => new ImgRect( Img.GainEnergyEqualToCardPlays ),
			// Many Minds
			"Gather1Token"                            => new ImgRect( Img.Land_Gather_Beasts ),
			"ApplyDamage"                             => new ImgRect( Img.Damage_2 ),
			"Discard 2 Power Cards"                   => new ImgRect( Img.Discard2 ),
			// Towering Roots
			"AddVitalityToIncarna"                    => AddVitalityToIncarna(),
			"ReplacePresenceWithIncarna"              => ReplacePresenceWithIncarna(),
			// Finder
			"IgnoreRange"                             => Draw_IgnoreRange(),
			// Relentless Gaze of the Sun
			"Gain Energy an additional time"          => GainEnergyAgain(),
			"Move up to 3 Presence together"          => MoveUpTo3PresenceTogether(),
			// Dances up Earthquakes
			"AddPresenceOrGainMajor"                  => AddPresenceOrGainMajor(),
			"AccelerateOrDelay"                       => AccelerateOrDelay(),
			// Breath of Darkness
			"All pieces Escape"                       => PiecesEscape( int.MaxValue ),
			"1 pieces Escape"                         => PiecesEscape( 1 ),
			"2 pieces Escape"                         => PiecesEscape( 2 ),
			"Move Incarna anywhere"                   => AddOrMoveIncarnaAnywhere(),
			"Add or Move Incarna to Presence"         => AddOrMoveIncarnaToPresence(),
			// Ember Eyed
			"Discard a Power Card with fire"          => DiscardCardWithFire(),
			"Reclaim All with Fire"                   => ReclaimAllWithFire(),
			"Empower Incarna"                          => new ImgRect(Img.EEB_Incarna_Empowered), // EEB is the only one in growth
			"Move Incarna - Range 1"                  => MovePresenceRect(1, Img.Icon_Incarna),
			_                                         => null,
		};

		if(paintable is not null) {
			paintable.Paint( _graphics, rect.ToInts() );
		} else {
			Rectangle r2 = rect.ToInts().InflateBy( -5, -5 );
			_graphics.FillRectangle( Brushes.Goldenrod, r2 );
			_graphics.DrawString(action.Description,SystemFonts.MessageBoxFont, Brushes.Black, r2 );
		}

	}

	static IPaintableRect DiscardCardWithFire() {
		return new PoolRect()
			.Float( new ImgRect( Img.Discard1 ), .05f,.05f,.9f,.9f)
			.Float( new ImgRect( Img.Token_Fire ), .6f,0f,.4f,.4f );
	}

	static IPaintableRect ReclaimAllWithFire() {
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

	private static PoolRect ReplacePresenceWithIncarna() {
		return new PoolRect()
							.Float( new ImgRect( Img.Icon_Incarna ), .1f, 0f, .8f, .8f )
							.Float( new ImgRect( Img.Icon_Presence ), .6f, .6f, .4f, .4f )
							.Float( new ImgRect( Img.RedX ), .6f, .6f, .4f, .4f );
	}

	IPaintableRect AddVitalityToIncarna() {
		var des = new IconDescriptor { ContentImg = Img.Icon_Vitality, Sub = new IconDescriptor { ContentImg = Img.Icon_Incarna } };
		return new IconDescriptorRect( _iconDrawer, des );
	}

	static IPaintableRect GainEnergyAgain() {
		return new PoolRect()
			.Float( new ImgRect( Img.Coin ), .1f, .1f,.5f, .5f )
			.Float( new ImgRect( Img.Coin ), .4f, .1f, .5f, .5f )
			.Float( new TextRect( "x2" ), .0f, .25f, 1f, .25f );
	}

	static IPaintableRect MoveUpTo3PresenceTogether() {
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

	static IPaintableRect Add3DestroyedPresenceTogether() {
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

	static IPaintableRect AdditionalPlay( int count ) {
		string txt = (count > 0)
			? ("+" + count.ToString())
			: ("\u2014" + (-count).ToString());
		return new PoolRect()
			.Float( new ImgRect(Img.CardPlayPlusN), 0f,0f,1f,1f)
			.Float( new TextRect(txt), .2f,.2f,.6f,.6f);
	}

	static IPaintableRect Gain1ElementRect( params Element[] elements ) {
		return new HorizontalStackRect(
			elements.Select(el=>new ImgRect(el.GetIconImg())).ToArray()
		);
	}

	IPaintableRect GainAllElementsRect( params Element[] elements ) {
		var descriptor = new IconDescriptor();
		if(0 < elements.Length )
			descriptor.ContentImg = elements[0].GetTokenImg();
		if(1 < elements.Length)
			descriptor.ContentImg2 = elements[1].GetTokenImg();
		return new IconDescriptorRect(_iconDrawer,descriptor);
	}

	class IconDescriptorRect : IPaintableRect {
		readonly IconDrawer _iconDrawer;
		readonly IconDescriptor _descriptor;
		public IconDescriptorRect(IconDrawer iconDrawer,IconDescriptor descriptor ) {
			_iconDrawer = iconDrawer;
			_descriptor = descriptor;
		}
		public Rectangle Paint( Graphics graphics, Rectangle rect ) {
			_iconDrawer.DrawTheIcon( _descriptor, rect );
			return rect;
		}
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

	static IPaintableRect MovePresenceRect( int range, Img img = Img.Icon_Presence ) {
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
			_ => throw new ArgumentException( "growth action factory not a place-presence", nameof( growth ) ),
		};

		if(presImg == Img.Icon_DestroyedPresence && num == 3) // "Add up to 3 Destroyed Presence - Range 1"
			return Add3DestroyedPresenceTogether(); //!! merge these methods together

		var filterImgRect = GetTargetFilterIcon( filterEnum );

		IPaintableRect paintable;
		if(range is null )
			// Ocean
			paintable = new VerticalStackRect(
				new NullRect(),
				PlacePresenceRect( presImg ),
				filterImgRect
			).SplitByWeight( .05f, .1f, .3f, .5f, .1f );
			// HeathVigil, : Keeper
        else paintable = filterImgRect != null
			? new VerticalStackRect(
				new NullRect(),
				PlacePresenceRect( presImg ),
				filterImgRect,
				new TextRect( range ),
				new ImgRect( Img.RangeArrow )
			).SplitByWeight( .05f, .01f /*null*/, .3f /*presence*/, .25f/*filter*/, .25f/*number*/, .1f/*arrow*/, .05f )
			: (IPaintableRect)new VerticalStackRect(
				new NullRect(),
				PlacePresenceRect( presImg ),
				new NullRect(),
				new TextRect( range ),
				new ImgRect( Img.RangeArrow )
			).SplitByWeight( .0f, .05f/*spacer*/, .35f, .01f /*spacer*/, .4f /*number*/, .1f/*arrow*/, .1f );

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

	static IPaintableRect Draw_IgnoreRange()  {
		return new VerticalStackRect(
			new NullRect(),
			new ImgRect(Img.Icon_Checkmark),
			new ImgRect(Img.RangeArrow)
		).SplitByWeight(0.05f,.1f,.8f,.1f,.1f);
	}

	public void Dispose() {
		_cachedImageLayer?.Dispose();
	}

}


class GainEnergyRect : IPaintableRect {
	public GainEnergyRect( int delta ) { _delta = delta; }
	readonly int _delta;
	public Rectangle Paint(Graphics graphics, Rectangle bounds ){
		var fitted = new ImgRect(Img.Coin).Paint(graphics,bounds);

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

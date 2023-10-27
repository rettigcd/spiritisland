using SpiritIsland.Basegame;
using SpiritIsland.JaggedEarth;
using SpiritIsland.NatureIncarnate;
using System;
using System.Drawing;
namespace SpiritIsland.WinForms; 

public sealed class GrowthPainter : IDisposable{

	readonly GrowthLayout _layout;

	Graphics _graphics; // single-threaded variables
	IconDrawer iconDrawer;
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
		iconDrawer = new IconDrawer( g, new CachedImageDrawer() );
		g.TranslateTransform( -_layout.Bounds.X, -_layout.Bounds.Y );
		g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

		// Growth - Dividers
		bool first = true;
		foreach(var (opt, rect) in _layout.EachGrowth())
			if(first)
				first = false;
			else
				g.DrawLine( optionPen, rect.Left, rect.Top, rect.Left, rect.Bottom );

		this._graphics = g;

		// Growth Actions
		foreach(var (action, rect) in _layout.EachAction())
			DrawAction( action, rect );
		return cachedImageLayer;
	}

	void DrawAction( GrowthActionFactory action, RectangleF rect ) {

		if(action is JaggedEarth.RepeatableActionFactory repeatableActionFactory 
			&& repeatableActionFactory.Inner is not JaggedEarth.GainTime
		)
			action = repeatableActionFactory.Inner;

		if(action is GainEnergy ge) { GainEnergy( rect, ge.Delta ); return; }

		if(action is ReclaimAll) { DrawIconInCenter( rect, Img.ReclaimAll ); return; }

		if(action is ReclaimN) { DrawIconInCenter( rect, Img.Reclaim1 ); return; }

		if(action is ReclaimHalf) {  DrawIconInCenter( rect, Img.ReclaimHalf ); return; }

		if(action is DrawPowerCard) { DrawIconInCenter( rect, Img.GainCard ); return; }

		if(action is PlacePresence pp ) { PlacePresence( rect, pp ); return; }

		if(action is MovePresence mp) { MovePresence( rect, mp.Range ); return; }

		switch(action.Name) {

			case "PlayExtraCardThisTurn(2)": AdditionalPlay( rect, 2 ); break;
			case "PlayExtraCardThisTurn(1)": AdditionalPlay( rect, 1 ); break;
			// Ocean
			case "PlaceInOcean":            PlacePresence( rect, action ); break;
			case "PlacePresenceAndBeast":   PlacePresence( rect, action ); break;

			case "GatherPresenceIntoOcean": DrawIconInCenter( rect, Img.GatherToOcean ); break;
			case "PushPresenceFromOcean":   DrawIconInCenter( rect, Img.Pushfromocean ); break;
			// Heart of the WildFire
			case "EnergyForFire": DrawIconInCenter( rect, Img.Oneenergyfire ); break;
			// Lure of the Deep Wilderness
			case "GainElement(Moon,Air,Plant)": GainElement( rect.ToInts(), Element.Moon, Element.Air, Element.Plant ); break;
			// Fractured Dates
			case "GainElement(Air)": GainElement( rect.ToInts(), Element.Air ); break;
			case "GainElement(Moon)": GainElement( rect.ToInts(), Element.Moon ); break;
			case "GainElement(Sun)": GainElement( rect.ToInts(), Element.Sun ); break;
			case "GainTime(2)":    GainTime( rect ); break;
			case "GainTime(1)x2":  Gain1TimeOr2CardPlaysX2( rect ); break;
			case "GainTime(1)x3":  Gain1TimeOr2EnergyX3( rect ); break;
			case "DrawPowerCardFromDaysThatNeverWere": DrawImage( rect, Img.FracturedDays_DrawDtnw ); break; 
			// Starlight Seeks Its Form
			case "MakePowerFast": DrawIconInCenter( rect, Img.Icon_Fast ); break;
			// Grinning Trickster
			case "GainEnergyEqualToCardPlays": DrawIconInCenter( rect, Img.GainEnergyEqualToCardPlays ); break;
			// Stones Unyielding Defiance
			case "GainElements(Earth,Earth)":
				iconDrawer.DrawTheIcon(
					new IconDescriptor { ContentImg = Img.Token_Earth, ContentImg2 = Img.Token_Earth, }, 
					rect
				);
				break; // !!! this is drawn as an OR, layer them and make them an AND
			case "GainElements(Water,Water)":
				iconDrawer.DrawTheIcon(
					new IconDescriptor { ContentImg = Img.Token_Water, ContentImg2 = Img.Token_Water, },
					rect
				);
				break; // !!! this is drawn as an OR, layer them and make them an AND
			// Many Minds
			case "Gather1Beast": DrawIconInCenter( rect, Img.Land_Gather_Beasts ); break;
			case "ApplyDamage": DrawIconInCenter( rect, Img.Damage_2 ); break;
			case "DiscardPowerCards": DrawIconInCenter( rect, Img.Discard2 ); break;
			case "IgnoreRange": IgnoreRange( rect ); break;
            // Towering Roots
			case "AddVitalityToIncarna": 
				iconDrawer.DrawTheIcon(
					//new IconDescriptor { ContentImg = Img.Icon_Incarna, Sub = new IconDescriptor{ ContentImg = Img.Icon_Vitality } },
					new IconDescriptor { ContentImg = Img.Icon_Vitality, Sub = new IconDescriptor { ContentImg = Img.Icon_Incarna } },
					rect
				);
				break;
			case "ReplacePresenceWithIncarna":
				iconDrawer.DrawTheIcon( new IconDescriptor {  ContentImg = Img.Icon_Incarna, Sub = new IconDescriptor { ContentImg = Img.Icon_DestroyedPresence }, }, rect );
				DrawMoveArrow( rect.Translate( 0, -rect.Height*.2f ) );
				break;

			case "PiecesEscape":
			case "PiecesEscape(1)":
			case "PiecesEscape(2)":
				iconDrawer.DrawTheIcon( new IconDescriptor { ContentImg = Img.Icon_EndlessDark, }, rect.Translate( 0, -rect.Height * .20f ) );
				PiecesEscape escape = (PiecesEscape)action;
				DrawMoveArrow( rect.Translate( 0, 0 ) );
				if(escape.NumToEscape != int.MaxValue)
					DrawRangeText( rect.Translate(0, rect.Height * .05f), escape.NumToEscape );
				break;

			case "MoveIncarnaAnywhere":
				iconDrawer.DrawTheIcon( new IconDescriptor { ContentImg = Img.Icon_Incarna, }, rect );
				DrawMoveArrow( rect );
				break;
			case "AddOrMoveIncarnaToPresence":
				iconDrawer.DrawTheIcon( new IconDescriptor { ContentImg = Img.Icon_Incarna, Sub = new IconDescriptor { ContentImg = Img.Icon_Presence }, }, rect );
				DrawMoveArrow( rect.Translate( 0, -rect.Height * .2f ) );
				break;
			default:
				_graphics.FillRectangle( Brushes.Goldenrod, Rectangle.Inflate( rect.ToInts(), -5, -5 ) );
				break;
		}

	}

	void AdditionalPlay( RectangleF bounds, int count ) {
		DrawIconInCenter( bounds, Img.CardPlayPlusN );

		using Font coinFont = UseGameFont( bounds.Height * .35f );
		string txt = (count > 0)
			? ("+" + count.ToString())
			: ("\u2014" + (-count).ToString());
		SizeF textSize = _graphics.MeasureString( txt, coinFont );
		PointF textTopLeft = new PointF(
			bounds.X + (bounds.Width - textSize.Width) * .35f,
			bounds.Y + (bounds.Height - textSize.Height) * .60f
		);
		_graphics.DrawString( txt, coinFont, Brushes.Black, textTopLeft );

	}

	void GainEnergy( RectangleF bounds, int delta ){
		// DrawTokenInCenter( rect, "Energy_Plus_"+delta);
		using var img = GetImage( Img.Coin );

		float imgWidth = bounds.Width, imgHeight = img.Height * imgWidth / img.Width; // assuming width limited

		_graphics.DrawImageFitBoth( img, bounds );

		using Font coinFont = UseGameFont( imgHeight * .5f );
		string txt = delta > 0 
			? ("+" + delta.ToString())
			: ("\u2014" + (-delta).ToString());
		SizeF textSize = _graphics.MeasureString( txt, coinFont );
		PointF textTopLeft = new PointF(
			bounds.X + (bounds.Width - textSize.Width) * .35f,
			bounds.Y + (bounds.Height - textSize.Height) * .60f
		);
		_graphics.DrawString( txt, coinFont, Brushes.Black, textTopLeft );

	}

	void DrawIconInCenter( RectangleF rect, Img img ) {
		var image = GetImage( img );
		float imgWidth = rect.Width, imgHeight = image.Height * imgWidth / image.Width;
		_graphics.DrawImage( image, rect.X, rect.Y + (rect.Height - imgHeight) / 2, imgWidth, imgHeight );
	}

	void GainElement( Rectangle rect, params Element[] elements ) {
		var parts = rect.SplitHorizontally(elements.Length);
		for(int i = 0; i < elements.Length; ++i) {
			using var img = GetImage( elements[i].GetTokenImg() );
			_graphics.DrawImageFitWidth(img, parts[i]);
		}
	}

	void GainTime( RectangleF rect ) {
		using var img = GetImage( Img.FracturedDays_Gain2Time );
		_graphics.DrawImageFitWidth(img, rect );
	}

	void Gain1TimeOr2CardPlaysX2( RectangleF rect ) {
		DrawImage( rect, Img.FracturedDays_Gain1Timex2 );
	}

	void Gain1TimeOr2EnergyX3( RectangleF rect ) {
		DrawImage(rect, Img.FracturedDays_Gain1Timex3 );
	}

	void DrawImage( RectangleF rect, Img img ) {
		using var image = GetImage( img );
		_graphics.DrawImageFitBoth(image, rect );
	}

	static Bitmap GetTargetFilterIcon( string filterEnum ) {
		Img img = GetImgEnum( filterEnum );
		return img == default ? null : GetImage( img );
	}

	static Img GetImgEnum( string filterEnum ) {
		const string OR = "Or"; // !!! Coordinate this with PowerCardImageManager DrawTarget
		Img img = filterEnum switch {
			Target.Dahan                           => Img.Icon_Dahan,
			Target.Jungle + OR + Target.Wetland    => Img.Icon_JungleOrWetland,
			Target.Dahan + OR + Target.Invaders    => Img.Icon_DahanOrInvaders,
			Target.Coastal                         => Img.Icon_Coastal,
			Target.Presence + OR + Target.Wilds    => Img.Icon_PresenceOrWilds,
			Target.NoBlight                        => Img.Icon_NoBlight,
			Target.Beast + OR + Target.Jungle      => Img.Icon_BeastOrJungle,
			Target.Ocean                           => Img.Icon_Ocean,
			Target.Mountain + OR + Target.Presence => Img.Icon_MountainOrPresence,
			_                                      => Img.None, // Inland, Any
		};
		return img;
	}

	void MovePresence( RectangleF rect, int range ) {

		using var presenceIcon = GetImage( Img.Icon_Presence );

		// + presence
		float iconCenterY = rect.Y + rect.Height * .3f; // top of presence
		float presenceWidth = rect.Width * .6f;
		float presenceHeight = presenceIcon.Height * presenceWidth / presenceIcon.Width;
		_graphics.DrawImage( presenceIcon, rect.X + (rect.Width - presenceWidth) / 2, iconCenterY - presenceHeight * .5f, presenceWidth, presenceHeight );

		// range # text
		DrawRangeText( rect, range );

		// range arrow
		DrawMoveArrow( rect );

	}

	void DrawRangeText( RectangleF rect, int range ) {
		float rangeTextTop = rect.Y + rect.Height * .55f;
		string txt = range.ToString();
		using Font font = UseGameFont( rect.Height * .25f );
		SizeF rangeTextSize = _graphics.MeasureString( txt, font );
		_graphics.DrawString( txt, font, Brushes.Black, rect.X + (rect.Width - rangeTextSize.Width) / 2, rangeTextTop );
	}

	void DrawMoveArrow( RectangleF rect ) {
		float rangeArrowTop = rect.Y + rect.Height * .85f;
		using var rangeIcon = GetImage( Img.MoveArrow );
		float arrowWidth = rect.Width * .8f, arrowHeight = arrowWidth * rangeIcon.Height / rangeIcon.Width;
		_graphics.DrawImage( rangeIcon, rect.X + (rect.Width - arrowWidth) / 2, rangeArrowTop, arrowWidth, arrowHeight );
	}

	void PlacePresence( RectangleF bounds, GrowthActionFactory growth ) {
		var (range,filterEnum,addOnIcon) = growth switch {
			PlaceInOcean { } => ((int?)null, Target.Ocean, Img.None),
			PlacePresenceAndBeast => ((int?)3, Target.Any, Img.Beast), // add on icon
			PlacePresenceOrDisease => ((int?)1, Target.Any, Img.Disease),
			PlacePresence { Range: int r, FilterDescription: string f } => ((int?)r, f, Img.None), // generic, do last
			_ => throw new ArgumentException("growth action factory not a place-presence",nameof(growth)),
		};

		using var image = GetTargetFilterIcon( filterEnum );

		float fontScale        = image == null ? .25f : .15f;
		float presenceYPercent = image == null ? .3f  : .2f;
		float textTopScale     = image == null ? .55f : .7f;

		using Font font        = UseGameFont( bounds.Height * fontScale );

		// Draw: + presence
		using var presenceIcon = GetImage( Img.Icon_Presence );
		float iconCenterY = bounds.Y + bounds.Height * presenceYPercent; // top of presence
		float presenceWidth = bounds.Width*.6f;
		float presenceHeight = presenceIcon.Height * presenceWidth / presenceIcon.Width;
		float presenceX = bounds.X + (bounds.Width-presenceWidth)/2 + bounds.Width*.1f;

		_graphics.DrawString( "+", font, Brushes.Black, presenceX - bounds.Width*.3f, iconCenterY-bounds.Height*fontScale*.5f );
		_graphics.DrawImage(presenceIcon, presenceX, iconCenterY-presenceHeight*.5f, presenceWidth, presenceHeight );


		// icon
		if(image != null) {
			const float iconPercentage = .4f;
			SizeF iconSize = GetIconSize( bounds, image.Size );
			_graphics.DrawImage( image,
				bounds.X + (bounds.Width - iconSize.Width) / 2,
				bounds.Y + bounds.Height * iconPercentage,
				iconSize.Width,
				iconSize.Height
			);
		}

		if( range.HasValue ) {
			// range # text
			float rangeTextTop = bounds.Y + bounds.Height * textTopScale;
			string txt = range.Value.ToString();
			SizeF rangeTextSize = _graphics.MeasureString(txt,font);
			_graphics.DrawString(txt,font,Brushes.Black, bounds.X+(bounds.Width-rangeTextSize.Width)/2, rangeTextTop);

			// range arrow
			float rangeArrowTop = bounds.Y + bounds.Height * .85f;
			using var rangeIcon = GetImage( Img.RangeArrow );
			float arrowWidth = bounds.Width * .8f, arrowHeight = arrowWidth * rangeIcon.Height / rangeIcon.Width;
			_graphics.DrawImage( rangeIcon, bounds.X + (bounds.Width-arrowWidth)/2, rangeArrowTop, arrowWidth, arrowHeight );
		}

		if( addOnIcon != Img.None )
			DrawIconInCenter( bounds.ToInts().InflateBy( -(int)(bounds.Width * .2f) ), addOnIcon );

	}

	static SizeF GetIconSize( RectangleF bounds, Size imageSize ) { // ??? can we make this generic?
		float iconHeight = bounds.Height * .3f;
		SizeF iconSize = new SizeF( iconHeight * imageSize.Width / imageSize.Height, iconHeight );
		if(bounds.Width < iconSize.Width) { // too wide, switch scaling to width limited
			iconSize = new SizeF( bounds.Width, bounds.Width * imageSize.Height / imageSize.Width );
		}

		return iconSize;
	}

	void IgnoreRange( RectangleF rect ) {
		using var icon = GetImage( Img.Icon_Checkmark );

		float fontScale        = .25f;
		float presenceYPercent = .3f;

		using Font font = UseGameFont( rect.Height * fontScale );

		// + presence
		float iconCenterY = rect.Y + rect.Height * presenceYPercent; // top of presence
		float iconWidth = rect.Width * .6f;
		float iconHeight = icon.Height * iconWidth / icon.Width;
		float iconX = rect.X + (rect.Width - iconWidth) / 2; // + rect.Width * .1f;

		_graphics.DrawImage( icon, iconX, iconCenterY - iconHeight * .5f, iconWidth, iconHeight );

		// range arrow
		float rangeArrowTop = rect.Y + rect.Height * .85f;
		using var rangeIcon = GetImage( Img.RangeArrow );
		float arrowWidth = rect.Width * .8f, arrowHeight = arrowWidth * rangeIcon.Height / rangeIcon.Width;
		_graphics.DrawImage( rangeIcon, rect.X + (rect.Width - arrowWidth) / 2, rangeArrowTop, arrowWidth, arrowHeight );

	}

	public void Dispose() {
		_cachedImageLayer?.Dispose();
	}

	static Font UseGameFont( float fontHeight ) => ResourceImages.Singleton.UseGameFont( fontHeight );
	static Bitmap GetImage( Img img ) => ResourceImages.Singleton.GetImage( img );

}

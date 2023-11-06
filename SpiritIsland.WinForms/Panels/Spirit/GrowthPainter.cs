using SpiritIsland.Basegame;
using SpiritIsland.JaggedEarth;
using SpiritIsland.NatureIncarnate;
using System;
using System.Drawing;
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
		_iconDrawer = new IconDrawer( g, new CachedImageDrawer() );
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

	void DrawAction( IHelpGrow growthAction, RectangleF rect ) {
		if(growthAction is not SpiritGrowthAction sga) return;
		IActOn<SelfCtx> action = sga.Cmd;

		if(action is JaggedEarth.RepeatableSelfCmd repeatableActionFactory 
			&& repeatableActionFactory.Inner is not JaggedEarth.GainTime
		)
			action = repeatableActionFactory.Inner;


		if(action is GainEnergy ge)             { GainEnergy( rect, ge.Delta ); return; }
		if(action is ReclaimAll)                { DrawIconInCenter( rect, Img.ReclaimAll ); return; }
		if(action is ReclaimN)                  { DrawIconInCenter( rect, Img.Reclaim1 ); return; }
		if(action is ReclaimHalf)               { DrawIconInCenter( rect, Img.ReclaimHalf ); return; }
		if(action is GainPowerCard)             { DrawIconInCenter( rect, Img.GainCard ); return; }
		if(action is PlacePresence)             { PlacePresence( rect, action ); return; }
		if(action is MovePresence mp)           { MovePresence( rect, mp.Range ); return; }
		if(action is PlayExtraCardThisTurn pec) { AdditionalPlay( rect, pec.Count ); return; }
		if(action is GainElements gel)          { DrawGainAllElements( rect, gel.ElementsToGain ); return; }
		if(action is Gain1Element g1e)          { DrawGain1Element( rect, g1e.ElementOptions ); return; }

		switch(action.Description)              {
			case "Add a Presence or Disease": PlacePresence( rect, action ); break;
			case "PlacePresenceAndBeast":   PlacePresence( rect, action ); break;

            // Wounded Waters Bleeding
			case "PlaceDestroyedPresence(1)": PlacePresence( rect, action ); break;

			// Ocean
			case "PlaceInOcean": PlacePresence( rect, action ); break;
			case "Gather Presence into Ocean": DrawIconInCenter( rect, Img.GatherToOcean ); break;
			case "Push Presence from Ocean":   DrawIconInCenter( rect, Img.Pushfromocean ); break;

			// Heart of the WildFire
			case "EnergyForFire": DrawIconInCenter( rect, Img.Oneenergyfire ); break;

			// Fractured Dates
			case "GainTime(2)":    GainTime( rect ); break;
			case "GainTime(1)x2":  Gain1TimeOr2CardPlaysX2( rect ); break;
			case "GainTime(1)x3":  Gain1TimeOr2EnergyX3( rect ); break;
			case "DrawPowerCardFromDaysThatNeverWere": DrawImage( rect, Img.FracturedDays_DrawDtnw ); break; 
			// Starlight Seeks Its Form
			case "MakePowerFast": DrawIconInCenter( rect, Img.Icon_Fast ); break;
			// Grinning Trickster
			case "GainEnergyEqualToCardPlays": DrawIconInCenter( rect, Img.GainEnergyEqualToCardPlays ); break;
				break;
			// Many Minds
			case "Gather1Token": DrawIconInCenter( rect, Img.Land_Gather_Beasts ); break; // Gather 1 Beast
			case "ApplyDamage": DrawIconInCenter( rect, Img.Damage_2 ); break;
			case "Discard 2 Power Cards": DrawIconInCenter( rect, Img.Discard2 ); break;
			case "IgnoreRange": IgnoreRange( rect ); break;
            // Towering Roots
			case "AddVitalityToIncarna": 
				_iconDrawer.DrawTheIcon(
					//new IconDescriptor { ContentImg = Img.Icon_Incarna, Sub = new IconDescriptor{ ContentImg = Img.Icon_Vitality } },
					new IconDescriptor { ContentImg = Img.Icon_Vitality, Sub = new IconDescriptor { ContentImg = Img.Icon_Incarna } },
					rect
				);
				break;
			case "ReplacePresenceWithIncarna":
				_iconDrawer.DrawTheIcon( new IconDescriptor {  ContentImg = Img.Icon_Incarna, Sub = new IconDescriptor { ContentImg = Img.Icon_DestroyedPresence }, }, rect );
				DrawMoveArrow( rect.Translate( 0, -rect.Height*.2f ) );
				break;

			case "All pieces Escape":
			case "1 pieces Escape":
			case "2 pieces Escape":
				_iconDrawer.DrawTheIcon( new IconDescriptor { ContentImg = Img.Icon_EndlessDark, }, rect.Translate( 0, -rect.Height * .20f ) );
				PiecesEscape escape = (PiecesEscape)action;
				DrawMoveArrow( rect.Translate( 0, 0 ) );
				if(escape.NumToEscape != int.MaxValue)
					DrawRangeText( rect.Translate(0, rect.Height * .05f), escape.NumToEscape );
				break;

			case "Move Incarna anywhere":
				_iconDrawer.DrawTheIcon( new IconDescriptor { ContentImg = Img.Icon_Incarna, }, rect );
				DrawMoveArrow( rect );
				break;
			case "AddOrMoveIncarnaToPresence":
				_iconDrawer.DrawTheIcon( new IconDescriptor { ContentImg = Img.Icon_Incarna, Sub = new IconDescriptor { ContentImg = Img.Icon_Presence }, }, rect );
				DrawMoveArrow( rect.Translate( 0, -rect.Height * .2f ) );
				break;
            case "AddPresenceOrGainMajor": {
					var rows = rect.ToInts().SplitVerticallyAt(.5f);
					Rectangle presRect = rows[0].SplitHorizontally( 2 )[0];
					Rectangle cardRect = rows[1].SplitHorizontally( 2 )[1];
					int majorOffset = cardRect.Width/2;
					Rectangle majorRect = new Rectangle( cardRect.X+majorOffset/43, cardRect.Y + majorOffset / 4, majorOffset, majorOffset );
					PlacePresence( presRect,new PlacePresence(2));
					DrawIconInCenter( cardRect, Img.GainCard );
					DrawIconInCenter( majorRect, Img.Icon_Major );
				}
				break;
			case "AccelerateOrDelay":
				_iconDrawer.DrawTheIcon( 
					new IconDescriptor {
						BackgroundImg = Img.Coin,
						Text = "±1",
						Sub = new IconDescriptor { BackgroundImg = Img.Icon_ImpendingCard },
						Super = new IconDescriptor { BackgroundImg = Img.Icon_ImpendingCard },
					}, rect
				);
				break;

            case "Add up to 3 Destroyed Presence together":
				// !!! stand-in  FIX
				_iconDrawer.DrawTheIcon( new IconDescriptor { ContentImg = Img.Icon_Incarna, }, rect );
				DrawMoveArrow( rect );
				break;

			case "Gain Energy an additional time":
				// !!! stand-in  FIX
				_iconDrawer.DrawTheIcon( new IconDescriptor { ContentImg = Img.Icon_Incarna, }, rect );
				DrawMoveArrow( rect );
				break;
            
			case "Move up to 3 Presence together":
				_iconDrawer.DrawTheIcon( new IconDescriptor { ContentImg = Img.Icon_Incarna, }, rect );
				DrawMoveArrow( rect );
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

	void DrawGain1Element( RectangleF rect, params Element[] elements ) {
		var parts = rect.ToInts().SplitHorizontally(elements.Length);
		for(int i = 0; i < elements.Length; ++i) {
			using Bitmap img = GetImage( elements[i].GetTokenImg() );
			_graphics.DrawImageFitWidth(img, parts[i]);
		}
	}

	void DrawGainAllElements( RectangleF rect, params Element[] elements ) {
		var descriptor = new IconDescriptor();
		if(0 < elements.Length )
			descriptor.ContentImg = elements[0].GetTokenImg();
		if(1 < elements.Length)
			descriptor.ContentImg2 = elements[1].GetTokenImg();

		_iconDrawer.DrawTheIcon( descriptor, rect );
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

	void PlacePresence( RectangleF bounds, IActOn<SelfCtx> growth ) {
		
		var (presence,range,filterEnum,addOnIcon) = growth switch {
			PlaceInOcean           => (Img.Icon_Presence, null,    Target.Ocean, Img.None),
			PlacePresenceAndBeast  => (Img.Icon_Presence, (int?)3, Target.Any, Img.Beast), // add an icon
			{Description: string n } when n == "Add a Presence or Disease" => (Img.Icon_Presence, (int?)1, Target.Any, Img.Disease),
			PlaceDestroyedPresence { Range: int r, FilterDescription: string f } => (Img.Icon_DestroyedPresence, (int?)r, f, Img.None), // generic, do last
			PlacePresence{ Range: int r, FilterDescription: string f } => (Img.Icon_Presence, (int?)r, f, Img.None), // generic, do last
			_ => throw new ArgumentException("growth action factory not a place-presence",nameof(growth)),
		};

		using var image = GetTargetFilterIcon( filterEnum );

		float fontScale        = image == null ? .25f : .15f;
		float presenceYPercent = image == null ? .3f  : .2f;
		float textTopScale     = image == null ? .55f : .7f;

		using Font font        = UseGameFont( bounds.Height * fontScale );

		// Draw: + presence
		using var presenceIcon = GetImage( presence );
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

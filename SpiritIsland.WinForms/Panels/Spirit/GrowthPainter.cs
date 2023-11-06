using Microsoft.CodeAnalysis.CSharp.Syntax;
using SpiritIsland.Basegame;
using SpiritIsland.JaggedEarth;
using SpiritIsland.NatureIncarnate;
using System;
using System.Drawing;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

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

		_graphics = g;

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

		Rectangle iRect = rect.ToInts();

		if(action is GainEnergy ge)             { GainEnergyRect_Paint( ge.Delta, iRect ); return; }
		if(action is ReclaimAll)                { ImgRect_Paint( Img.ReclaimAll, iRect ); return; }
		if(action is ReclaimN)                  { ImgRect_Paint( Img.Reclaim1, iRect ); return; }
		if(action is ReclaimHalf)               { ImgRect_Paint( Img.ReclaimHalf, iRect ); return; }
		if(action is GainPowerCard)             { ImgRect_Paint( Img.GainCard, iRect ); return; }
		if(action is PlacePresence)             { Draw_PlacePresence( rect, action ); return; }
		if(action is MovePresence mp)           { Draw_MovePresence( mp.Range, iRect ); return; }
		if(action is PlayExtraCardThisTurn pec) { Draw_AdditionalPlay( rect, pec.Count ); return; }
		if(action is GainElements gel)          { Draw_GainAllElements( rect, gel.ElementsToGain ); return; }
		if(action is Gain1Element g1e)          { DrawGain1Element( rect, g1e.ElementOptions ); return; }

		switch(action.Description)              {
			case "Add a Presence or Disease": Draw_PlacePresence( rect, action ); break;
			case "PlacePresenceAndBeast":   Draw_PlacePresence( rect, action ); break;

            // Wounded Waters Bleeding
			case "PlaceDestroyedPresence(1)": Draw_PlacePresence( rect, action ); break;

			// Ocean
			case "PlaceInOcean": Draw_PlacePresence( rect, action ); break;
			case "Gather 1 Presence into EACH Ocean": ImgRect_Paint(Img.GatherToOcean, iRect ); break;
			case "Push Presence from Ocean":          ImgRect_Paint(Img.Pushfromocean, iRect ); break;

			// Heart of the WildFire
			case "EnergyForFire": ImgRect_Paint(Img.Oneenergyfire, iRect ); break;

			// Fractured Dates
			case "GainTime(2)":		ImgRect_Paint( Img.FracturedDays_Gain2Time, iRect ); break;
			case "GainTime(1)x2":	ImgRect_Paint( Img.FracturedDays_Gain1Timex2, iRect ); break;
			case "GainTime(1)x3":	ImgRect_Paint( Img.FracturedDays_Gain1Timex3, iRect ); break;
			case "DrawPowerCardFromDaysThatNeverWere": 
									ImgRect_Paint( Img.FracturedDays_DrawDtnw, iRect ); break; 

			// Starlight Seeks Its Form
			case "MakePowerFast": ImgRect_Paint(Img.Icon_Fast, iRect ); break;

			// Grinning Trickster
			case "GainEnergyEqualToCardPlays": ImgRect_Paint(Img.GainEnergyEqualToCardPlays, iRect ); break;

			// Many Minds
			case "Gather1Token": ImgRect_Paint(Img.Land_Gather_Beasts, iRect ); break; // Gather 1 Beast
			case "ApplyDamage": ImgRect_Paint( Img.Damage_2, iRect ); break;
			case "Discard 2 Power Cards": ImgRect_Paint(Img.Discard2, iRect ); break;
			case "IgnoreRange": Draw_IgnoreRange( rect ); break;

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
            
			// Breath of Darkness
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

            // Dances up Earthquakes
            case "AddPresenceOrGainMajor": {
					Rectangle[] rows = rect.ToInts().SplitVerticallyAt(.5f);
					Rectangle presRect = rows[0].SplitHorizontally( 2 )[0];
					Rectangle cardRect = rows[1].SplitHorizontally( 2 )[1];
					int majorOffset = cardRect.Width/2;
					Rectangle majorRect = new Rectangle( cardRect.X+majorOffset/43, cardRect.Y + majorOffset / 4, majorOffset, majorOffset );
					Draw_PlacePresence( presRect,new PlacePresence(2));
					ImgRect_Paint(Img.GainCard, cardRect);
					ImgRect_Paint(Img.Icon_Major, majorRect);
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

            // Relentless Gaze of the Sun
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

	void Draw_AdditionalPlay( RectangleF bounds, int count ) {
		ImgRect_Paint(Img.CardPlayPlusN, bounds.ToInts());

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

	void GainEnergyRect_Paint( int delta, Rectangle bounds ){
		new GainEnergyRect( delta ).Paint( _graphics, bounds );
	}

	void ImgRect_Paint( Img img, Rectangle rect ) {
		new ImgRect(img).Paint(_graphics,rect);
	}

	void DrawGain1Element( RectangleF rect, params Element[] elements ) {
		var parts = rect.ToInts().SplitHorizontally(elements.Length);
		for(int i = 0; i < elements.Length; ++i) {
			using Bitmap img = GetImage( elements[i].GetTokenImg() );
			_graphics.DrawImageFitWidth(img, parts[i]);
		}
	}

	void Draw_GainAllElements( RectangleF rect, params Element[] elements ) {
		var descriptor = new IconDescriptor();
		if(0 < elements.Length )
			descriptor.ContentImg = elements[0].GetTokenImg();
		if(1 < elements.Length)
			descriptor.ContentImg2 = elements[1].GetTokenImg();

		_iconDrawer.DrawTheIcon( descriptor, rect );
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

	void Draw_MovePresence( int range, Rectangle rect ) {
		new VerticalStackRect(
			new NullRect(),
			new ImgRect( Img.Icon_Presence ),
			new TextRect( range ),
			new ImgRect( Img.MoveArrow )
		)	.SplitByWeight(.05f, .1f, .3f, .35f, .15f, .1f)
			.Paint(_graphics,rect);
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

	void Draw_PlacePresence( RectangleF bounds, IActOn<SelfCtx> growth ) {
		
		var (presence,range,filterEnum,addOnIcon) = growth switch {
			PlaceInOcean			=> (Img.Icon_Presence, null,    Target.Ocean, Img.None),
			PlacePresenceAndBeast	=> (Img.Icon_Presence, (int?)3, Target.Any, Img.Beast), // add an icon
			{Description: string n } when n == "Add a Presence or Disease" 
									=> (Img.Icon_Presence, (int?)1, Target.Any, Img.Disease),
			PlaceDestroyedPresence { Range: int r, FilterDescription: string f } 
									=> (Img.Icon_DestroyedPresence, (int?)r, f, Img.None),
			// generic, do last
			PlacePresence{ Range: int r, FilterDescription: string f } 
									=> (Img.Icon_Presence, (int?)r, f, Img.None),          
			_ => throw new ArgumentException("growth action factory not a place-presence",nameof(growth)),
		};

		// IPaintableRect placePresenceRect = new ImgRect( Img.Icon_Presence ); // + presence
		IPaintableRect placePresenceRect = new HorizontalStackRect(
			new NullRect(),
			new TextRect( "+" ),
			new ImgRect( Img.Icon_Presence )
		).SplitByWeight(0f,.15f,.15f,.5f,.2f);

		Img filterImg = GetImgEnum( filterEnum );

		IPaintableRect paintable = 
			// Ocean
			range is null ? new VerticalStackRect(
				new NullRect(),
				placePresenceRect,
				new ImgRect( filterImg )
			).SplitByWeight( .05f, .1f, .3f, .5f, .1f )
			// HeathVigil, : Keeper
			: filterImg != Img.None ? new VerticalStackRect(
					new NullRect(),
					placePresenceRect,
					new ImgRect( filterImg ),
					new TextRect( range ),
					new ImgRect( Img.RangeArrow )
				).SplitByWeight( .05f,.05f /*null*/,.2f /*presence*/,.25f/*filter*/, .25f/*number*/,.1f/*arrow*/,.05f )
            // default
            : new VerticalStackRect(
				new NullRect(),
				placePresenceRect,
				new NullRect(),
				new TextRect( range ),
				new ImgRect( Img.RangeArrow )
			).SplitByWeight( .0f, .1f/*spacer*/, .3f, .05f /*spacer*/, .4f /*number*/, .1f/*arrow*/, .1f );
	
		paintable.Paint(_graphics,bounds.ToInts());

		if( addOnIcon != Img.None )
			ImgRect_Paint( addOnIcon, bounds.ToInts().InflateBy( -(int)(bounds.Width * .2f) ) );

	}

	void Draw_IgnoreRange( RectangleF rect ) {
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

interface IPaintableRect {
	Rectangle Paint(Graphics graphics,Rectangle rect);
}

class VerticalStackRect : IPaintableRect {
	IPaintableRect[] _children;
	Func<Rectangle, Rectangle[]> _splitter;

	public VerticalStackRect(params IPaintableRect[] children ) {
		_children = children;
		_splitter = SplitEqually;
	}
    public VerticalStackRect SplitByWeight( float margin, params float[] weights ) {
		_splitter = (bounds) => bounds.SplitVerticallyByWeight( margin, weights );
		return this;
	}
	Rectangle[] SplitEqually(Rectangle rect) => rect.SplitVerticallyIntoRows(0,_children.Length);
	public Rectangle Paint( Graphics graphics, Rectangle rect ) {
		var rects = _splitter(rect);
		for(int i=0;i<_children.Length;++i)
			_children[i].Paint( graphics, rects[i] );
		return rect;
	}
}

class HorizontalStackRect : IPaintableRect {
	IPaintableRect[] _children;
	Func<Rectangle, Rectangle[]> _splitter;

	public HorizontalStackRect( params IPaintableRect[] children ) {
		_children = children;
		_splitter = SplitEqually;
	}
	public HorizontalStackRect SplitByWeight( float margin, params float[] weights ) {
		_splitter = ( bounds ) => bounds.SplitHorizontallyByWeight( margin, weights );
		return this;
	}
	Rectangle[] SplitEqually( Rectangle rect ) => rect.SplitHorizontallyIntoColumns( 0, _children.Length );
	public Rectangle Paint( Graphics graphics, Rectangle rect ) {
		var rects = _splitter( rect );
		for(int i = 0; i < _children.Length; ++i)
			_children[i].Paint( graphics, rects[i] );
		
		return rect;
	}
}


class ImgRect : IPaintableRect {
	public ImgRect(Img img ) { _img=img; }
	readonly Img _img;
	public Rectangle Paint( Graphics graphics, Rectangle rect ) {
		using Bitmap image = ResourceImages.Singleton.GetImage( _img );
		float imgWidth = rect.Width, imgHeight = image.Height * imgWidth / image.Width;
		var fitted = rect.FitBoth( image.Size );
		graphics.DrawImage(image, fitted );
		return fitted;
	}

}

class TextRect : IPaintableRect {
	public TextRect( string text ) { _text = text; }
	public TextRect( object obj ) { _text = obj.ToString(); }
	readonly string _text;
	public Rectangle Paint( Graphics graphics, Rectangle rect ) {
		Font font = ResourceImages.Singleton.UseGameFont( rect.Height );
		try {
			SizeF textSize = graphics.MeasureString(_text,font);
			Rectangle fitted = rect.FitHeight( textSize.ToSize() );

			if(rect.Width < fitted.Width ) { // too narrow
				font.Dispose();
				// Scale down font
				font = ResourceImages.Singleton.UseGameFont( rect.Height * rect.Width / fitted.Width );
				textSize = graphics.MeasureString( _text, font );
				fitted = rect.FitWidth( textSize.ToSize() );
			}

			using StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
			graphics.DrawString( _text, font, Brushes.Black, fitted, alignCenter );

			return fitted;
		}
		finally {
			font?.Dispose();
		}
	}

}

class NullRect : IPaintableRect {
	public Rectangle Paint( Graphics graphics, Rectangle rect ){ /* draw nothing */ return rect; }
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SpiritIsland.WinForms.Panels.Island;

class IslandPanel : IPanel {

	#region constructor
	public IslandPanel( SharedCtx ctx ) {
		_ctx = ctx;

		WorldLayoutChanged += CacheBackground_Invalidate;
		ScreenBoundsChanged+= CacheBackground_Invalidate;

		WorldLayoutChanged += InitButtonContainer; // !! instead of Init, switch to invalidate
	}
	#endregion

	public void AssignBounds( RegionLayoutClass regionLayout ) {
		Bounds = regionLayout.IslandRect;
	}

	public Rectangle Bounds {
		get {
			return _availableScreenRect;
		}
		set {
			_availableScreenRect = value;
			ScreenBoundsChanged?.Invoke();
		}
	}

	public bool HasFocus { set { } }

	public int OptionCount { get; private set; }

	// !! Instead of putting Gamestate in the _sharedCtx, pass in here.
	public void OnGameLayoutChanged() { _worldLayout = null; WorldLayoutChanged?.Invoke(); }

	public void Paint( Graphics graphics ) {
		if(_cachedBackground == null) {
			MapWorldToScreen();
			InitBackgroundCache();
		}
		DrawBackground( graphics );

		_buttonContainer.ClearTransient();
		if(_decision is A.SpaceToken spaceTokenDecision)
			_outstandingSpaceTokenOptions.UnionWith( spaceTokenDecision.SpaceTokens );

		foreach(SpaceState space in _ctx.GameState.Spaces_Unfiltered)
			DecorateSpace( graphics, space );

		AddButtonsForVirtualSpaceTokens();

		DrawArrows( graphics );
		_buttonContainer.Paint( graphics );
	}

	public void ActivateOptions( IDecision decision ) {
		_decision = decision;

		_outstandingSpaceTokenOptions.Clear();
		if(_decision is A.SpaceToken spaceTokenDecision)
			_outstandingSpaceTokenOptions.UnionWith( spaceTokenDecision.SpaceTokens );

		_buttonContainer.EnableOptions( decision );

		this.OptionCount = _buttonContainer.ActivatedOptions + _outstandingSpaceTokenOptions.Count;
	}

	public Action GetClickableAction( Point clientCoords ) {
		IOption option = _buttonContainer.FindEnabledOption( clientCoords );
		return option == null ? null : ()=>_ctx.SelectOption( option );
	}

	public RegionLayoutClass GetLayout( Rectangle bounds ) {
		return RegionLayoutClass.ForIslandFocused( bounds, _ctx._spirit.Decks.Length + 1 ); // everything else
	}

	public int ZIndex => 0;

	#region private methods

	void InitBackgroundCache() {
		_cachedBackground = new Bitmap( _usedBoardScreenRect.Width, _usedBoardScreenRect.Height );
		Graphics graphics = Graphics.FromImage( _cachedBackground );
		graphics.TranslateTransform( -_usedBoardScreenRect.X, -_usedBoardScreenRect.Y );
		graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

		//foreach(Board board in _ctx.GameState.Island.Boards)
		//	DrawBoardSpacesOnly( graphics, board.Spaces_Unfiltered );
		DrawBoardSpacesOnly( graphics, _ctx.GameState.Spaces_Unfiltered.Downgrade() ); //!!! wastefull to promote to SpaceState then downgrade.
	}

	void MapWorldToScreen() {
		// Mapping part 1 set used Board Screen Rect
		RectangleF worldBounds = WorldLayout.Bounds;
		Size islandWorldSize = worldBounds.Scale( 1000 ).ToInts().Size;
		_usedBoardScreenRect = _availableScreenRect.FitBoth( islandWorldSize, Align.Center, Align.Near );
		// Mapping part 2 set world-to-screen transform
		_mapper = new PointMapper( CalcWorldToScreenMatrix( worldBounds, _usedBoardScreenRect ) );
		_iconWidth = (int)(_mapper.XScale * .075f);//  gw_boardScreenRect.Width / 20; // !!! scale tokens based on board/space size, NOT widow size (for 2 boards, tokens are too big)
	}

	static Matrix3D CalcWorldToScreenMatrix( RectangleF worldRect, Rectangle viewportRect ) {
		// calculate scaling Assuming height limited
		float scale = viewportRect.Height / worldRect.Height;

		var islandBitmapMatrix
			= RowVector.Translate( -worldRect.X, -worldRect.Y ) // translate to origin
			* RowVector.Scale( scale, -scale ) // flip-y and scale
			* RowVector.Translate( 0, viewportRect.Height ) // because 0,0 is at the bottom,left
			* RowVector.Translate( viewportRect.X, viewportRect.Y ); // translate to viewport origin
		return islandBitmapMatrix;
	}

	void CacheBackground_Invalidate() {
		if(_cachedBackground != null) {
			_cachedBackground.Dispose();
			_cachedBackground = null;
		}
	}

	/// <summary> Used for Arrows and Space Buttons </summary>
	Point GetPortPoint( Space space, IToken visibileTokens ) {
		PointF worldCoord = WorldLayout.GetCoord( space, visibileTokens );
		return _mapper.Map( worldCoord ).ToInts();
	}

	void DrawBackground( Graphics graphics ) {
		graphics.DrawImage( _cachedBackground, _usedBoardScreenRect );
	}

	void DrawBoardSpacesOnly( Graphics graphics, IEnumerable<Space> spaces ) {
		Pen perimeterPen = new Pen( SpacePerimeterColor, 5f );

		foreach(var space in spaces) {
			using Brush brush = ResourceImages.Singleton.UseSpaceBrush( space );
			SpaceLayout spaceLayout = WorldLayout.MySpaceLayout( space );
			PointF[] points = spaceLayout.Corners.Select( _mapper.Map ).ToArray();

			// Draw smoothy
			graphics.FillClosedCurve( brush, points, FillMode.Alternate, .25f );
			graphics.DrawClosedCurve( perimeterPen, points, .25f, FillMode.Alternate );

			// Draw Label
			PointF nameLocation = _mapper.Map( WorldLayout.InsidePoints( space ).NameLocation );
			graphics.DrawString( space.Text, SystemFonts.MessageBoxFont, SpaceLabelBrush, nameLocation );
		}

	}

	void DecorateSpace( Graphics graphics, SpaceState spaceState ) {

		_transientSubscripts.Clear();

		WorldLayout.InsidePoints( spaceState.Space ).Init( spaceState );

		if(_ctx._debug)
			Debug_DrawTokenTargets( graphics, spaceState );

		if(spaceState.Space is MultiSpace ms)
			DrawMultiSpace( graphics, ms );

		DrawInvaderRow( graphics, spaceState );
		DrawRow( graphics, spaceState );
		DrawSubscripts( graphics );
	}

	void DrawRow( Graphics graphics, SpaceState spaceState ) {
		int iconWidth = _iconWidth;

		var tokenTypes = new List<IToken> {
			Token.Defend, Token.Blight, // These don't show up in .OfAnyType if they are dynamic
			Token.Wilds, Token.Badlands, Token.Isolate, Token.Vitality, Token.Quake
		}	
			.Union( spaceState.OfCategory( TokenCategory.Dahan ) )
			.Union( spaceState.OfCategory( TokenCategory.Incarna ) )
			.Union( spaceState.OfClass( Token.Beast ) )
			.Union( spaceState.OfAnyClass( _ctx._spirit.Presence.Token, Token.Element, Token.OpenTheWays, Token.Beast, Token.Disease ) )
			.Cast<IToken>()
			.ToArray();

		foreach(var token in tokenTypes) {
			int count = spaceState[token];
			if(count == 0) continue;

			Image img = _ctx._tip.AccessTokenImage( token );

			// calc rect
			int iconHeight = iconWidth * img.Height / img.Width;

			PointF pt = _mapper.Map( WorldLayout.InsidePoints( spaceState.Space ).GetPointFor( token ) );
			Rectangle rect = new Rectangle( 
				(int)(pt.X - iconWidth / 2), 
				(int)(pt.Y - iconHeight / 2), 
				iconWidth, 
				iconHeight
			);

			// record token location
			RecordSpaceTokenLocation( new SpaceToken( spaceState.Space, token ), rect );

			if(token is SpiritPresenceToken && _ctx._spirit.Presence.IsSacredSite( spaceState )) {
				const int inflationSize = 10;
				rect.Inflate( inflationSize, inflationSize );

				using var brush = new SolidBrush( Color.FromArgb( 100, SacredSiteColor ) );
				graphics.FillEllipse( brush, rect );
				rect.Inflate( -inflationSize, -inflationSize );
			}

			// Draw Tokens
			graphics.DrawImage( img, rect );
			// graphics.DrawCountIfHigherThan( rect, count );
			if(1 < count)
				_transientSubscripts.Add( new RectangleSubscript( rect, "x" + count ) );

		}


	}

	void DrawInvaderRow( Graphics graphics, SpaceState ss ) {
		int iconWidth = _iconWidth;

		var orderedInvaders = ss.OfTypeHuman()
			.Where( k => k.Class.Category == TokenCategory.Invader )
			// Major ordering: (Type > Strife)
			.OrderByDescending( i => i.FullHealth )
			.ThenBy( x => x.StrifeCount )
			// Minor ordering: (remaining health)
			.ThenBy( i => i.RemainingHealth ); // show damaged first so when we apply damage, the damaged one replaces the old undamaged one.

		foreach(IToken token in orderedInvaders) {

			// New way
			PointF center = _mapper.Map( WorldLayout.InsidePoints( ss.Space ).GetPointFor( token ) );
			float x = center.X - iconWidth / 2;
			float y = center.Y - iconWidth / 2; //!! approximate - need Image to get actual Height to scale

			// Strife
			IToken imageToken;
			if(token is HumanToken si && 0 < si.StrifeCount) {
				imageToken = si.HavingStrife( 0 );
				Image strife = _ctx._tip._strife;
				Rectangle strifeRect = new Rectangle( new Point( (int)x, (int)y ), strife.Size.FitWidth( (int)iconWidth ) );
				graphics.DrawImage( strife, strifeRect );
				if(si.StrifeCount > 1)
					graphics.DrawSuperscript( strifeRect, "x" + si.StrifeCount );
			} else {
				imageToken = token;
			}

			// record token location
			Image img = _ctx._tip.AccessTokenImage( imageToken );
			Rectangle rect = new Rectangle( new Point( (int)x, (int)y ), img.Size.FitWidth( (int)iconWidth ) );

			RecordSpaceTokenLocation( new SpaceToken( ss.Space, token ), rect );

			// Draw Token
			graphics.DrawImage( img, rect );
			// Count
			if(1 < ss[token])
				_transientSubscripts.Add( new RectangleSubscript( rect, "x" + ss[token] ) );
			//graphics.DrawCountIfHigherThan( rect, ss[token] );

		}

	}

	void DrawArrows( Graphics graphics ) {
		if(_decision is not A.IHaveArrows quiver) return;
		using Pen pushArrowPen = new Pen( ArrowColor, 7 );
		foreach(A.Arrow arrow in quiver.Arrows)
			graphics.DrawArrow( pushArrowPen, GetPortPoint( arrow.From, arrow.Token ), GetPortPoint( arrow.To, arrow.Token ) );
	}

	void DrawSubscripts( Graphics graphics ) {
		foreach(var sub in _transientSubscripts) // Draw these last so they are on top of the tokens AND VISIBLE
			graphics.DrawSubscript( sub );
	}

	void Debug_DrawTokenTargets( Graphics graphics, SpaceState spaceState ) {
		int iconWidth = _iconWidth;

		foreach(var (token, point) in WorldLayout.InsidePoints( spaceState.Space ).Assignments()) {
			using Image img = ResourceImages.Singleton.GetGhostImage( token.Img );
			var clientPoint = _mapper.Map( new PointF( point.X, point.Y ) );
			Rectangle rect = new Rectangle( // !!! verify all 3 instances that use this rect are offsetting point correctly.
				new Point(
					(int)(clientPoint.X - iconWidth / 2),
					(int)(clientPoint.Y - iconWidth / 2)
				),
				img.Size.FitWidth( (int)iconWidth )
			);
			graphics.DrawImage( img, rect );
		}
	}

	#endregion

	#region Buttons

	void InitButtonContainer() {
		_buttonContainer.Clear();
		foreach(SpaceState spaceState in _ctx.GameState.Spaces_Unfiltered) {
			// !!! Since GetPortPoint is dynamic, can we PLEASE make _hotspotRadius dynamic also?
			SpaceButton button = new SpaceButton( GetPortPoint, spaceState.Space, _hotspotRadius );
			_buttonContainer.Add( spaceState.Space, button );
		}
	}

	// For Options that are SpaceTokens, calc and draw their rectangles
	void AddButtonsForVirtualSpaceTokens() {
		foreach(SpaceToken spaceToken in _outstandingSpaceTokenOptions) {
			Point p = _mapper.Map( WorldLayout.InsidePoints( spaceToken.Space ).GetPointFor( spaceToken.Token ) ).ToInts();
			Rectangle rect = new Rectangle( p.X - _hotspotRadius, p.Y - _hotspotRadius, _hotspotRadius * 2, _hotspotRadius * 2 ); // !!! use token size, whatever that is.
			_buttonContainer.AddTransientEnabled( spaceToken, new SpaceTokenButton( rect ) );
		}
		_outstandingSpaceTokenOptions.Clear();
	}

	void RecordSpaceTokenLocation( SpaceToken st, Rectangle rect ) {
		if(_outstandingSpaceTokenOptions.Contains( st )) {
			_buttonContainer.AddTransientEnabled( st, new SpaceTokenButton( rect ) );
			_outstandingSpaceTokenOptions.Remove( st );
		}
	}

	#endregion

	#region Multi-Space

	void DrawMultiSpace( Graphics graphics, MultiSpace multi ) {

		using var pen = new Pen( MultiSpacePerimeterColor, 3f );

		using var brush = UseMultiSpaceBrush( multi );

		var points = WorldLayout.MySpaceLayout( multi ).Corners.Select( _mapper.Map ).ToArray();
		graphics.FillClosedCurve( brush, points, FillMode.Alternate, .25f );
		graphics.DrawClosedCurve( pen, points, .25f, FillMode.Alternate );
	}

	static LinearGradientBrush UseMultiSpaceBrush( MultiSpace multi ) {
		var brush = new LinearGradientBrush( new Rectangle( 0, 0, 30, 30 ), Color.Transparent, Color.Transparent, 45F );

		var colors = multi.OrigSpaces
			.Select( x => Color.FromArgb( 92, SpaceColor( x ) ) )
			.ToArray();

		var blend = new ColorBlend {
			Positions = new float[colors.Length * 2],
			Colors = new Color[colors.Length * 2]
		};
		float step = 1.0f / colors.Length;
		for(int i = 0; i < colors.Length; ++i) {
			blend.Positions[i * 2] = i * step;
			blend.Positions[i * 2 + 1] = (i + 1) * step;
			blend.Colors[i * 2] = blend.Colors[i * 2 + 1] = colors[i];
		}
		brush.InterpolationColors = blend;
		return brush;
	}

	static Color MultiSpacePerimeterColor => Color.Gold;
	static Color SpaceColor( Space space )
		=> space.IsWetland ? Color.LightBlue
		: space.IsSand ? Color.PaleGoldenrod
		: space.IsMountain ? Color.Gray
		: space.IsJungle ? Color.ForestGreen
		: space.IsOcean ? Color.Blue
		: Color.Gold;
	#endregion Multi-Space

	#region private fields

	readonly SharedCtx _ctx;
	IDecision _decision;

	// Screen Bounds - set immediatly (nothing to calculate)
	event Action ScreenBoundsChanged;
	Rectangle _availableScreenRect;


	// World Layout - lazy calculated
	WorldLayoutOfIsland WorldLayout => _worldLayout ??= new WorldLayoutOfIsland( _ctx.GameState.Island );
	WorldLayoutOfIsland _worldLayout;
	event Action WorldLayoutChanged;


	// World to Screen mapping.
	PointMapper _mapper; // Maps from normallized space to Client space.
	int _iconWidth;
	const int _hotspotRadius = 40; // Radius when targetting space or virtual token

	Rectangle _usedBoardScreenRect;
	Bitmap _cachedBackground;

	// Cleared at beginning of Space-Draw and collects subscripts as we go
	readonly List<RectangleSubscript> _transientSubscripts = new List<RectangleSubscript>();

	// Available Options
	readonly VisibleButtonContainer _buttonContainer = new VisibleButtonContainer();
	readonly HashSet<SpaceToken> _outstandingSpaceTokenOptions = new HashSet<SpaceToken>();


	static Color SpacePerimeterColor => Color.Black;
	static Brush SpaceLabelBrush => Brushes.White;
	static Color SacredSiteColor => Color.Yellow;
	static Color ArrowColor => Color.DeepSkyBlue;

	#endregion

}

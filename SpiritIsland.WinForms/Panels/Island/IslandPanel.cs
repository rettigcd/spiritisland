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

		WorldLayoutChanged += InitButtonContainerToSpaceButtons; // !! instead of Init, switch to invalidate
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
		// Background & Initialization
		if(_cachedBackground == null) {
			MapWorldToScreen();
			InitBackgroundCache();
		}
		graphics.DrawImage( _cachedBackground, _usedBoardScreenRect );

		// Load all SpaceToken options into the Outstanding-SpaceToken collection.
		_buttonContainer.ClearTransient();
		if(_decision is A.SpaceTokenDecision spaceTokenDecision)
			_outstandingSpaceTokenOptions.UnionWith( spaceTokenDecision.SpaceTokens );
		if(_decision is A.MyTokenOn tokenOnDecision)
			_outstandingSpaceTokenOptions.UnionWith( tokenOnDecision.TokensOn.OfType<SpaceToken>() );

		// As we draw the space, we pull matched SpaceTokens out of the collection and place in buttonContainer
		foreach(Space space in ActionScope.Current.Spaces_Unfiltered) {  // !!! this is WRONG - just use GameState SpaceStaets.

			var paintableSpace = WorldLayout.GetPaintable( space.SpaceSpec );
			// Init and paint
			paintableSpace.Space = space;
			paintableSpace.PaintAbove( graphics );

			// record locations
			foreach(var pair in paintableSpace.Locations)
				RecordSpaceTokenLocation( pair.Key, pair.Value );

			// debug
			if(_ctx._debug)
				paintableSpace.PaintAbove_Debug(graphics);
		}

		// for SpaceToken options with no real SpaceToken, put them in buttonContainer too.
		AddButtonsForVirtualSpaceTokens();

		DrawArrows( graphics );

		// Draw enabled buttons in the container
		_buttonContainer.Paint( graphics );
	}

	public void ActivateOptions( IDecision decision ) {
		_decision = decision;

		// Spaces
		_outstandingSpaceTokenOptions.Clear();
		if(_decision is A.SpaceTokenDecision spaceTokenDecision)
			_outstandingSpaceTokenOptions.UnionWith( spaceTokenDecision.SpaceTokens );

		// Tokens
		_buttonContainer.EnableOptions( decision );

		this.OptionCount = _buttonContainer.ActivatedOptions + _outstandingSpaceTokenOptions.Count;
	}

	public IClickable GetClickableAction( Point clientCoords ) {
		IOption option = _buttonContainer.FindEnabledOption( clientCoords );
		return option == null ? null : new GenericClickable( ()=>_ctx.SelectOption( option ) );
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
		DrawBoardSpacesOnly( graphics, _ctx.GameState.SpaceSpecs_Unfiltered );
	}

	void MapWorldToScreen() {
		// Mapping part 1 set used Board Screen Rect
		RectangleF worldBounds = WorldLayout.Bounds;
		Size islandWorldSize = worldBounds.Scale( 1000 ).ToInts().Size;
		_usedBoardScreenRect = _availableScreenRect.FitBoth( islandWorldSize, Align.Center, Align.Near );

		// Mapping part 2 set world-to-screen transform
		_mapper = PointMapper.FromWorldToViewport( worldBounds.ToBounds(), _usedBoardScreenRect.ToBounds() );

		_iconWidth = (int)(_mapper.UnitLength * .075f);//  gw_boardScreenRect.Width / 20; // !!! scale tokens based on board/space size, NOT widow size (for 2 boards, tokens are too big)
	}

	void CacheBackground_Invalidate() {
		if(_cachedBackground != null) {
			_cachedBackground.Dispose();
			_cachedBackground = null;
		}
	}

	void DrawBoardSpacesOnly( Graphics graphics, IEnumerable<SpaceSpec> spaces ) {
		using Pen perimeterPen = new Pen( SpacePerimeterColor, 5f );

		foreach(var space in spaces) {
			var paintable = _worldLayout.GetPaintable( space );
			paintable.Mapper = _mapper;
			paintable.Paint( graphics );
		}

	}

	PointF MapWorldToClient( PointF world) => _mapper.Map(world.ToXY()).ToPointF();
	XY MapWorldToClientXY( XY world ) => _mapper.Map( world );

	void DrawArrows( Graphics graphics ) {
		if(_decision is not A.IHaveArrows quiver) return;
		using Pen pushArrowPen = new Pen( ArrowColor, 7 );
		foreach(A.Arrow arrow in quiver.Arrows)
			graphics.DrawArrow( pushArrowPen, 
				GetPortPoint( arrow.From, arrow.Token ), 
				GetPortPoint( arrow.To, arrow.Token )
			);

		Point GetPortPoint( SpaceSpec space, IToken visibileTokens ) {
			XY worldCoord = WorldLayout.InsidePoints( space ).GetPointFor( visibileTokens );
			return _mapper.Map( worldCoord ).ToInts();
		}
	}

	#endregion

	#region Buttons

	void InitButtonContainerToSpaceButtons() {
		_buttonContainer.Clear();
		foreach(Space space in ActionScope.Current.Spaces_Unfiltered) { // !!! Don't use ActionScope here, just use raw GameState.
			SpaceLayout layout = WorldLayout.InsidePoints(space.SpaceSpec).SpaceLayout;
			SpaceButton button = new SpaceButton( layout, MapWorldToClientXY ); // Can't use _mapper.Map directly because it has not been initialized AND it might change.
			_buttonContainer.Add( space.SpaceSpec, button );
		}
	}

	void RecordSpaceTokenLocation( SpaceToken st, Rectangle rect ) {
		if(_outstandingSpaceTokenOptions.Contains( st )) {
			_buttonContainer.AddTransientEnabled( st, new SpaceTokenButton( rect ) );
			_outstandingSpaceTokenOptions.Remove( st );
		}
	}

	// For Options that are SpaceTokens, calc and draw their rectangles
	void AddButtonsForVirtualSpaceTokens() {
		foreach(SpaceToken spaceToken in _outstandingSpaceTokenOptions) {
			Point p = _mapper.Map( WorldLayout.InsidePoints( spaceToken.Space.SpaceSpec ).GetPointFor( spaceToken.Token ) ).ToInts();
			Rectangle rect = new Rectangle( p.X - _hotspotRadius, p.Y - _hotspotRadius, _hotspotRadius * 2, _hotspotRadius * 2 ); // !!! use token size, whatever that is.
			_buttonContainer.AddTransientEnabled( spaceToken, new SpaceTokenButton( rect ) );
		}
		_outstandingSpaceTokenOptions.Clear();
	}

	#endregion

	#region private fields

	readonly SharedCtx _ctx;
	IDecision _decision;

	// Screen Bounds - set immediatly (nothing to calculate)
	event Action ScreenBoundsChanged;
	Rectangle _availableScreenRect;


	/// <summary>
	/// Recalculated each time game-layout changed.
	/// </summary>
	WorldLayoutOfIsland WorldLayout => _worldLayout ??= new WorldLayoutOfIsland( _ctx.GameState.Island );
	WorldLayoutOfIsland _worldLayout;
	event Action WorldLayoutChanged;


	// World to Screen mapping.
	PointMapper _mapper; // Maps from normallized space to Client space.
	int _iconWidth;
	const int _hotspotRadius = 40; // Radius when targetting space or virtual token

	Rectangle _usedBoardScreenRect;
	Bitmap _cachedBackground;

	// Available Options
	readonly VisibleButtonContainer _buttonContainer = new VisibleButtonContainer();
	readonly HashSet<SpaceToken> _outstandingSpaceTokenOptions = [];


	static Color SpacePerimeterColor => Color.Black;
	static Brush SpaceLabelBrush => Brushes.White;
	static Color SacredSiteColor => Color.Yellow;
	static Color ArrowColor => Color.DeepSkyBlue;

	#endregion

}

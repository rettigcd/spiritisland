//using Android.OS;
namespace SpiritIsland.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics.Text;

public class SpaceWidget : OptionView {

	public IOption Option => _space;

	Color _glowColor = Colors.Transparent;

	public OptionState State { 
		get => _state;
		set {
			if(_state == value) return;
			_state = value;

			_glowColor = _state switch {
				OptionState.Selected => Color.FromRgba( 0, 255, 0, 144 ),
				OptionState.IsOption => Color.FromRgba( 255, 0, 0, 144 ),
				_ => Colors.Transparent,
			};

			_graphicsView.Invalidate();
		}
	}

	public SpaceState Tokens { get; private set; }

	#region constructors

	public SpaceWidget(SpaceState spaceState, SpaceLayout layout, PointMapper mapper, AbsoluteLayout abs, GraphicsView graphicsView ) {
		_space = spaceState.Space;
		_layout = layout;
		_insidePoints = new ManageInternalPoints( layout );
		_terrainColor = GetTerrainColor();

		// mapper
		_mapper = mapper;
		_iconWidth = (int)(mapper.UnitLength * _insidePoints.TokenSize);

		// Boundary / perimeter
		_outter = _layout.Corners.Select( mapper.Map ).ToArray();
		_inner = Polygons.InnerPoints( _outter, -GLOW_WIDTH );

		_abs = abs;
		_graphicsView = graphicsView;

		Tokens = spaceState;
	}

	#endregion constructors

	public bool Contains( PointF clientCoords ) {
		// !!! check Bounds in screen coords here TOO
		// !!! don't recalc these every time...
		XY[] cornersInScreenCoords = _layout.Corners.Select( _mapper.Map ).ToArray();
		return Polygons.IsInside( cornersInScreenCoords, clientCoords.ToXY() );
	}

	public void Click() => SelectOptionCallback?.Invoke(Option,false);

	public Action<IOption,bool>? SelectOptionCallback { get; set; }

	public TokenLocationView GetSpaceTokenView( SpaceToken st ) {
		return st.Space != _space ? throw new ArgumentException( "Wrong space" )
			: _visibleTokens.TryGetValue( st.Token, out TokenLocationView? stv ) ? stv
			: throw new ArgumentException( $"Token {st.Token} not found in {st.Space}" );
	}

	#region Floating background

	//void InitBackgroundShape( XY[] points, AbsoluteLayout abs ) {
	//	(_spaceShape, Rect bounds) = MakePathFromPoints( points );
	//	// style
	//	_spaceShape.BackgroundColor = GetTerrainColor();
	//	_spaceShape.Stroke = Colors.Black;
	//	// tap
	//	TapGestureRecognizer tap = new TapGestureRecognizer();
	//	tap.Tapped += Tap_Tapped;
	//	_spaceShape.GestureRecognizers.Add( tap );
	//	// add it
	//	abs.Float( _spaceShape, bounds );
	//}

	//void InitGlow( XY[] points, AbsoluteLayout abs ) {
	//	(_glow, Rect glowBounds) = MakePathFromPoints( points, _inner );
	//	abs!.Float( _glow, glowBounds );
	//}

	//void InitLabel( AbsoluteLayout abs ) {
	//	Point center = Mapper.Map( _layout.Center ).ToPoint();
	//	Point topLeft = new Point( center.X - _iconWidth / 2, center.Y - _iconWidth / 2 );
	//	_spaceLabel = new Label { Text = _space.Label, FontSize = 10f, TextColor = Colors.White };
	//	abs.Float( _spaceLabel, new Rect( topLeft.X, topLeft.Y, _iconWidth, _iconWidth ) );
	//}
	// readonly Path? _spaceShape;
	// Path? _glow;
	// Label? _spaceLabel;
	#endregion Floating background

	public void DrawBackground( ICanvas canvas ) {

		// Outline
		PointF[] points = _outter.Select( x => x.ToPointF() ).ToArray();
		PathF path = new PathF();
		path.MoveTo( points[^1] );
		for(int j = 0; j < points.Length; j++)
			path.LineTo( points[j] );
		// Fill
		canvas.FillColor = _terrainColor;
		canvas.FillPath( path );
		// Stroke
		canvas.StrokeColor = Colors.Black;
		canvas.DrawPath( path );

		// Glow
		if(_glowColor != Colors.Transparent) {
			points = _inner.Select( x => x.ToPointF() ).ToArray();
			for(int j = points.Length - 1; 0 <= j; --j)
				path.LineTo( points[j] );
			path.LineTo( points[^1] );

			// Fill
			canvas.FillColor = _glowColor;
			canvas.FillPath( path );
		}

		// Internal points
		// DrawPoints( canvas, _insidePoints.InternalPoints, Colors.Blue );
		// DrawPoints( canvas, _insidePoints.BorderPoints, Colors.Red );
		// DrawPoints( canvas, _insidePoints.NamePoints, Colors.Green );


		// Label
		XY center = _mapper.Map( _insidePoints.NameLocation );
		XY topLeft = new XY( center.X - _iconWidth / 2, center.Y - _iconWidth / 2 );
		canvas.FontSize = 10f;
		canvas.FontColor = Colors.White;
		DrawText( canvas, Tokens.Space.Label, topLeft.X, topLeft.Y, _iconWidth, _iconWidth );
	}

	void DrawPoints( ICanvas canvas, XY[] points, Color color ) {
		canvas.StrokeColor = color; 
		canvas.StrokeSize = 2;
		foreach(XY pt in points) {
			var screen = _mapper.Map( pt );
			canvas.DrawLine( screen.X - 1, screen.Y, screen.X + 1, screen.Y );
		}
	}

	void DrawText( ICanvas canvas, string text, float x, float y, float w, float h ) {

		try{ 
			ITextAttributes attr1 = new TextAttributes();
			IAttributedTextRun run1 = new AttributedTextRun( 0, 200, attr1 );
			var text1 = new AttributedText( text, [run1] );
			canvas.DrawText( text1, x, y, _iconWidth * 3, _iconWidth );
		}
		catch {
			// !!! NOT IMPLEMENTED ON WINDOWS
		}
	}

	public void SyncVisibleTokensToSpaceState(OptionViewManager ovm) {

		if(Tokens is null || _abs is null) return;
		_insidePoints.Init( Tokens );

		IToken[] orderedTokens = [ ..Tokens.Keys.OfType<IToken>().OrderBy( OrderTokens ) ];

		// Do Remove
		IToken[] toRemove = _visibleTokens.Keys.Except( orderedTokens ).ToArray();
		foreach(var old in toRemove) {
			var stv = _visibleTokens[old];
			_abs.Children.Remove(stv);
			_visibleTokens.Remove(old);
			OptionView? ov = stv.BindingContext as OptionView;
			if(ov is not null)
				ovm.Remove(ov);
		}

		// Refresh tokens neither added or removed. (Do this BEFORE we Add the new ones.)
		var updateCount = orderedTokens.Intersect( _visibleTokens.Keys );
		foreach(var update in updateCount)
			_visibleTokens[update].Model!.RefreshCountAndSS();

		// Do Add
		IToken[] toAdd = orderedTokens.Except( _visibleTokens.Keys ).ToArray();
//		string s = $"Ordered({orderedTokens.Length}) - Visible({_visibleTokens.Keys.Count()}) => ToAdd( {toAdd.Length} )";
		foreach(IToken token in toAdd) {
			TokenLocationView? stv = FloatToken(_abs,token);
			if(stv is not null && stv.Model is not null)
				ovm.Add(stv.Model);
		}

		static int OrderTokens( IToken t ) => t is not HumanToken human ? 0
			: -human.FullHealth * 100       // healthy first
				+ human.StrifeCount * 10   // minimum strife
				+ human.RemainingHealth;     // most damaged

	}

	public void ReleaseViews(OptionViewManager ovm) {
		if(_abs is null) return;
		foreach(TokenLocationView view in _visibleTokens.Values) {
			_abs.Remove( view );
			if(view.Model is not null)
				ovm.Remove( view.Model );
		}
		_visibleTokens.Clear();

		//foreach(View? view in new View?[]{_spaceShape, _spaceLabel, _glow } )
		//	if(view != null)
		//		_abs.Remove(view);
		//_spaceLabel = null;
		//_spaceLabel = null;
		//_glow = null;
	}

	#region private methods

	TokenLocationView? FloatToken( AbsoluteLayout abs, IToken token ) {
		if(token is not IAppearInSpaceAbreviation) return null;

		// calc rect
		XY center = _mapper.Map( _insidePoints.GetPointFor( token ) );
		XY topLeft = new XY( center.X - _iconWidth / 2, center.Y - _iconWidth / 2 );

		// build token
		var st = new SpaceToken(Tokens, token);

		var vm = new TokenLocationModel( st );
		TokenLocationView view = new TokenLocationView { 
			ZIndex = 2, 
			BindingContext = vm
		};

		// add
		_visibleTokens[token] = view;
		abs.Float( view, new Rect( topLeft.X, topLeft.Y, _iconWidth, _iconWidth ) );
		return view;
	}

	(Path, Rect) MakePathFromPoints( XY[] outter, XY[]? inner = null ) {
		var outterPathFigure = new PathFigure { StartPoint = outter[0].ToPoint(), IsClosed = true };
		for(int j = 1; j < outter.Length; ++j)
			outterPathFigure.Segments.Add( new LineSegment( outter[j].ToPoint() ) );

		float minX = outter.Min( x => x.X );
		float minY = outter.Min( x => x.Y );

		PathFigure? innerPathFigure = null;
		if(inner is not null) {
			innerPathFigure = new PathFigure { StartPoint = inner[0].ToPoint(), IsClosed = true };
			for(int j = 1; j < inner.Length; ++j)
				innerPathFigure.Segments.Add( new LineSegment( inner[j].ToPoint() ) );
		}

		var path = new Path {
			Data = new PathGeometry( innerPathFigure is null ? [outterPathFigure] : [outterPathFigure, innerPathFigure] ),
			Aspect = Stretch.Uniform,
		};

		// the -1s are because there is a 1 pixel offset from graphcs and canvas
		var bounds = new Rect( minX -1, minY-1, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize );
		return (path, bounds);
	}

	Color GetTerrainColor() => _space is Space1 s1 ? s1.NativeTerrain.GetColor() : Colors.White;

	void Tap_Tapped( object? sender, TappedEventArgs e ) {
	}

	#endregion

	#region private fields

	readonly int _iconWidth;
	OptionState _state;

	readonly AbsoluteLayout _abs;
	readonly GraphicsView _graphicsView;

	readonly PointMapper _mapper;
	readonly XY[] _outter;
	readonly XY[] _inner;
	readonly Dictionary<IToken, TokenLocationView> _visibleTokens = [];
	readonly Color _terrainColor;
	readonly SpaceLayout _layout;
	readonly ManageInternalPoints _insidePoints;
	readonly Space _space;

	const float GLOW_WIDTH = 10f;

	#endregion
}

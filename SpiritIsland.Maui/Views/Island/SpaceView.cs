//using Android.OS;

namespace SpiritIsland.Maui;

using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;


public class SpaceWidget {

	public readonly ObservableCollection<TokenLocationModel> Tokens;

	Color _glowColor = Colors.Transparent;

	public Space Space => _model.Space;

	#region constructors

	public SpaceWidget(SpaceModel model, PointMapper mapper, AbsoluteLayout abs, GraphicsView graphicsView ) {
		_model = model;
		Tokens = model.Tokens;
		_spaceSpec = Space.SpaceSpec;

		_insidePoints = new ManageInternalPoints( model.Layout );
		_terrainColor = GetTerrainColor();

		// mapper
		_mapper = mapper;
		_iconWidth = (int)(mapper.UnitLength * _insidePoints.TokenSize);

		// Boundary / perimeter
		_outter = _layout.Corners.Select( mapper.Map ).ToArray();
		_inner = Polygons.InnerPoints( _outter, -GLOW_WIDTH );

		_absLayout = abs;
		_graphicsView = graphicsView;

		Tokens.CollectionChanged += Tokens_CollectionChanged;

		_insidePoints.Init(Space);

		foreach (TokenLocationModel tlModel in model.Tokens)
			FloatToken(_absLayout, tlModel);

		model.PropertyChanged += Model_PropertyChanged;

	}
	public readonly SpaceModel _model;

	void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
		if(e.PropertyName == nameof(_model.State) ) {
			_glowColor = _model.State switch {
				OptionState.Selected => Color.FromRgba(0, 255, 0, 144),
				OptionState.IsOption => Color.FromRgba(255, 0, 0, 144),
				_ => Colors.Transparent,
			};
			_graphicsView.Invalidate();
		}
	}

	#endregion constructors

	public bool Contains( PointF clientCoords ) {
		// !!! check Bounds in screen coords here TOO
		// !!! don't recalc these every time...
		XY[] cornersInScreenCoords = _layout.Corners.Select( _mapper.Map ).ToArray();
		return Polygons.IsInside( cornersInScreenCoords, clientCoords.ToXY() );
	}

	public void Click() { 
		_model.ClickCommand.Execute(null);
	}

	public TokenLocationView GetSpaceTokenView( SpaceToken st ) {
		return st.Space.SpaceSpec != _spaceSpec ? throw new ArgumentException( "Wrong space" )
			: _visibleTokens.TryGetValue( st.Token, out TokenLocationView? stv ) ? stv
			: throw new ArgumentException( $"Token {st.Token} not found in {st.Space.SpaceSpec}" );
	}

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

		// Label
		XY center = _mapper.Map( _insidePoints.NameLocation );
		XY topLeft = new XY( center.X - _iconWidth / 2, center.Y - _iconWidth / 2 );
		canvas.FontSize = 10f;
		canvas.FontColor = Colors.White;
		DrawText( canvas, Space.SpaceSpec.Label, topLeft.X, topLeft.Y, _iconWidth, _iconWidth );
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

	void Tokens_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		switch( e.Action) {
			case NotifyCollectionChangedAction.Add:
				foreach (TokenLocationModel tlModel in e.NewItems!)
					FloatToken(_absLayout, tlModel);
				break;
			case NotifyCollectionChangedAction.Remove:
				foreach (TokenLocationModel tlModel in e.OldItems!) {
					IToken token = tlModel.TokenLocation.Token;
					TokenLocationView view = _visibleTokens[token];
					_visibleTokens.Remove(token);
					_absLayout.Children.Remove(view);
				}
				break;
			case NotifyCollectionChangedAction.Reset:
				if(Tokens.Count == 0) {
					foreach(var oldView in _visibleTokens.Values)
						_absLayout.Children.Remove(oldView);
					_visibleTokens.Clear();
				} else {
					// !! what just happened.  What kind of reset did we get?
				}
				break;
			default:
				break;
		}
	}

	#region private methods

	TokenLocationView? FloatToken( AbsoluteLayout abs, TokenLocationModel tlModel ) {

		TokenLocationView view = new TokenLocationView { 
			ZIndex = 2, 
			BindingContext = tlModel
		};

		// add
		_visibleTokens[tlModel.TokenLocation.Token] = view;

		XY center = _mapper.Map( _insidePoints.GetPointFor(tlModel.TokenLocation.Token) );// calc center
		XY topLeft = new XY(center.X - _iconWidth / 2, center.Y - _iconWidth / 2);
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

	Color GetTerrainColor() => _spaceSpec is SingleSpaceSpec s1 
		? s1.NativeTerrain.GetColor() 
		: Colors.DarkSlateGray; // Multi-spaces & Endless Dark

	void Tap_Tapped( object? sender, TappedEventArgs e ) {
	}

	#endregion

	#region private fields

	readonly int _iconWidth;

	readonly AbsoluteLayout _absLayout;
	readonly GraphicsView _graphicsView;

	readonly PointMapper _mapper;
	readonly XY[] _outter;
	readonly XY[] _inner;
	readonly Dictionary<IToken, TokenLocationView> _visibleTokens = [];
	readonly Color _terrainColor;
	SpaceLayout _layout => _model.Layout;
	readonly ManageInternalPoints _insidePoints;
	readonly SpaceSpec _spaceSpec;

	const float GLOW_WIDTH = 10f;

	#endregion
}

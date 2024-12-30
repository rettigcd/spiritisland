//using Android.OS;

using Microsoft.Maui.Graphics.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SpiritIsland.Maui;

public class SpaceWidget {

	public Space Space => Model.Space;

	public readonly SpaceModel Model;

	#region constructors

	/// <summary>
	/// Will automatically Float new Tokens so don't create new ones for ones that already exist.
	/// </summary>
	public SpaceWidget(SpaceModel model, PointMapper mapper, AbsoluteLayout absLayout, GraphicsView graphicsView ) {
		Model = model;
		_spaceSpec = Space.SpaceSpec;
		_mapper = mapper;

		_insidePoints = new ManageInternalPoints(model.Layout);
		_iconWidth = (int)(mapper.UnitLength * _insidePoints.TokenSize);

		// Boundary / perimeter
		_outer = CalcOuter(_mapper, Model );
		_inner = CalcInner( _outer );

		_terrainColor = GetTerrainColor(); // after _outter initialized so we can make 2nd ring

		_absLayout = absLayout;
		_graphicsView = graphicsView;

		_insidePoints.Init(Space);

		foreach (TokenLocationModel tlModel in model.Tokens)
			FloatToken(tlModel);

		// events
		model.Tokens.CollectionChanged += ModelTokens_CollectionChanged;
		model.PropertyChanged += Model_PropertyChanged;
	}

	static XY[] CalcOuter(PointMapper mapper, SpaceModel model)
		=> model.Layout.Corners.Select(mapper.Map).ToArray();

	static XY[] CalcInner(XY[] outer)	=> Polygons.InnerPoints( outer, -GLOW_WIDTH );

	static XY[] Calc2ndInner(XY[] outer) => Polygons.InnerPoints(outer, -GLOW_WIDTH * 1.5f);


	void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
		if(e.PropertyName == nameof(Model.State) ) SpaceStateChanged();
	}

	void SpaceStateChanged() {
		_glowColor = Model.State switch {
			OptionState.Selected => Color.FromRgba(0, 255, 0, 144),
			OptionState.IsOption => Color.FromRgba(255, 0, 0, 144),
			_ => Colors.Transparent,
		};
		_graphicsView.Invalidate();
	}

	#endregion constructors

	/// <summary>
	/// When Boards change orientation, their mapper needs updated.
	/// </summary>
	public void UpdateMapper(PointMapper mapper) {
		_mapper = mapper;
		_outer = CalcOuter(_mapper, Model);
		_inner = CalcInner(_outer);
		if(_secondColorInner is not null)
			_secondColorInner = Calc2ndInner(_outer);

		_insidePoints = new ManageInternalPoints(Model.Layout);
		_iconWidth = (int)(mapper.UnitLength * _insidePoints.TokenSize);

		// Move out tokens
		foreach( TokenLocationView view in _visibleTokens.Values )
			view.UpdatePosition(GetCenter(view.Model), _iconWidth );

	}

	public bool Contains( PointF clientCoords ) {
		// !!! check Bounds in screen coords here TOO
		// !!! don't recalc these every time...
		XY[] cornersInScreenCoords = Model.Layout.Corners.Select( _mapper.Map ).ToArray();
		return Polygons.IsInside( cornersInScreenCoords, clientCoords.ToXY() );
	}

	public void Click() { 
		Model.ClickCommand.Execute(null);
	}

	public TokenLocationView GetSpaceTokenView( SpaceToken st ) {
		return st.Space.SpaceSpec != _spaceSpec ? throw new ArgumentException( "Wrong space" )
			: _visibleTokens.TryGetValue( st.Token, out TokenLocationView? stv ) ? stv
			: throw new ArgumentException( $"Token {st.Token} not found in {st.Space.SpaceSpec}" );
	}

	public void DrawBackground( ICanvas canvas ) {

		// Outline
		PointF[] points = _outer.Select( x => x.ToPointF() ).ToArray();
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

		// 2nd color
		if(_secondColorInner is not null )
			DrawWidePerimeter(canvas, path, _secondColorInner, _secondColor! );

		// Glow
		if(_glowColor != Colors.Transparent)
			DrawWidePerimeter(canvas, path, _inner, _glowColor);

		// Label
		XY center = _mapper.Map( _insidePoints.NameLocation );
		XY topLeft = new XY( center.X - _iconWidth / 2, center.Y - _iconWidth / 2 );
		canvas.FontSize = 10f;
		canvas.FontColor = Colors.White;
		DrawText( canvas, Space.SpaceSpec.Label, topLeft.X, topLeft.Y, _iconWidth, _iconWidth );
	}

	static void DrawWidePerimeter(ICanvas canvas, PathF path, XY[] myInner, Color myColor) {

		int startingSegments = path.Count;

		var points2 = myInner.Select(x => x.ToPointF()).ToArray();
		for( int j = points2.Length - 1; 0 <= j; --j )
			path.LineTo(points2[j]);
		path.LineTo(points2[^1]);

		// Fill
		canvas.FillColor = myColor;
		canvas.FillPath(path);

		path.RemoveAllSegmentsAfter(startingSegments);
		if(path.Count != startingSegments)
			throw new Exception($"{path.Count} <> {startingSegments}");
	}

	void DrawText( ICanvas canvas, string text, float x, float y, float _/*w*/, float _1/*h*/ ) {

		try{
			//			Microsoft.Maui.Graphics.Text.ITextAttributes attr1 = new TextAttributes();
			var attr1 = new TextAttributes();

			IAttributedTextRun run1 = new AttributedTextRun( 0, 200, attr1 );
			var text1 = new AttributedText( text, [run1] );
			canvas.DrawText( text1, x, y, _iconWidth * 3, _iconWidth );
		}
		catch {
			// !!! NOT IMPLEMENTED ON WINDOWS
		}
	}

	void ModelTokens_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		switch( e.Action) {
			case NotifyCollectionChangedAction.Add:
				foreach (TokenLocationModel tlModel in e.NewItems!)
					FloatToken(tlModel);
				break;
			case NotifyCollectionChangedAction.Remove:
				foreach (TokenLocationModel tlModel in e.OldItems!)
					UnFloat(tlModel.TokenLocation.Token);
				break;
			case NotifyCollectionChangedAction.Reset:
				if(Model.Tokens.Count != 0) 
					throw new InvalidOperationException("What just happened?  How did we get a reset and still have tokens?");
				// On Reset, .OldItems is null, have to use our copy of the tokens
				foreach( IToken token in _visibleTokens.Keys.ToArray() )
					UnFloat(token);
				break;
			default:
				break;
		}
	}

	#region private methods

	void FloatToken( TokenLocationModel tlModel ) {

		TokenLocationView view = new TokenLocationView {
			BindingContext = tlModel
		};

		// add to our lookup list
		_visibleTokens[tlModel.TokenLocation.Token] = view;

		// add to the View
		view.Float( _absLayout, GetCenter(tlModel), _iconWidth );
	}

	XY GetCenter(TokenLocationModel tlModel) {
		return _mapper.Map(_insidePoints.GetPointFor(tlModel.TokenLocation.Token));
		// calc center
	}

	void UnFloat(IToken token) {
		_absLayout.Children.Remove(_visibleTokens[token]);
		_visibleTokens.Remove(token);
	}

	Color GetTerrainColor() {
		if( _spaceSpec is SingleSpaceSpec s1 )
			return s1.NativeTerrain.GetColor(); // Multi-spaces & Endless Dark

		if( _spaceSpec is MultiSpaceSpec ms && ms.OrigSpaces.Length == 2 ) {
			_secondColorInner = Calc2ndInner(_outer);
			_secondColor = ms.OrigSpaces[0].NativeTerrain.GetColor();
			return ms.OrigSpaces[1].NativeTerrain.GetColor();
		}

		return Colors.DarkSlateGray; // Multi-spaces & Endless Dark
	}

	void Tap_Tapped( object? sender, TappedEventArgs e ) {
	}

	#endregion

	#region private fields

	PointMapper _mapper; // not readonly, changes when boards are moved
	XY[] _outer;
	XY[] _inner;
	XY[]? _secondColorInner = null;
	Color? _secondColor = null;
	ManageInternalPoints _insidePoints;
	int _iconWidth;

	readonly AbsoluteLayout _absLayout;
	readonly GraphicsView _graphicsView;
	readonly Dictionary<IToken, TokenLocationView> _visibleTokens = [];
	readonly Color _terrainColor;
	readonly SpaceSpec _spaceSpec;

	const float GLOW_WIDTH = 10f;
	Color _glowColor = Colors.Transparent;

	#endregion
}

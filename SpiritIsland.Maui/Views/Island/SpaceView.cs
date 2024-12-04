//using Android.OS;

using Microsoft.Maui.Graphics.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SpiritIsland.Maui;

public class SpaceWidget {

	public Space Space => Model.Space;

	#region constructors

	/// <summary>
	/// Will automatically Float new Tokens so don't create new ones for ones that already exist.
	/// </summary>
	public SpaceWidget(SpaceModel model, PointMapper mapper, AbsoluteLayout absLayout, GraphicsView graphicsView ) {
		Model = model;
		_spaceSpec = Space.SpaceSpec;

		_insidePoints = new ManageInternalPoints( model.Layout );
		_terrainColor = GetTerrainColor();

		// mapper
		_mapper = mapper;
		_iconWidth = (int)(mapper.UnitLength * _insidePoints.TokenSize);

		// Boundary / perimeter
		_outter = _layout.Corners.Select( mapper.Map ).ToArray();
		_inner = Polygons.InnerPoints( _outter, -GLOW_WIDTH );

		_absLayout = absLayout;
		_graphicsView = graphicsView;

		_insidePoints.Init(Space);

		foreach (TokenLocationModel tlModel in model.Tokens)
			FloatToken(tlModel);

		// events
		model.Tokens.CollectionChanged += ModelTokens_CollectionChanged;
		model.PropertyChanged += Model_PropertyChanged;
	}
	public readonly SpaceModel Model;

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

	public bool Contains( PointF clientCoords ) {
		// !!! check Bounds in screen coords here TOO
		// !!! don't recalc these every time...
		XY[] cornersInScreenCoords = _layout.Corners.Select( _mapper.Map ).ToArray();
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
			ZIndex = 2, 
			BindingContext = tlModel
		};

		// add to our lookup list
		_visibleTokens[tlModel.TokenLocation.Token] = view;

		// add to the View
		XY center = _mapper.Map( _insidePoints.GetPointFor(tlModel.TokenLocation.Token) );// calc center
		XY topLeft = new XY(center.X - _iconWidth / 2, center.Y - _iconWidth / 2);
		_absLayout.Float( view, new Rect( topLeft.X, topLeft.Y, _iconWidth, _iconWidth ) );
	}

	void UnFloat(IToken token) {
		_absLayout.Children.Remove(_visibleTokens[token]);
		_visibleTokens.Remove(token);
	}

	Color GetTerrainColor() => _spaceSpec is SingleSpaceSpec s1 
		? s1.NativeTerrain.GetColor() 
		: Colors.DarkSlateGray; // Multi-spaces & Endless Dark

	void Tap_Tapped( object? sender, TappedEventArgs e ) {
	}

	#endregion

	#region private fields

	SpaceLayout _layout => Model.Layout;

	readonly int _iconWidth;

	readonly AbsoluteLayout _absLayout;
	readonly GraphicsView _graphicsView;

	readonly PointMapper _mapper;
	readonly XY[] _outter;
	readonly XY[] _inner;
	readonly Dictionary<IToken, TokenLocationView> _visibleTokens = [];
	readonly Color _terrainColor;
	readonly ManageInternalPoints _insidePoints;
	readonly SpaceSpec _spaceSpec;

	const float GLOW_WIDTH = 10f;
	Color _glowColor = Colors.Transparent;

	#endregion
}

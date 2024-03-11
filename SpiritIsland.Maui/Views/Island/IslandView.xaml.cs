namespace SpiritIsland.Maui;

public partial class IslandView : ContentView, IDrawable {

	public Tokens_ForIsland IslandTokens {
		get {
			return _tokens;
		}
		set {
			_tokens = value;
		}
	}
	Tokens_ForIsland _tokens = new Tokens_ForIsland();

	public Board? Board { 
		get => _board;
		set {
			_board = value;
			if(_board is null) {
				_boardLayout = null;
				_spaces = [];
			} else {
				_boardLayout = BoardLayout.Get( _board.Name );
				PointMapper mapper = CalcMapper( _boardLayout );
				_spaces = _board.Spaces_Unfiltered
					.Select(s=>new SpaceWidget(_tokens[s], _boardLayout.ForSpace(s), mapper, Abs, IslandGraphicsView ))
					.ToArray();
			}
			RegisterSpaces();
			SyncTokensToGameState();
		}
	}

	public OptionViewManager? Ovm { 
		get => _ovm;
		set {
			_ovm = value;
			RegisterSpaces();
		}
	}
	OptionViewManager? _ovm;

	void RegisterSpaces() {
		if(_ovm is null || _spaces.Length == 0) return;
		foreach(var space in _spaces)
			_ovm.Add(space);
	}

	#region constructor

	public IslandView() {
		InitializeComponent();
		IslandGraphicsView.Drawable = this;
	}
	
	#endregion constructor

	public void SyncTokensToGameState() {
		if(_ovm is null) return;
		foreach(SpaceWidget space in _spaces)
			space.SyncVisibleTokensToSpaceState(_ovm);
	}

	public void ReleaseTokens() {
		if (_ovm is null) return;
		foreach (SpaceWidget space in _spaces)
			space.ReleaseViews(_ovm);
	}

	#region private

	SpaceWidget? GetSpaceUnderPoint( PointF point ) {
		foreach(SpaceWidget space in _spaces) {
			if(space.Contains( point ))
				return space;
		}
		return null;
	}

	void IslandGraphicsView_StartInteraction( object sender, TouchEventArgs e ) {
		// if(1 < e.Touches.Length) return;
		SpaceWidget? spaceWidget = GetSpaceUnderPoint( e.Touches[0] );
		spaceWidget?.Click();
	}

	void IDrawable.Draw( ICanvas canvas, RectF dirtyRect ) {
		if(_boardLayout == null) return;
		if(_board == null) return;

		foreach(var space in _spaces)
			space.DrawBackground( canvas );
	}

	static PointMapper CalcMapper( BoardLayout boardLayout ) {
		Bounds screenBounds = new Bounds( 0, 0, 390, 240 );
		Bounds worldBounds = boardLayout!.Bounds;
		return PointMapper.FromWorldToViewport(
			worldBounds,
			screenBounds.FitBoth( worldBounds.Size )
		);
	}

	Board? _board;
	BoardLayout? _boardLayout;
	SpaceWidget[] _spaces = [];

	#endregion private
}

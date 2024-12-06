namespace SpiritIsland.Maui;

public partial class IslandView : ContentView, IDrawable {

	// updating this triggers a ui update
	//public string Spaces { get => (string)GetValue(SpacesProperty); set => SetValue(SpacesProperty, value); }
	public static readonly BindableProperty ModelProperty = BindableProperty.Create(nameof(Model), typeof(IslandModel), typeof(IslandView), propertyChanged: (containingObj, oldValue, newValue) => { 
		IslandView iv = (IslandView)containingObj;
		iv.InitSpaces( oldValue as IslandModel, newValue as IslandModel );
	});

	public IslandModel? Model {
		get => (IslandModel?)GetValue(ModelProperty);
		set => SetValue(ModelProperty, value);
	}

	void InitSpaces(IslandModel? oldModel, IslandModel? newModel) {
		if(oldModel is not null) 
			throw new Exception("(Q1) How the hell are we replacing the Island model?");
		if(newModel is null) 
			throw new Exception("(Q2) Why the hell are we clearing out the model?");
		//if( newModel == null ) { _spaceWidgets = []; return; }

		newModel.SpacesChanged += Model_SpacesChanged;

		var mapper = CalcMapper(newModel.WorldBounds);
		_spaceWidgets = newModel.Spaces
			.Select(model => new SpaceWidget(model, mapper, Abs, IslandGraphicsView))
			.ToArray();
	}

	void Model_SpacesChanged(IslandModel.SpacesChangedEventArgs obj) {
		var model = Model!;
		var mapper = CalcMapper(model.WorldBounds);

		var widgetsWeAreKeeping = _spaceWidgets.Where(w => !obj.Removed.Contains(w.Model)).ToArray();
		foreach(var widget in widgetsWeAreKeeping)
			widget.UpdateMapper( mapper );

		_spaceWidgets = [
			.. widgetsWeAreKeeping,
			.. obj.Added.Select(model => new SpaceWidget(model, mapper, Abs, IslandGraphicsView))
		];

		//_spaceWidgets = model.Spaces
		//	.Select(model => new SpaceWidget(model, mapper, Abs, IslandGraphicsView))
		//	.ToArray();
		IslandGraphicsView.Invalidate();
	}

	PointMapper CalcMapper(Bounds notUsedworldBounds) {

		// The following line is return width/height of -1
		// Bounds screenBounds = new Bounds(0, 0, (float)IslandGraphicsView.Width, (float)IslandGraphicsView.Height);
		Bounds screenBounds = new Bounds(0, 0, 390, 240);

		// Determine Rotation that creates the smallest height
		var corners = Model!.Corners;

		return PointMapper.FitPointsInViewportHeight(screenBounds, Model!.Corners, 30, 60, 90, 120, 150);
		// screenBounds.FitBoth(bestWorldBounds.Size),

	}

	#region constructor

	public IslandView() {
		InitializeComponent();
		IslandGraphicsView.Drawable = this;
	}
	
	#endregion constructor

	#region private methods

	SpaceWidget? GetSpaceUnderPoint(PointF point) {
		foreach (SpaceWidget space in _spaceWidgets) {
			if (space.Contains(point))
				return space;
		}
		return null;
	}

	void IslandGraphicsView_StartInteraction(object sender, TouchEventArgs e) {
		// if(1 < e.Touches.Length) return;
		SpaceWidget? spaceWidget = GetSpaceUnderPoint(e.Touches[0]);
		spaceWidget?.Click();
	}

	void IDrawable.Draw(ICanvas canvas, RectF dirtyRect) {
		foreach (var space in _spaceWidgets)
			space.DrawBackground(canvas);
	}

	#endregion private methods

	#region private fields

	SpaceWidget[] _spaceWidgets = [];

	#endregion private fields

}

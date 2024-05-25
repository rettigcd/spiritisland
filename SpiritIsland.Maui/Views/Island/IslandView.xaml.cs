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

	void InitSpaces(IslandModel? _, IslandModel? newModel) {
		if( newModel == null ) {
			_spaceWidgets = [];
			return;
		}
		// Assumes there is only 1 board.
		var mapper = CalcMapper(newModel.WorldBounds);
		_spaceWidgets = newModel._spaceModels
			.Select(model => {
				return new SpaceWidget(model, mapper, Abs, IslandGraphicsView);
			})
			.ToArray();
	}

#pragma warning disable CA1822 // Mark members as static
	PointMapper CalcMapper(Bounds worldBounds) {

		// The following line is return width/height of -1
		// Bounds screenBounds = new Bounds(0, 0, (float)IslandGraphicsView.Width, (float)IslandGraphicsView.Height);
		Bounds screenBounds = new Bounds(0, 0, 390, 240);

		return PointMapper.FromWorldToViewport(
			worldBounds,
			screenBounds.FitBoth(worldBounds.Size)
		);
	}
#pragma warning restore CA1822 // Mark members as static

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

using Microsoft.Maui.Layouts;

namespace SpiritIsland.Maui;

/// <summary>
/// Displays the Token-part (count,SS,selected) of a Token-Location 
/// </summary>
public partial class TokenLocationView : ContentView {

	public TokenLocationModel Model => (TokenLocationModel)BindingContext;

	public TokenLocationView() {
		ZIndex = 2;

		InitializeComponent();
	}

	protected override void OnBindingContextChanged() {
		base.OnBindingContextChanged();
		Model.PropertyChanged += Model_PropertyChanged;
	}

	void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
		if(_absLayout is null || e.PropertyName != nameof(Model.State)) return;
		bool makeBig = Model.State == OptionState.Selected;
		Size(makeBig ? _defaultWidth*3/2 : _defaultWidth);
		ZIndex = makeBig ? 12 : 2;
	}

	#region Set Position / Size

	public void Float(AbsoluteLayout abs, XY center, int width) {
		_absLayout = abs;
		_center = center;
		_defaultWidth = width;

		// add to the View
		_absLayout.Add(this);
		Size(_defaultWidth);
		_absLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.None);
	}

	public void UpdatePosition(XY center, int width) {
		_center = center;
		_defaultWidth = width;
		_currentWidth = -1; // force redraw

		Size(_defaultWidth);
	}

	void Size(int width) {
		if(width == _currentWidth) return;
		_currentWidth = width;
		XY topLeft = new XY(_center.X - width / 2, _center.Y - width / 2);
		var bounds = new Rect(topLeft.X, topLeft.Y, width, width);
		_absLayout.SetLayoutBounds(this, bounds);
	}

	#endregion Set Position / Size

	#region Tapped

	// Copied from Growth
	void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) {
		if (++_tapCount == 1)
			HandleClick();
	}
	int _tapCount = 0;
	async void HandleClick() {
		await Task.Delay(400);
		await MainThread.InvokeOnMainThreadAsync(() => {
			switch (_tapCount) {
				case 1: Model?.Select(false); break;
				case 2: Model?.Select(true); break;
				default: break;
			}
			_tapCount = 0;
		});
	}

	#endregion

	#region private fields

	AbsoluteLayout? _absLayout;
	XY _center;
	int _defaultWidth;
	int _currentWidth = -1;

	#endregion private fields

}
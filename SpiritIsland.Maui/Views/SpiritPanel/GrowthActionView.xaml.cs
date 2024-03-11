//using Android.Health.Connect.DataTypes;

namespace SpiritIsland.Maui;

/// <summary>
/// Displays 1 Growth Action
/// </summary>
public partial class GrowthActionView : ContentView {
	
	public GrowthActionView() {
		InitializeComponent();
	}

	#region private Gesture handler

	void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) {
		if(++_tapCount == 1)
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
	GrowthActionModel? Model => BindingContext as GrowthActionModel;

	void DragGestureRecognizer_DragStarting(object sender, DragStartingEventArgs e) {
		e.Data.Properties.Add("GrowthActionModel", ((GrowthActionModel)BindingContext));
	}


	#endregion private


}

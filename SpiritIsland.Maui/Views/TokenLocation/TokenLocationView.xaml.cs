namespace SpiritIsland.Maui;

/// <summary>
/// Displays the Token-part (count,SS,selected) of a Token-Location 
/// </summary>
public partial class TokenLocationView : ContentView {

	public TokenLocationModel Model => (TokenLocationModel)BindingContext;

	public TokenLocationView() {
		InitializeComponent();
	}

//	void TapGestureRecognizer_Tapped( object sender, TappedEventArgs e ) => Model?.Select();

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


}
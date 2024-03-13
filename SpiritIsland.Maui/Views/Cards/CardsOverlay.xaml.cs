namespace SpiritIsland.Maui;

public partial class CardsOverlay : ContentView {

	public CardsOverlay() {
		InitializeComponent();
	}

	void Play_Clicked(object sender, EventArgs e) {
		Model.AcceptCards();
		Hide();
	}

	void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) {
		Model.ResetDetails();
		Hide();
	}

	void Hide() {
		Model.IsVisible = false;
	}

	CardsModel Model => (CardsModel)BindingContext;

}
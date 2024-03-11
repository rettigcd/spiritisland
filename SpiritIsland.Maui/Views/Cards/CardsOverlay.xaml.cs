namespace SpiritIsland.Maui;

public partial class CardsOverlay : ContentView {

	public CardsOverlay() {
		InitializeComponent();
	}

	void Play_Clicked(object sender, EventArgs e) {
		Model.AcceptCards();
		IsVisible = false;
	}

	void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) {
		Model.ResetDetails();
		IsVisible = false;
	}

	CardsModel Model => (CardsModel)BindingContext;

}
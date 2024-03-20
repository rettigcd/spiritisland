namespace SpiritIsland.Maui;

public partial class CardsOverlay : ContentView {

	public CardsOverlay() {
		InitializeComponent();
	}

	void Accept_Clicked(object sender, EventArgs e) {
		Model.AcceptCards();
		Hide();
	}

	void Background_Tapped(object sender, TappedEventArgs e) {
		Model.ResetDetails();
		Hide();
	}

	void Hide() {
		Model.IsVisible = false;
	}

	CardsOverlayModel Model => (CardsOverlayModel)BindingContext;

}
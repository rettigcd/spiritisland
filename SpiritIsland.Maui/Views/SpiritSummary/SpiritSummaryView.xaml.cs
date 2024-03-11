namespace SpiritIsland.Maui;

public partial class SpiritSummaryView : ContentView {

	public SpiritSummaryView() {
		InitializeComponent();
	}

	void Spirit_Clicked(object? sender, TappedEventArgs e) {
		GrowthDetailsClicked?.Invoke(this, e);
	}

	void CardButton_Clicked(object? sender, EventArgs e) {
		CardDetailsClicked?.Invoke(this, e);
	}

	public event EventHandler<EventArgs>? GrowthDetailsClicked;
	public event EventHandler<EventArgs>? CardDetailsClicked;

}
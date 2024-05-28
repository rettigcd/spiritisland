using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using SpiritIsland.JaggedEarth;

namespace SpiritIsland.Maui; 
public partial class MainPage : ContentPage {

	public MainPage() {
		InitializeComponent();
		PowerCard[] cards = [
			PowerCard.For(typeof(SleepAndNeverWaken)),
			PowerCard.For(typeof(FlashFloods)),
			PowerCard.For(typeof(WashAway)),
			PowerCard.For(typeof(GrowthThroughSacrifice)),
		];

		var slots = cards.Select(card => new CardSlotModel(new CardModel(card))).ToList();
		for( int i = 0; i < 2; ++i )
			slots.Add(new CardSlotModel());

		Slots.ItemsSource = slots;

		// ======
		ReportOrientation();

		DeviceDisplay.Current.MainDisplayInfoChanged += Current_MainDisplayInfoChanged;

	}

	void Current_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e) {
		ReportOrientation();
	}

	void DragGestureRecognizer_DragStarting(object sender, DragStartingEventArgs e) {}

	private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) {}

	private void CircleDrag_Started(object sender, DragStartingEventArgs e) {
		e.Data.Properties.Add("IsValidShape", true);
		ReportOrientation();
	}

	private static void ReportOrientation() {
		Shell.Current.DisplayAlert("Orientation", DeviceDisplay.Current.MainDisplayInfo.Orientation.ToString(), "Ok");
		// IDeviceDislpay - add to use
	}

}

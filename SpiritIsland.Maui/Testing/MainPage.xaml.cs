using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using SpiritIsland.JaggedEarth;

namespace SpiritIsland.Maui; 
public partial class MainPage : ContentPage {

	public MainPage() {
		InitializeComponent();
		//Card1.BindingContext = new CardModel(PowerCard.For(typeof(FlashFloods)));
		//Card2.BindingContext = new CardModel(PowerCard.For(typeof(BoonOfVigor)));
		//Card3.BindingContext = new CardModel(PowerCard.For(typeof(RiversBounty)));
		//Card4.BindingContext = new CardModel(PowerCard.For(typeof(WashAway)));

		//Card1.BindingContext = new CardModel(PowerCard.For(typeof(SleepAndNeverWaken)));
		//Card2.BindingContext = new CardModel(PowerCard.For(typeof(VigorOfTheBreakingDawn)));
		//Card3.BindingContext = new CardModel(PowerCard.For(typeof(BloodwrackPlague)));
		//Card4.BindingContext = new CardModel(PowerCard.For(typeof(GrowthThroughSacrifice)));

		PowerCard[] cards = [
			PowerCard.For(typeof(SleepAndNeverWaken)),
			PowerCard.For(typeof(FlashFloods)),
			PowerCard.For(typeof(WashAway)),
			PowerCard.For(typeof(GrowthThroughSacrifice)),
		];

		var slots = cards.Select(card=>new CardSlotModel(new CardModel(card))).ToList();
		for(int i=0;i<2;++i)
			slots.Add(new CardSlotModel());

		Slots.ItemsSource = slots;

	}

	private void DragGestureRecognizer_DragStarting(object sender, DragStartingEventArgs e) {

	}

	private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) {

	}

	private void CircleDrag_Started(object sender, DragStartingEventArgs e) {
		e.Data.Properties.Add("IsValidShape", true);
	}

	private void Shape_Drop(object _, DropEventArgs e) {
		bool isValidShape = (bool)e.Data.Properties["IsValidShape"];

//		validationLabel.IsVisible = true;

		if (isValidShape) {
			//((sender as Element)?.Parent as Frame).BackgroundColor = Color.FromArgb("#FF2B0B98");

			//shapeLable.Text = "Circle";
			//shapeLable.TextColor = Colors.White;

			//circleFrame.IsVisible = false;
			//squareFrame.IsVisible = false;
			//headerLabel.IsVisible = false;

			//validationLabel.TextColor = Colors.Green;
			//validationLabel.Text = "Valid shape dropped!";
		} else {
			//validationLabel.TextColor = Colors.Red;
			//validationLabel.Text = "Drop valid shape here!";
		}
	}
}

namespace SpiritIsland.Maui;

public partial class CardSlotView : ContentView {

	public CardSlotView() {
		InitializeComponent();
	}

	void Button_Clicked(object sender, EventArgs e) {
		if(BindingContext is not CardSlotModel model) return;
		model.HasCard = !model.HasCard;
	}

	#region Drag-n-Drop

	void DragGestureRecognizer_DragStarting(object sender, DragStartingEventArgs e) {
		e.Data.Properties.Add("Card",((CardSlotModel)this.BindingContext).Card);
		e.Data.Properties.Add("SourceSlot",this);
	}

	Color? _origColor;
	void DropGestureRecognizer_DragOver(object sender, DragEventArgs e) {
		if(_origColor == null) {
			_origColor = DropArea.BackgroundColor;
			DropArea.BackgroundColor = Colors.White;
		}
	}

	void DropGestureRecognizer_DragLeave(object sender, DragEventArgs e) {
		RestoreBackground();
	}

#pragma warning disable IDE0051 // Remove unused private members
	void DropGestureRecognizer_Drop(object _, DropEventArgs e) {
		CardModel cardModel = (CardModel)e.Data.Properties["Card"];
		CardSlotView oldSlot = (CardSlotView)e.Data.Properties["SourceSlot"];
		CardSlotModel oldModel = oldSlot.Model;

		RestoreBackground();

		oldModel.Card = null;
		Model.Card = cardModel;
	}
#pragma warning restore IDE0051 // Remove unused private members

	void RestoreBackground() {
		if (_origColor is not null) {
			DropArea.BackgroundColor = _origColor;
			_origColor = null;
		}
	}

	CardSlotModel Model {
		get => (CardSlotModel)BindingContext;
	}
	#endregion Drag-n-Drop

	void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) {
		Model.Selected = true;
	}
}

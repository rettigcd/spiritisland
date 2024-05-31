namespace SpiritIsland.Maui;

public partial class SpiritPanelView : ContentView {

	public SpiritPanelView() {
		InitializeComponent();
	}

	#region Drag-n-Drop Growth

	void DropGestureRecognizer_DragOver(object _, DragEventArgs e) {
		SpiritBackdrop.StrokeThickness = 20;
	}

#pragma warning disable IDE0051 // Remove unused private members
	void DropGestureRecognizer_Drop(object _, DropEventArgs e) {
		GrowthActionModel? growthActionModel = (GrowthActionModel?)e.Data.Properties["GrowthActionModel"];
		growthActionModel?.Select(true);
		RestoreSpritBgColor();
		_model.TryToClose();
		e.Handled = true; // stops auto-stuff like hiding image and setting text
	}
#pragma warning restore IDE0051 // Remove unused private members

	void DropGestureRecognizer_DragLeave(object _, DragEventArgs e) {
		RestoreSpritBgColor();
	}

	void RestoreSpritBgColor() {
		SpiritBackdrop.StrokeThickness = 0;
	}

	#endregion Drag-n-Drop Growth

	void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) {
		_model.TryToClose();
	}

	SpiritPanelModel _model => (SpiritPanelModel)BindingContext;
}
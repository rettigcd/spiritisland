namespace SpiritIsland.Maui;

public partial class SpiritPanelView : ContentView {

	public SpiritPanelView() {
		InitializeComponent();
	}

	#region Drag-n-Drop Growth

	void DropGestureRecognizer_DragOver(object _, DragEventArgs e) {
		SpiritBackdrop.StrokeThickness = 20;
	}

	void DropGestureRecognizer_DragLeave(object _, DragEventArgs e) {
		RestoreSpritBgColor();
	}
	void RestoreSpritBgColor() {
		SpiritBackdrop.StrokeThickness = 0;
	}

#pragma warning disable IDE0051 // Remove unused private members
	void DropGestureRecognizer_Drop(object _, DropEventArgs e) {

		GrowthActionModel? model = (GrowthActionModel?)e.Data.Properties["GrowthActionModel"];
		model?.Select(true);
		RestoreSpritBgColor();

		_model.TryToClose();
	}
#pragma warning restore IDE0051 // Remove unused private members


	#endregion Drag-n-Drop Growth

	void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) {
		_model.TryToClose();
	}

	SpiritPanelModel _model => (SpiritPanelModel)BindingContext;
}
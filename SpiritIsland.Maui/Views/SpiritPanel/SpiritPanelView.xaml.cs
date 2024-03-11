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

	void DropGestureRecognizer_Drop(object _, DropEventArgs e) {

		GrowthActionModel? model = (GrowthActionModel?)e.Data.Properties["GrowthActionModel"];
		model?.Select(true);
		RestoreSpritBgColor();

		IsVisible = false;
	}


	#endregion Drag-n-Drop Growth

	private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) {
		IsVisible = false;
	}
}
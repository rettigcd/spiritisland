namespace SpiritIsland.Maui;

public partial class LogView : ContentPage {

	public LogView(LogModel model) {
		BindingContext = model;
		InitializeComponent();
	}

}
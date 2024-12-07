namespace SpiritIsland.Maui;

public class TogglableModel : ObservableModel {
	public required string Text { get; set; }
	public bool Selected { get=>_selected; set=>SetProp(ref _selected,value); }
	bool _selected;
}

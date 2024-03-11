namespace SpiritIsland.Maui;

public class CardSlotModel : ObservableModel1 {

	#region Observable Properties

	public bool HasCard      { get => GetStruct<bool>(); set => SetProp(value); }
	public bool Selected     { get => GetStruct<bool>(); set => SetProp(value); }
	public Color BorderColor { get => GetProp<Color>(); set => SetProp(value); }
	public CardModel? Card   { 
		get => GetNullableProp<CardModel>(); 
		set {
			SetProp(value);
			HasCard = value is not null;
			BorderColor = value is not null && value.IsDraggable ? Colors.Red : Colors.DimGray;
		}
	}

	#endregion Observable Properties

	public CardSlotModel( CardModel? card=null ) {
		Card = card;
	}

}

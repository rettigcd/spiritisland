using System.Windows.Input;

namespace SpiritIsland.Maui;

public class CardSlotModel : ObservableModel1 {

	#region Observable Properties

	public bool HasCard      { get => GetStruct<bool>(); set => SetProp(value); }
	public bool Selected     { get => GetStruct<bool>(); set => SetProp(value); }

	public CardModel? Card   { 
		get => GetNullableProp<CardModel>();
		set {
			SetProp(value);
			HasCard = value is not null;
		}
	}

	public ICommand SelectCommand { get; }

	#endregion Observable Properties

	#region constructor

	public CardSlotModel( CardModel? card=null ) {
		Card = card;
		SelectCommand = new Command( ()=> Selected = true );
	}

	#endregion constructor

}

//using Android.OS;


using SpiritIsland.SinglePlayer;
using System.Windows.Input;

namespace SpiritIsland.Maui;

public class GameStatusModel : ObservableModel1 {

	public int RewindableRound { get => GetStruct<int>(); set => SetProp(value); }
	public ICommand RewindCommand { get; }

	public Phase Phase { get => GetStruct<Phase>(); set { SetProp(value); } }

	#region Invader Cards - Observable

	public ImageSource[] RavageImages { get => GetProp<ImageSource[]>(); set => SetProp(value); }
	public ImageSource[] BuildImages { get => GetProp<ImageSource[]>(); set => SetProp(value); }
	public int InvaderStage { get => GetInt(); set => SetProp(value); }
	public int RemainingInvaderDeckCards { get => GetInt(); set => SetProp(value); }

	#endregion Invader Cards - Observable

	#region Fear - Observable

	public int FearPool                    { get => GetInt(); set => SetProp(value); }
	public int ActivatedFearCards          { get => GetInt(); set => SetProp(value); }
	public int RemainingCardsInTerrorLevel { get => GetInt(); set => SetProp(value); }
	public int TerrorLevel                 { 
		get => GetInt(); 
		set => SetProp(value);
	}

	public int BlightOnCard { get => GetInt(); set => SetProp(value); }

//	public int EarnedFear { get => GetInt(); set => SetProp(value); }
//	public int FearPoolSize { get => GetInt(); set => SetProp(value); }
//	public int[] FearCardsRemaining { get => GetProp<int[]>(); set => SetProp(value); }

	#endregion Fear - Observable

	public GameStatusModel( SinglePlayerGame game ) {
		var gs = game.GameState;
		_deck = gs.InvaderDeck;
		_fear = gs.Fear;
		_blightPool = gs.Tokens[SpiritIsland.BlightCard.Space];

		RavageImages = [];
		BuildImages = [];

		Phase = gs.Phase;

		game.GameState.NewLogEntry += GameState_NewLogEntry;
		UpdatePhaseStuff(SpiritIsland.Phase.Init);
		UpdateFear();
		UpdateBlight();

		RewindCommand = new Command(
			execute: () => {
				if (0 < RewindableRound) {
					// var a = await DisplayAlert("Question?", "Would you like to rewind to Round N?", "Yes", "No");
					game.UserPortal.RewindToRound(RewindableRound);
					--RewindableRound;
				}
			},
			canExecute: () => true // 0 < RewindableRound
		);
	}

	void GameState_NewLogEntry(Log.ILogEntry obj) {
		if(obj is Log.Phase phaseEntry)
			UpdatePhaseStuff(phaseEntry.phase);
		else if(obj is Log.BlightOnCardChanged)
			UpdateBlight();
		else if (obj is Log.FearGenerated)
			UpdateFear();
	}

	void UpdateFear() {
		// Fear
		FearPool = _fear.PoolMax - _fear.EarnedFear;
		ActivatedFearCards = _fear.ActivatedCards.Count;
		RemainingCardsInTerrorLevel = _fear.CardsPerLevel.FirstOrDefault(x => x != 0);
		TerrorLevel = _fear.TerrorLevel;
	}

	void UpdateBlight() {
		BlightOnCard = _blightPool[Token.Blight];
	}

	void UpdatePhaseStuff(Phase phase) {
		// Phase
		Phase = phase;
		// Invaders
		RavageImages = GetInvaderFlipped(_deck.Ravage.Cards);
		BuildImages = GetInvaderFlipped(_deck.Build.Cards);
		InvaderStage = _deck.InvaderStage;
		RemainingInvaderDeckCards = _deck.UnrevealedCards.Count;
		// remove activated Fear
		ActivatedFearCards = _fear.ActivatedCards.Count;
	}

	#region private methods

	#endregion

	#region private Invader Card Helpers

	static ImageSource[] GetInvaderFlipped(List<InvaderCard> cards) {

		List<ImageSource> imgs = [];
		foreach(var card in cards) {
			if (card.Filter is SingleTerrainFilter singleTerrain)
				imgs.Add( singleTerrain.Terrain.ToIcon().ImgSource() );
			else if (card.Filter is DoubleTerrainFilter doubleTerrain) {
				imgs.Add(doubleTerrain.Terrain1.ToIcon().ImgSource() );
				imgs.Add(doubleTerrain.Terrain2.ToIcon().ImgSource());
			} else if( card.Filter.Text == CoastalFilter.Name )
				imgs.Add(Img.Icon_Ocean.ImgSource());
			else if(card.Filter.Text.Contains("Salt"))
				imgs.Add(ImageCache.FromFile("salt_deposits.png"));
		}
		return [..imgs];
	}

	static string GetInvaderBackside(InvaderCard? card) {
		return card is null ? "--" : $"Stage {card.InvaderStage}";
	}

	#endregion private Invader Card Helpers

	readonly InvaderDeck _deck;
	readonly Fear _fear;
	readonly Space _blightPool;
}
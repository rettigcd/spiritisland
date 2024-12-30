using SpiritIsland.SinglePlayer;

namespace SpiritIsland.Maui;

public class InvaderBoardModel : ObservableModel {

	#region Invader Cards - Observable

	public bool HasHighBuild { get => _hasHighBuild; set => SetProp(ref _hasHighBuild, value); }
	public ImageSource[] HighBuildImages { get => _highBuildImages; set => SetProp(ref _highBuildImages, value); }
	public ImageSource[] RavageImages { get => _ravageImages; set => SetProp(ref _ravageImages, value); }
	public ImageSource[] BuildImages { get => _buildImages; set => SetProp(ref _buildImages,value); }
	public int InvaderStage { get => _invaderStage; set => SetProp(ref _invaderStage,value); }
	public int RemainingInvaderDeckCards { get => _remainingInvaderDeckCards; set => SetProp(ref _remainingInvaderDeckCards,value); }

	ImageSource[] _highBuildImages;
	ImageSource[] _ravageImages;
	ImageSource[] _buildImages;
	int _invaderStage;
	int _remainingInvaderDeckCards;
	bool _hasHighBuild;

	#endregion Invader Cards - Observable

	#region Fear - Observable

	public int FearPool                    { get => _fearPool; set => SetProp(ref _fearPool,value); }
	public int ActivatedFearCards          { get => _activatedFearCards; set => SetProp(ref _activatedFearCards,value); }
	public int RemainingCardsInTerrorLevel { get => _remainingCardsInTerrorLevel; set => SetProp(ref _remainingCardsInTerrorLevel,value); }
	public int TerrorLevel                 { get => _terrorLevel; set => SetProp(ref _terrorLevel,value); }

	int _fearPool;
	int _activatedFearCards;
	int _remainingCardsInTerrorLevel;
	int _terrorLevel;

	#endregion Fear - Observable

	#region Blight - Observable

	public int BlightOnCard { get => _blightOnCard; set => SetProp(ref _blightOnCard, value); }
	int _blightOnCard;

	#endregion Blight - Observable

	#region constructor

	public InvaderBoardModel( SoloGame game ) {
		var gs = game.GameState;
		_deck = gs.InvaderDeck;
		_fear = gs.Fear;
		_blightPool = gs.BlightCard;
		_ravageImages = [];
		_buildImages = [];
		_highBuildImages = [];

		game.GameState.NewLogEntry += GameState_NewLogEntry;

		UpdateInvaderCards();
		UpdateFear();
		UpdateBlight(_blightPool.BlightCount);
	}

	#endregion constructor

	#region private methods

	void GameState_NewLogEntry(Log.ILogEntry obj) {
		if(obj is Log.Phase)
			UpdateInvaderCards();
		else if(obj is Log.BlightOnCardChanged x)
			UpdateBlight(x.NewCount);
		else if (obj is Log.FearGenerated)
			UpdateFear();
	}

	void UpdateFear() {
		FearPool = _fear.PoolMax - _fear.EarnedFear;
		ActivatedFearCards = _fear.ActivatedCards.Count;
		TerrorLevel = _fear.TerrorLevel;
		RemainingCardsInTerrorLevel = _fear.CardsPerLevel_Remaining.FirstOrDefault(x => x != 0);
	}

	void UpdateBlight(int count) {
		BlightOnCard = count;
	}

	void UpdateInvaderCards() {
		// Invaders
		RavageImages = GetInvaderFlipped(_deck.Ravage.Cards);
		BuildImages = GetInvaderFlipped(_deck.Build.Cards);
		InvaderStage = _deck.InvaderStage;
		RemainingInvaderDeckCards = _deck.UnrevealedCards.Count;

		HasHighBuild = (_deck.ActiveSlots[0] != _deck.Ravage);
		if(HasHighBuild)
			HighBuildImages = GetInvaderFlipped(_deck.ActiveSlots[0].Cards);

		// remove activated Fear
		ActivatedFearCards = _fear.ActivatedCards.Count;
	}

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

	//static string GetInvaderBackside(InvaderCard? card) {
	//	return card is null ? "--" : $"Stage {card.InvaderStage}";
	//}

	#endregion private Invader Card Helpers

	readonly InvaderDeck _deck;
	readonly Fear _fear;
	readonly SpiritIsland.BlightCard _blightPool;
}
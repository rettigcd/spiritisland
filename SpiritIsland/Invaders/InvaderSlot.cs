namespace SpiritIsland;

/// <summary>
/// The Engine + card management
/// </summary>
public abstract class InvaderSlot : IHaveMemento {

	public InvaderSlot(string label ) { Label = label;}
	public string Label { get; }
	public List<InvaderCard> Cards { get; } = new List<InvaderCard>();
	public void HoldNextBack() { _holdBackCount++; }
	public void SkipNextNormal() { _skipCount++; }
	public virtual async Task Execute( GameState gs ) {
		foreach(var card in Cards)
			if(0 < _skipCount)
				_skipCount--;
			else
				await ActivateCard(card,gs);
		await ActionComplete.InvokeAsync(gs);
	}

	/// <summary>
	/// All Builds, or all Explores, or all Ravages
	/// </summary>
	public readonly AsyncEvent<GameState> ActionComplete = new AsyncEvent<GameState>();

	public virtual List<InvaderCard> GetCardsToAdvance() {
		var result = new List<InvaderCard>();
		for(int i=0; i < Cards.Count; ++i)
			if(0 < _holdBackCount)
				--_holdBackCount;
			else {
				result.Add(Cards[i]);
				Cards.RemoveAt(i--); // post-decrement is correct
			}
		return result;
	}

	public abstract Task ActivateCard( InvaderCard card, GameState gameState);

	object IHaveMemento.Memento {
		get => new MyMemento(this);
		set => ((MyMemento)value).Restore(this);
	}

	class MyMemento {
		public MyMemento(InvaderSlot slot) {
			_cards = slot.Cards.ToArray();
			_flipped = _cards.Select(x=>x.Flipped).ToArray();
			_skipCount = slot._skipCount;
			_holdBackCount = slot._holdBackCount;
		}
		public void Restore(InvaderSlot slot ) {
			slot.Cards.SetItems( _cards );
			for(int i=0;i< _cards.Length; ++i) slot.Cards[i].Flipped = _flipped[i];
			slot._skipCount = _skipCount;
			slot._holdBackCount = _holdBackCount;
		}
		readonly int _skipCount;
		readonly int _holdBackCount;
		readonly InvaderCard[] _cards;
		readonly bool[] _flipped;
	}

	int _skipCount = 0;
	protected int _holdBackCount = 0;
}

namespace SpiritIsland;

/// <summary>
/// The Engine + card management
/// </summary>
public abstract class InvaderSlot( string label ) : IHaveMemento {

	public string Label { get; } = label; public List<InvaderCard> Cards { get; } = [];
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

	#region Json

	/// <summary> Same field list as MyMemento below. Engine isn't captured - like MyMemento, it's
	/// assumed setup-time-stable, not per-turn state. </summary>
	public JsonArray ToJson() => new JsonArray(
		_skipCount, _holdBackCount,
		new JsonArray( Cards.Select( c => (JsonNode)c.ToJson() ).ToArray() )
	);

	/// <summary> Restores onto this existing slot (Explore/Build/Ravage are fixed, always-present
	/// instances on an InvaderDeck) rather than constructing a new one - InvaderSlot is abstract and
	/// its subclasses carry their own pluggable Engine that this doesn't try to reconstruct. </summary>
	public void RestoreFromJson( JsonArray json ) {
		_skipCount = json[0]!.GetValue<int>();
		_holdBackCount = json[1]!.GetValue<int>();
		Cards.SetItems( ( (JsonArray)json[2]! ).Select( n => InvaderCardRegistry.Deserialize( (JsonArray)n! ) ).ToArray() );
	}

	#endregion Json

	object IHaveMemento.Memento {
		get => new MyMemento(this);
		set => ((MyMemento)value).Restore(this);
	}

	class MyMemento {
		public MyMemento(InvaderSlot slot) {
			_cards = [..slot.Cards];
			_flipped = [.._cards.Select(x=>x.Flipped)];
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

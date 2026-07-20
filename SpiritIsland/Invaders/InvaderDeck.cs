namespace SpiritIsland;

public class InvaderDeck : IHaveMemento {

	#region constructors

	/// <summary>
	/// 
	/// </summary>
	/// <param name="cards">Prepared cards for the game.</param>
	/// <param name="leftOver">Remaining cards that were not part of the game.</param>
	public InvaderDeck( List<InvaderCard> cards, Queue<InvaderCard>[] leftOver ) {
		_unrevealedCards = cards;
		_leftOverCards = leftOver;
		InitNumberOfCardsToDraw();
		_activeSlots = [ Ravage, Build, Explore ];
	}

	void InitNumberOfCardsToDraw() {
		// Setup draw: 1 card at a time.
		for(int i = 0; i < UnrevealedCards.Count; ++i) 
			_drawCount.Add( 1 );
	}

	#endregion

	public List<InvaderCard> UnrevealedCards => _unrevealedCards;
	// ??? Should Unused, Unrevealed, and Discard be slots also?  (Just not in the ActiveSlots) list
	public ExploreSlot Explore { get; } = new ExploreSlot();
	public BuildSlot Build { get; } = new BuildSlot();
	public RavageSlot Ravage { get; } = new RavageSlot();

	public InvaderCard TakeNextUnused( int selectionLevel ) => _leftOverCards[selectionLevel - 1].Dequeue();

	public List<InvaderSlot> ActiveSlots {
		get {  return _activeSlots; }
		set { _activeSlots = value; }
	}

	public List<InvaderCard> Discards {get;} = [];

	public int InvaderStage => (Explore.Cards.FirstOrDefault() ?? UnrevealedCards.First()).InvaderStage;

	/// <summary>
	/// Triggers Ravage / 
	/// </summary>
	public async Task AdvanceAsync() {

		var destination = Discards;
		foreach(var slot in ActiveSlots) {
			var cardsToMove = slot.GetCardsToAdvance();
			destination.AddRange( cardsToMove );
			destination = slot.Cards;
		}

		await InitExploreSlotAsync();
	}

	/// <summary>
	/// Moves 1 (or more) cards from the unrealed deck to the Explore deck.
	/// </summary>
	public async Task InitExploreSlotAsync() {
		if(UnrevealedCards.Count == 0) return; // !!! Should this throw a GameOver(Loss) exception?
		int count = _drawCount[0]; _drawCount.RemoveAt( 0 );
		while(0 < count--) {
			var unrevealedCard = UnrevealedCards[0];
			UnrevealedCards.RemoveAt( 0 );
			Explore.Cards.Add( unrevealedCard );
			await unrevealedCard.OnReveal();
		}
	}

	#region private fields

	List<InvaderSlot> _activeSlots;
	readonly List<InvaderCard> _unrevealedCards;
	readonly List<int> _drawCount = []; // tracks how many cards to draw each turn
	readonly Queue<InvaderCard>[] _leftOverCards; // cards not drawn and made part of the deck.  (Russia only)

	#endregion

	#region Json

	/// <summary>
	/// Named keys - same field list as MyMemento below. _leftOverCards isn't captured - like the
	/// CardRevealed event subscriptions InvaderCard doesn't serialize, it's consumed only during
	/// adversary setup (Russia's TakeNextUnused), fully drained before the game becomes interactively
	/// playable, and isn't captured by MyMemento either.
	///
	/// ActiveSlots is [ [ "Fixed", label ] | [ "Extra", extraSlotJson ], ... ] - Explore/Build/
	/// Ravage resolve by label against this deck's own always-present instances (as before); anything
	/// else (e.g. England's HighImmegrationSlot, inserted by adversary setup - roadmap section 9) must
	/// implement ISerializableInvaderSlot and resolves via InvaderSlotRegistry. Re-establishing that an
	/// adversary-added slot's IRunWhenTimePasses subscription (if any) is still needed after restore is
	/// out of scope here - that's hook-action-list territory (section 10), not built yet.
	/// </summary>
	public JsonObject ToJson() => new JsonObject {
		["UnrevealedCards"] = new JsonArray( UnrevealedCards.Select( c => (JsonNode)c.ToJson() ).ToArray() ),
		["Discards"] = new JsonArray( Discards.Select( c => (JsonNode)c.ToJson() ).ToArray() ),
		["DrawCounts"] = new JsonArray( _drawCount.Select( n => (JsonNode)n ).ToArray() ),
		["ActiveSlots"] = new JsonArray( ActiveSlots.Select( s => (JsonNode)SerializeActiveSlot( s ) ).ToArray() ),
		["Explore"] = Explore.ToJson(),
		["Build"] = Build.ToJson(),
		["Ravage"] = Ravage.ToJson()
	};

	JsonArray SerializeActiveSlot( InvaderSlot slot ) {
		if( ReferenceEquals( slot, Explore ) || ReferenceEquals( slot, Build ) || ReferenceEquals( slot, Ravage ) )
			return new JsonArray( "Fixed", slot.Label );
		if( slot is ISerializableInvaderSlot extra )
			return new JsonArray( "Extra", extra.ToJson() );
		throw new NotSupportedException( $"InvaderDeck.ToJson doesn't know how to serialize InvaderSlot of type {slot.GetType().Name} - implement ISerializableInvaderSlot and register a reader in InvaderSlotRegistry." );
	}

	/// <remarks> Builds through the normal constructor (no leftover cards, per the ToJson note above)
	/// then delegates to <see cref="RestoreFromJson"/>, same "construct then overwrite" approach as
	/// Island.FromJson and Fear.FromJson. Standalone convenience only - a full-GameState restore must
	/// call <see cref="RestoreFromJson"/> on the GameState's own (already adversary-wired) InvaderDeck
	/// instead of swapping in one of these, since this constructs brand new Explore/Build/Ravage slots
	/// with default Engines, discarding whatever an adversary's Init wired up (e.g. Russia L6's
	/// Ravage.Engine) - see docs/GameSerialization-Roadmap.md's Adversary section. </remarks>
	public static InvaderDeck FromJson( JsonObject json ) {
		var unrevealedCards = ( (JsonArray)json["UnrevealedCards"]! ).Select( n => InvaderCardRegistry.Deserialize( (JsonArray)n! ) ).ToList();
		var deck = new InvaderDeck( unrevealedCards, [ new Queue<InvaderCard>(), new Queue<InvaderCard>(), new Queue<InvaderCard>() ] );
		deck.RestoreFromJson( json );
		return deck;
	}

	/// <summary> Restores onto this existing deck - preserves whatever Explore/Build/Ravage.Engine an
	/// adversary already wired up (see remarks on <see cref="FromJson"/>), only overwriting the
	/// card/count state each slot itself tracks. </summary>
	public void RestoreFromJson( JsonObject json ) {
		UnrevealedCards.SetItems( ( (JsonArray)json["UnrevealedCards"]! ).Select( n => InvaderCardRegistry.Deserialize( (JsonArray)n! ) ).ToArray() );
		Discards.SetItems( ( (JsonArray)json["Discards"]! ).Select( n => InvaderCardRegistry.Deserialize( (JsonArray)n! ) ).ToArray() );
		_drawCount.SetItems( ( (JsonArray)json["DrawCounts"]! ).Select( n => n!.GetValue<int>() ).ToArray() );

		Dictionary<string, InvaderSlot> byLabel = new InvaderSlot[] { Explore, Build, Ravage }.ToDictionary( s => s.Label );
		ActiveSlots = ( (JsonArray)json["ActiveSlots"]! ).Select( n => DeserializeActiveSlot( (JsonArray)n!, byLabel ) ).ToList();

		Explore.RestoreFromJson( (JsonArray)json["Explore"]! );
		Build.RestoreFromJson( (JsonArray)json["Build"]! );
		Ravage.RestoreFromJson( (JsonArray)json["Ravage"]! );
	}

	static InvaderSlot DeserializeActiveSlot( JsonArray entry, Dictionary<string, InvaderSlot> byLabel ) => entry[0]!.GetValue<string>() switch {
		"Fixed" => byLabel[entry[1]!.GetValue<string>()],
		"Extra" => InvaderSlotRegistry.Deserialize( (JsonArray)entry[1]! ),
		var kind => throw new NotSupportedException( $"Unknown ActiveSlot entry kind '{kind}'" )
	};

	#endregion Json

	#region Memento

	public virtual object Memento {
		get => new MyMemento( this );
		set => ((MyMemento)value).Restore( this );
	}

	protected class MyMemento {
		public MyMemento(InvaderDeck src) {
			// Explore/Build/Ravage
			_slots = [..src.ActiveSlots];
			_mementos.SaveMany( src._activeSlots );

			// Future / Past cards
			_unrevealedCards = [..src.UnrevealedCards];
			_discards = [..src.Discards];
			_drawCount = [..src._drawCount];
		}
		public void Restore(InvaderDeck src ) {
			src.UnrevealedCards.SetItems(_unrevealedCards);
			src._drawCount.SetItems(_drawCount);
			src.ActiveSlots.SetItems( _slots );
			src.Discards.SetItems( _discards );

			_mementos.Restore();
		}
		readonly Dictionary<IHaveMemento, object> _mementos = [];
		readonly InvaderSlot[] _slots;
		readonly InvaderCard[] _unrevealedCards;
		readonly InvaderCard[] _discards;
		readonly int[] _drawCount;

	}

	#endregion

}

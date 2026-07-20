using SpiritIsland.Log;

namespace SpiritIsland;

/// <remarks>
/// Not an engine because it contains games state.
/// So it is ok to hold a GameState instance.
/// </remarks>
public class Fear : IHaveMemento {

	public event Action<IFearCard>? CardActivated;

	public Fear(GameState gs) {
		_gs = gs;
		PoolMax = gs.Spirits.Length * 4;
		Init();
	}

	public void Init() {
		while(Deck.Count < 9)
			Deck.Push( new NullFearCard() );
	}

	public void PushOntoDeck( IFearCard fearCard ) {
		Deck.Push( fearCard );
	}

	public int[] CardsPerLevel_Initial = [3, 3, 3]; // only adjusted during Setup - doesn't need saved to memento

	public int TerrorLevel {
		get {
			int level3Count = CardsPerLevel_Initial[2];
			int level2Count = CardsPerLevel_Initial[1];
			int ct = Deck.Count;
			return (ct > level2Count + level3Count) ? 1
				: ct > level3Count ? 2
				: ct > 0 ? 3
				: 4; // Victory
		}
	}

	// This returns lowest Terror Level 1st
	// When some missing, does not return that Terror Level
	public int[] CardsPerLevel_Remaining {
		get {
			// int[] slots = [0,0,0];

			// int remaining = Deck.Count;
			// int index = 2;
			// while( 0 < remaining) {
			// 	slots[index] = Math.Min( CardsPerLevel[index], remaining );
			// 	remaining -= slots[index--];
			// }
			// return slots;

			int remaining = Deck.Count;
			int l3 = Math.Min( CardsPerLevel_Initial[2], remaining ); remaining -= l3;
			int l2 = Math.Min( CardsPerLevel_Initial[1], remaining ); remaining -= l2;
			int l1 = Math.Min( CardsPerLevel_Initial[0], remaining );
			return [l1,l2,l3];


		}
	}

	public async Task AddOnSpace(Space space, int count, FearType fearType) {
		if( count == 0 ) return;

		Add(count);

		var mods = space.Keys.OfType<IReactToLandFear>().ToArray();
		foreach( IReactToLandFear mod in mods )
			await mod.HandleFearAddedAsync(space, count, fearType);
	}


	/// <summary> Adds generated Fear to the fear pool </summary>
	public void Add( int count ) {
		if(count == 0) return;

		EarnedFear += count;
		while(PoolMax <= EarnedFear && Deck.Count != 0) {
			EarnedFear -= PoolMax;

			var topCard = Deck.Pop();
			ActivatedCards.Push(topCard);
			CardActivated?.Invoke(topCard);
		}
		// ! Do NOT check for victory here and throw GameOverException(...)
		// This is called inside PowerCard using Invoke() which converts exception to a TargetInvocationException which we don't want.
		// Let the post-Action check catch the victory.

		_gs.Log(new FearGenerated(count));
	}

	/// <summary>
	/// Flips and Acts() all 'ActivatedCards'
	/// </summary>
	public async Task ResolveActivatedCards() {
		while(0 < ActivatedCards.Count) {
			IFearCard fearCard = ActivatedCards.Pop();
			await using var actionScope = await ActionScope.Start(ActionCategory.Fear);
			await fearCard.ActAsync(TerrorLevel);
			++ResolvedCardCount; // record discard cards (for England-6)
		}
	}

	public int ResolvedCardCount { get; private set; }

	// - ints -
	public int EarnedFear { get; private set; } = 0;
	public int PoolMax { get; set; }

	// - cards -
	public readonly Stack<IFearCard> Deck = new Stack<IFearCard>();
	public readonly Stack<IFearCard> ActivatedCards = new Stack<IFearCard>();
	// - events -
	readonly GameState _gs;

	#region Json

	/// <summary>
	/// Named keys - same field list as MyMemento below. CardsPerLevel_Initial isn't here - its own doc
	/// comment says it's only adjusted during Setup and doesn't need saved.
	/// </summary>
	public JsonObject ToJson()
		=> new JsonObject {
			["EarnedFear"] = EarnedFear,
			["PoolMax"] = PoolMax,
			["ResolvedCardCount"] = ResolvedCardCount,
			["Deck"] = new JsonArray( Deck.Select( c => (JsonNode)c.ToJson() ).ToArray() ),
			["ActivatedCards"] = new JsonArray( ActivatedCards.Select( c => (JsonNode)c.ToJson() ).ToArray() )
		};

	/// <remarks> Needs a live GameState to construct through - Fear's own constructor requires one
	/// (PoolMax defaults from Spirits.Length, Init() pads the deck) - so this builds fresh via `new
	/// Fear(gs)` then overwrites every field from JSON, same as Island.FromJson going through Island's
	/// normal constructor rather than bypassing it. Standalone convenience only - GameState.Fear has no
	/// setter (an adversary can subscribe to the *existing* Fear's CardActivated event during Init replay,
	/// e.g. Russia L6 - see docs/GameSerialization-Roadmap.md's Adversary section), so a full-GameState
	/// restore must call <see cref="RestoreFromJson"/> on the GameState's own Fear instance instead of
	/// swapping in one of these. </remarks>
	public static Fear FromJson( JsonObject json, GameState gs ) {
		var fear = new Fear( gs );
		fear.RestoreFromJson( json );
		return fear;
	}

	/// <summary> Restores onto this existing Fear instance, preserving whatever event subscriptions
	/// (e.g. an adversary's CardActivated hook) it already carries - see the remarks on
	/// <see cref="FromJson"/>. </summary>
	public void RestoreFromJson( JsonObject json ) {
		EarnedFear = json["EarnedFear"]!.GetValue<int>();
		PoolMax = json["PoolMax"]!.GetValue<int>();
		ResolvedCardCount = json["ResolvedCardCount"]!.GetValue<int>();
		Deck.SetItems( ( (JsonArray)json["Deck"]! ).Select( n => FearCardRegistry.Deserialize( (JsonArray)n! ) ).ToArray() );
		ActivatedCards.SetItems( ( (JsonArray)json["ActivatedCards"]! ).Select( n => FearCardRegistry.Deserialize( (JsonArray)n! ) ).ToArray() );
	}

	#endregion Json

	#region Memento

	object IHaveMemento.Memento {
		get => new MyMemento( this );
		set => ((MyMemento)value).Restore( this );
	}

	protected class MyMemento {
		public MyMemento(Fear src) {
			_earnedFear = src.EarnedFear;
			_poolMax = src.PoolMax;
			_resolvedCardCount = src.ResolvedCardCount;
			_deck = [..src.Deck];
			_activatedCards = [..src.ActivatedCards];
			_flipped = _deck.Union( _activatedCards ).ToDictionary(c=>c,c=>c.Flipped);
		}
		public void Restore(Fear src ) {
			src.EarnedFear = _earnedFear;
			src.PoolMax = _poolMax;
			src.ResolvedCardCount = _resolvedCardCount;
			src.Deck.SetItems( _deck );
			src.ActivatedCards.SetItems(_activatedCards);
			foreach(var pair in _flipped)
				pair.Key.Flipped = pair.Value;
		}
		readonly int _earnedFear;
		readonly int _poolMax;
		readonly int _resolvedCardCount;
		readonly IFearCard[] _deck;
		readonly IFearCard[] _activatedCards;
		readonly Dictionary<IFearCard, bool> _flipped;
	}

	#endregion Memento

}


public enum FearType { Direct, FromInvaderDestruction }
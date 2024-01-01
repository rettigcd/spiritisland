namespace SpiritIsland;

/// <summary>
/// The Engine + card management
/// </summary>
public abstract class InvaderSlot {

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

	int _skipCount = 0;
	protected int _holdBackCount = 0;
}

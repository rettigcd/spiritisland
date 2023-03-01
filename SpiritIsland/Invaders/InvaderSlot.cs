namespace SpiritIsland;

/// <summary>
/// The Engine + card management
/// </summary>
public abstract class InvaderSlot {

	public InvaderSlot(string label ) { Label = label;}
	public string Label { get; }
	public List<InvaderCard> Cards { get; } = new List<InvaderCard>();
	public void HoldNextBack() { holdBackCount++; }
	public void SkipNextNormal() { skipCount++; }
	public virtual async Task Execute( GameState gs ) {
		foreach(var card in Cards)
			if(0 < skipCount)
				skipCount--;
			else
				await ActivateCard(card,gs);
	}

	public List<InvaderCard> GetCardsToAdvance() {
		var result = new List<InvaderCard>();
		for(int i=0; i < Cards.Count; ++i)
			if(0 < holdBackCount)
				--holdBackCount;
			else {
				result.Add(Cards[i]);
				Cards.RemoveAt(i--); // post-decrement is correct
			}
		return result;
	}

	public abstract Task ActivateCard( InvaderCard card, GameState gameState);

	int skipCount = 0;
	int holdBackCount = 0;
}

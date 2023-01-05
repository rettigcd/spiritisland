namespace SpiritIsland;

public abstract class InvaderSlot {
	public InvaderSlot(string label ) { Label = label;}
	public string Label { get; }
	public List<IInvaderCard> Cards { get; } = new List<IInvaderCard>();
	public void HoldNextBack() { holdBackCount++; }
	public void SkipNextNormal() { skipCount++; }
	public virtual async Task Execute( GameState gs ) {
		foreach(var card in Cards)
			if(skipCount > 0)
				skipCount--;
			else if(card.Skip)
				card.Skip = false; // !!!! not sure if Card.Skip is ever set to true.
			else
				await ActivateCard(card,gs);
	}

	public List<IInvaderCard> GetCardsToAdvance() {
		var result = new List<IInvaderCard>();
		for(int i=0; i < Cards.Count; ++i)
			if(holdBackCount > 0)
				holdBackCount--;
			else {
				result.Add(Cards[i]);
				Cards.RemoveAt(i--);
			}
		return result;
	}

	public abstract Task ActivateCard( IInvaderCard card, GameState gameState);

	int skipCount = 0;
	int holdBackCount = 0;
}

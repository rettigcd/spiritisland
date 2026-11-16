namespace SpiritIsland;

public abstract class StillHealthyBlightCard( string title, string description, int additinalBlightWhenFlipped ) 
	: BlightCard( title, description, additinalBlightWhenFlipped )
{
	protected override async Task Side2Depleted(  GameState gs ) {
		// If there is ever NO Blight here, draw a new Blight Card.
		gs.BlightCard = gs.BlightCards[0];
		gs.BlightCards.RemoveAt(0);
		// It comes into play already flipped. Its effect can wait on the player, so wait for it: the Invader action that
		// added the blight must not finish (and close its ActionScope) while this card's own scope is still open.
		await gs.BlightCard.OnBlightDepleated(gs);
		ActionScope.Current.Log( new Log.IslandBlighted( gs.BlightCard ) );
	}

}
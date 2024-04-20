namespace SpiritIsland;

public abstract class StillHealthyBlightCard( string title, string description, int additinalBlightWhenFlipped ) 
	: BlightCard( title, description, additinalBlightWhenFlipped )
{
	protected override void Side2Depleted(  GameState gs ) {
		// If there is ever NO Blight here, draw a new Blight Card.
		gs.BlightCard = gs.BlightCards[0];
		gs.BlightCards.RemoveAt(0);
		// It comes into play already flipped
		_ = gs.BlightCard.OnBlightDepleated(gs); // !!!! this looks like a bug
		ActionScope.Current.Log( new Log.IslandBlighted( gs.BlightCard ) );
	}

}
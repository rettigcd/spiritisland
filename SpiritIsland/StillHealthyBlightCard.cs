namespace SpiritIsland;

public class StillHealthyBlightCard : BlightCardBase {

	public StillHealthyBlightCard(string title, int additinalBlightWhenFlipped)
		:base( title, additinalBlightWhenFlipped ) { }

	public override ActionOption<GameState> Immediately => new ActionOption<GameState>("Still Healthy", (_)=>{ } );

	protected override void Side2Depleted(  GameState gs ) {
		// If there is ever NO Blight here, draw a new Blight Card.
		gs.BlightCard = gs.BlightCards[0];
		gs.BlightCards.RemoveAt(0);
		// It comes into play already flipped
		gs.BlightCard.OnBlightDepleated(gs);
	}

}
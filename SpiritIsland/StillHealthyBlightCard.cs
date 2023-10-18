namespace SpiritIsland;

public class StillHealthyBlightCard : BlightCard {

	public StillHealthyBlightCard(string title, string description, int additinalBlightWhenFlipped)
		:base( title, description, additinalBlightWhenFlipped ) { }

	public override BaseCmd<GameCtx> Immediately => new BaseCmd<GameCtx>("Still Healthy", (_)=>{ } );

	protected override void Side2Depleted(  GameState gs ) {
		// If there is ever NO Blight here, draw a new Blight Card.
		gs.BlightCard = gs.BlightCards[0];
		gs.BlightCards.RemoveAt(0);
		// It comes into play already flipped
		gs.BlightCard.OnBlightDepleated(gs);
		gs.Log( new Log.IslandBlighted( gs.BlightCard ) );
	}

}
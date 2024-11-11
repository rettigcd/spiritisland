namespace SpiritIsland;

public class RavageSlot : InvaderSlot {
	public RavageSlot() : base( "Ravage" ) { }
	public override Task ActivateCard( InvaderCard card, GameState _ ) => Engine.ActivateCard( card );
	public RavageEngine Engine = new RavageEngine();

	public override List<InvaderCard> GetCardsToAdvance() {
		var result = new List<InvaderCard>();
		for(int i = 0; i < Cards.Count; ++i) {
			if(0 < _holdBackCount) { --_holdBackCount; continue; }
		
			// This is a hack because .Ravage slot is not settable. Habsburg Mining Colony
			// !! If Invader Cards had a property on them like .ShouldAdvance, then we could override that property.
			if(Cards[i].Code.Contains("Salt")) continue;
				
			result.Add( Cards[i] );
			Cards.RemoveAt( i-- ); // post-decrement is correct
		}
		return result;
	}


}

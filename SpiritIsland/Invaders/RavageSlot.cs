namespace SpiritIsland;

// ??? Is this the Visitor Pattern ???
public class RavageSlot : InvaderSlot {
	public RavageSlot() : base( "Ravage" ) { }
	public override Task ActivateCard( InvaderCard card, GameState gameState ) => Engine.ActivateCard( card, gameState );
	public RavageEngine Engine = new RavageEngine();
}

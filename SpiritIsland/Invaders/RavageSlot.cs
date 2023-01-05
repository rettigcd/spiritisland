namespace SpiritIsland;

// ??? Is this the Visitor Pattern ???
// !!! We could pull all of the Ravage behavior and Build behavior out of the Invader Cards and put them in the Slot
public class RavageSlot : InvaderSlot {
	public RavageSlot() : base( "Ravage" ) { }
	public override Task ActivateCard( IInvaderCard card, GameState gameState ) => card.Ravage( gameState );
}

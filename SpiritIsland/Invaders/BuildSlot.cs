namespace SpiritIsland;

/// <remarks>
/// Purpose:
///		Makes calling each of the InvaderSlots uniform via ActivateCard(card)
///		Allows the Build Engine to be pluggable
///		Holds cards.
/// </remarks>
/// <param name="label"></param>
public class BuildSlot( string label = "Build" ) : InvaderSlot( label ) {

	public override async Task ActivateCard( InvaderCard card, GameState _ ) {
		await Engine.ActivateCard( card );
	}

	public BuildEngine Engine { get; set; } = new BuildEngine();

}

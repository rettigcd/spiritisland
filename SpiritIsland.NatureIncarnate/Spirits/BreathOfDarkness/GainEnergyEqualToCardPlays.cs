namespace SpiritIsland.NatureIncarnate;

public class GainEnergyEqualToCardPlays : SpiritAction {

	public GainEnergyEqualToCardPlays():base( "GainEnergyEqualToCardPlays" ) { }
	public override Task ActAsync( SelfCtx ctx ) {
		ctx.Self.Energy += ctx.Self.NumberOfCardsPlayablePerTurn;
		return Task.CompletedTask;
	}
}
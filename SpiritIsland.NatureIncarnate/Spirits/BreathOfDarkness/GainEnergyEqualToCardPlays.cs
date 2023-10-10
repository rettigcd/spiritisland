namespace SpiritIsland.NatureIncarnate;

public class GainEnergyEqualToCardPlays : GrowthActionFactory {
	public override Task ActivateAsync( SelfCtx ctx ) {
		ctx.Self.Energy += ctx.Self.NumberOfCardsPlayablePerTurn;
		return Task.CompletedTask;
	}
}
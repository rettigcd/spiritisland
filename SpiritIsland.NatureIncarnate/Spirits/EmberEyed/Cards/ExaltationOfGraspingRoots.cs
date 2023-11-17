namespace SpiritIsland.NatureIncarnate;

public class ExaltationOfGraspingRoots {

	public const string Name = "Exaltation of Grasping Roots";

	[SpiritCard(Name, 0, Element.Moon,Element.Fire,Element.Earth,Element.Plant)]
	[Slow, AnotherSpirit]
	[Instructions( "Target Spirit may Add 1 Wilds in one of their lands. You may do likewise." ), Artist( Artists.DavidMarkiwsky )]
	static public async Task ActionAsync(TargetSpiritCtx ctx) {
		// Target Spirit may Add 1 Wilds in one of their lands.
		await AddWildsIn1OfYourLands.ActAsync( ctx.OtherCtx );

		// You may do likewise.
		await AddWildsIn1OfYourLands.ActAsync( ctx );
	}

	static IActOn<SelfCtx> AddWildsIn1OfYourLands => Cmd
		.AddWilds( 1 )
		.To().SpiritPickedLand()
		.Which( Has.YourPresence );
}
namespace SpiritIsland.Basegame;

public class RiversBounty {

	public const string Name = "River's Bounty";

	[SpiritCard(RiversBounty.Name, 0, Element.Sun,Element.Water,Element.Animal)]
	[Slow, FromPresence(0)]
	[Instructions( "Gather up to 2 Dahan. If there are now at least 2 Dahan, add 1 Dahan and gain 1 Energy." ), Artist( Artists.NolanNasser )]
	static public async Task ActionAsync(TargetSpaceCtx ctx) {

		// Gather up to 2 Dahan
		await ctx.GatherUpToNDahan( 2 );

		// If there are now at least 2 dahan, then add 1 dahan and gain 1 energy
		if(2 <= ctx.Dahan.CountAll) {
			await ctx.Dahan.Add( 1 );
			++ctx.Self.Energy;
		}
	}

}
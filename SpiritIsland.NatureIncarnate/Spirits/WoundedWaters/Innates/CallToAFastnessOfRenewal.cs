namespace SpiritIsland.NatureIncarnate;

[InnatePower( CallToAFastnessOfRenewal.Name )]
[Fast, FromPresence( 1 )]
public class CallToAFastnessOfRenewal {

	public const string Name = "Call to a Fastness of Renwal";

	[InnateTier( "1 water", "Gather up to 2 Dahan.", 0 )]
	static public Task Option1( TargetSpaceCtx ctx ) => ctx.GatherUpToNDahan(2);

	[InnateTier( "2 water,1 plant", "Defend 3 or Downgrade 1 Invader.", 1 )]
	static public Task Option2( TargetSpaceCtx ctx ) {
		return Cmd.Pick1(
			new SpaceCmd("Defend 3", x=>x.Defend(3)),
			new SpaceCmd("Downgrade 1 Invader", ctx=>ReplaceInvader.Downgrade1(ctx,Present.Always,Human.Invader) )
		).ActAsync(ctx);
	}

	[InnateTier( "3 water,1 plant", "Add 1 Beast.", 2 )]
	static public Task Option3( TargetSpaceCtx ctx ) => ctx.Beasts.AddAsync(1);

	[InnateTier( "1 sun,4 water,2 plant", "If at least 2 Dahan are present, Replace 1 Invader with 1 Dahan.", 3 )]
	static public async Task Option4( TargetSpaceCtx ctx ){
		if(2 <= ctx.Dahan.CountAll && ctx.HasInvaders) {
			await ctx.Invaders.RemoveLeastDesirable(RemoveReason.Replaced,Human.Invader);
			await ctx.Dahan.AddDefault( 1, AddReason.AsReplacement );
		}
	}


}

namespace SpiritIsland.NatureIncarnate;

[InnatePower( Name ), Slow, FromSacredSite( 0, Filter.Invaders+"+"+Filter.Incarna )]
public class RevokeSanctuaryAndCastOut {

	public const string Name = "Revoke Sanctuary and Cast Out";

	[InnateTier( "1 sun,1 moon,2 plant", "1 Fear. Remove 1 Explorer/Town.", 0 )]
	static public async Task Option1( TargetSpaceCtx ctx ) {
		await ctx.AddFear(1);
		await Cmd.RemoveNTokens( 1, Human.Explorer_Town ).ActAsync( ctx );
	}

	[InnateTier( "2 sun,1 moon,3 plant", "1 Fear. Remove 1 Explorer/Town.", 1 )]
	static public async Task Option2( TargetSpaceCtx ctx ) {
		await ctx.AddFear(1);
		await Cmd.RemoveNTokens( 1, Human.Explorer_Town ).ActAsync( ctx );
	}

	[InnateTier( "2 sun,2 moon,4 plant", "1 Fear. Remove 1 Invader.", 2 )]
	static public async Task Option3( TargetSpaceCtx ctx ) {
		await ctx.AddFear(1);
		await Cmd.RemoveNTokens( 1, Human.Invader ).ActAsync( ctx );
	}

}
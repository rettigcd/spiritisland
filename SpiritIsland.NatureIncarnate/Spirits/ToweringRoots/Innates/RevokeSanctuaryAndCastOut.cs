namespace SpiritIsland.NatureIncarnate;

[InnatePower( "Revoke Sanctuary and Cast Out" ), Slow, FromSacredSite( 0, Target.Invaders+"+"+Target.Incarna )]
public class RevokeSanctuaryAndCastOut {

	[InnateTier( "1 sun,1 moon,2 plant", "1 Fear. Remove 1 Explorer/Town.", 0 )]
	static public async Task Option1( TargetSpaceCtx ctx ) {
		ctx.AddFear(1);
		await Cmd.RemoveNTokens( 1, Human.Explorer_Town ).ActAsync( ctx );
	}

	[InnateTier( "2 sun,1 moon,3 plant", "1 Fear. Remove 1 Explorer/Town.", 1 )]
	static public async Task Option2( TargetSpaceCtx ctx ) {
		ctx.AddFear( 1 );
		await Cmd.RemoveNTokens( 1, Human.Explorer_Town ).ActAsync( ctx );
	}

	[InnateTier( "2 sun,2 moon,4 plant", "1 Fear. Remove 1 Invader.", 2 )]
	static public async Task Option3( TargetSpaceCtx ctx ) {
		ctx.AddFear( 1 );
		await Cmd.RemoveNTokens( 1, Human.Invader ).ActAsync( ctx );
	}

}
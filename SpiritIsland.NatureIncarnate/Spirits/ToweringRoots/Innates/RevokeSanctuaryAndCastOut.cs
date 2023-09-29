namespace SpiritIsland.NatureIncarnate;

[InnatePower( "Revoke Sanctuary and Cast Out" ), Fast, FromPresence( 1, Target.Invaders )]
public class RevokeSanctuaryAndCastOut {

	[InnateOption( "2 earth", "Defend 2" )]
	static public Task Option1( TargetSpaceCtx ctx ) {
		ctx.Defend( 2 );
		return Task.CompletedTask;
	}

	[InnateOption( "1 air,2 earth", "Instead, Defend 3" )]
	static public Task Option2( TargetSpaceCtx ctx ) {
		ctx.Defend( 3 );
		return Task.CompletedTask;
	}

	[InnateOption( "2 moon,3 air,4 earth", "Instead, Defend 2 per card in the Invader discard pile." )]
	static public Task Option3( TargetSpaceCtx ctx ) {
		ctx.Defend( ctx.GameState.InvaderDeck.Discards.Count * 2 );
		return Task.CompletedTask;
	}

}
namespace SpiritIsland.NatureIncarnate;

[InnatePower( "Shelter Under Towering Branches" ), Slow, FromSacredSite( 0 )]
public class ShelterUnderToweringBranches {

	[InnateOption( "1 sun, 1 plant", "Gather up to 1 Dahan" )]
	static public async Task Option1( TargetSpaceCtx ctx ) {
		await Cmd.GatherUpToNDahan(1).Execute(ctx);
	}

	[InnateOption( "1 sun,1 earth,2 plant", "Gather up to 1 Explorer" )]
	static public async Task Option2( TargetSpaceCtx ctx ) {
		await Cmd.GatherUpToNInvaders(1,Human.Explorer).Execute( ctx );
	}

	[InnateOption( "2 sun,1 earth,3 plant", "Gather up to 1 Town" )]
	static public async Task Option3( TargetSpaceCtx ctx ) {
		await Cmd.GatherUpToNInvaders( 1, Human.Town ).Execute( ctx );
	}

	[InnateOption( "3 sun,2 earth,4 plant", "Gather up to 1 City" )]
	static public async Task Option4( TargetSpaceCtx ctx ) {
		await Cmd.GatherUpToNInvaders( 1, Human.City ).Execute( ctx );
	}


}

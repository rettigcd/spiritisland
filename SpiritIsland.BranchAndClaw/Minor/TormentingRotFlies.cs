namespace SpiritIsland.BranchAndClaw;

public class TormentingRotFlies {

	[MinorCard( "Tormenting Rotflies", 1, Element.Air, Element.Plant, Element.Animal )]
	[Slow]
	[FromPresence( 2, Target.SandOrWetland )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceAction( "Add 1 disease", ctx => ctx.Disease.Add(1) ),
			new SpaceAction( "2 fear, +1(if disease) +1(if blight)", AddFear ).Matches( x => x.Tokens.HasInvaders() )
		);

	}

	static public void AddFear( TargetSpaceCtx ctx ) {
		int fearCount = 2;
		if( ctx.Disease.Any ) fearCount++;
		if( ctx.Blight.Count>0 ) fearCount++;
		ctx.AddFear( fearCount );
	}
}
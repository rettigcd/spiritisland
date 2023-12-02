namespace SpiritIsland.BranchAndClaw;

public class TormentingRotFlies {

	[MinorCard( "Tormenting Rotflies", 1, Element.Air, Element.Plant, Element.Animal ),Slow,FromPresence( 2, Filter.Sands, Filter.Wetland )]
	[Instructions( "Add 1 Disease. -or- If target land has Invaders, 2 Fear. If Disease is present, +1 Fear. If Blight is present, +1 Fear." ), Artist( Artists.KatBirmelin )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceAction( "Add 1 disease", ctx => ctx.Disease.AddAsync(1) ),
			new SpaceAction( "2 fear, +1(if disease) +1(if blight)", AddFear ).OnlyExecuteIf( x => x.Tokens.HasInvaders() )
		);

	}

	static public void AddFear( TargetSpaceCtx ctx ) {
		int fearCount = 2;
		if( ctx.Disease.Any ) fearCount++;
		if( ctx.Blight.Count>0 ) fearCount++;
		ctx.AddFear( fearCount );
	}
}
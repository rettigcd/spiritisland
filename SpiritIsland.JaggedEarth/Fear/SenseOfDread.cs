namespace SpiritIsland.JaggedEarth;

public class SenseOfDread : FearCardBase, IFearCard {

	public const string Name = "Sense of Dread";
	public string Text => Name;

	[FearLevel(1, "On Each Board: Remove 1 Explorer from a land matching a Ravage card." )]
	public Task Level1( GameCtx ctx ) {
		return Cmd.RemoveExplorers( 1 ).From().OneLandPerBoard().Which( MatchesRavageCard )
			.ForEachBoard()
			.Execute( ctx );
	}

	[FearLevel(2, "On Each Board: Remove 1 Explorer/Town from a land matching a Ravage card." )]
	public Task Level2( GameCtx ctx ) { 
		return Cmd.RemoveExplorersOrTowns( 1 ).From().OneLandPerBoard().Which( MatchesRavageCard )
			.ForEachBoard()
			.Execute( ctx );
	}

	[FearLevel(3, "On Each Board: Remove 1 Invader from a land matching a Ravage card." )]
	public Task Level3( GameCtx ctx ) {
		return Cmd.RemoveInvaders( 1 ).From().OneLandPerBoard().Which( MatchesRavageCard )
			.ForEachBoard()
			.Execute( ctx );
	}

	static TargetSpaceCtxFilter MatchesRavageCard => new TargetSpaceCtxFilter( "matching a Ravage card", MatchingRavageCardImp );

	static bool MatchingRavageCardImp( TargetSpaceCtx ctx )
		=> ctx.GameState.InvaderDeck.Ravage.Cards
			.Any( card => card.MatchesCard(ctx.Tokens) );

}
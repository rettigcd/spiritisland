﻿namespace SpiritIsland.JaggedEarth;

public class SenseOfDread : IFearOptions {

	public const string Name = "Sense of Dread";
	string IFearOptions.Name => Name;

	[FearLevel(1, "On Each Board: Remove 1 Explorer from a land matching a Ravage card." )]
	public Task Level1( FearCtx ctx ) {
		// On Each Board
		return Cmd.OnEachBoard(
			// Remove 1 Explorer
			Cmd.RemoveExplorers(1)
				// from a land matching a Ravage card.
				.FromLandOnBoard( MatchingRavageCard, "a land matching a Ravage card" )
		).Execute( ctx.GameState );
	}

	[FearLevel(2, "On Each Board: Remove 1 Explorer / Town from a land matching a Ravage card." )]
	public Task Level2( FearCtx ctx ) { 
		// On Each Board
		return Cmd.OnEachBoard(
			// Remove 1 Explorer/Town
			Cmd.RemoveExplorersOrTowns(1)
				// from a land matching a Ravage card.
				.FromLandOnBoard( MatchingRavageCard,"a land matching a Ravage card" )
		).Execute( ctx.GameState );
	}

	[FearLevel(3, "On Each Board: Remove 1 Invader from a land matching a Ravage card." )]
	public Task Level3( FearCtx ctx ) {
		// On Each Board
		return Cmd.OnEachBoard(
			// Remove 1 Invader
			Cmd.RemoveInvaders(1)
				// from a land matching a Ravage card.
				.FromLandOnBoard( MatchingRavageCard,"a land matching a Ravage card" )
		).Execute( ctx.GameState );
	}

	static bool MatchingRavageCard( TargetSpaceCtx ctx )
		=> ctx.GameState.InvaderDeck.Ravage
			.Any( card => card.Matches(ctx.Tokens.Space) );

}
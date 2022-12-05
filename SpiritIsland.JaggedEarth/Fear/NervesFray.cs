namespace SpiritIsland.JaggedEarth;

public class NervesFray : IFearOptions {

	public const string Name = "Nerves Fray";
	string IFearOptions.Name => Name;


	[FearLevel(1, "Each player adds 1 Strife in a land not matching a Ravage Card." )]
	public Task Level1( GameCtx ctx ) {
		return EachPlayerAddsStrifeToNonRavageLand( 1 ).Execute( ctx );
	}

	[FearLevel(2, "Each player adds 2 Strife in a single land not matching a Ravage Card." )]
	public Task Level2( GameCtx ctx ) { 
		return EachPlayerAddsStrifeToNonRavageLand( 2 ).Execute( ctx );
	}

	[FearLevel(3, "Each player adds 2 Strife in a single land not matching a Ravage Card. 1 Fear per player." )]
	public async Task Level3( GameCtx ctx ) { 

		await EachPlayerAddsStrifeToNonRavageLand( 2 ).Execute(ctx);

		// 1 Fear per player.
		ctx.GameState.Fear.AddDirect(new FearArgs { count = ctx.GameState.Spirits.Length });
	}

	static public DecisionOption<GameCtx> EachPlayerAddsStrifeToNonRavageLand( int strifeCount )
		=> Cmd.EachSpirit(
			Cmd.AddStrife(strifeCount).To( DoesNotMatchRavageCard, "land that does not match Ravage card" )
		);

	static bool DoesNotMatchRavageCard( TargetSpaceCtx spaceCtx )
		=> !spaceCtx.GameState.InvaderDeck.Ravage.Cards.Any( card => card.Matches(spaceCtx.Space) );

}
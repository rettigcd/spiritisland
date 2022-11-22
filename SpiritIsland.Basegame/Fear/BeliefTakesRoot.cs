namespace SpiritIsland.Basegame;

public class BeliefTakesRoot : IFearOptions {

	public const string Name = "Belief takes Root";
	string IFearOptions.Name => Name;


	[FearLevel( 1, "Defend 2 in all lands with Presence." )]
	public Task Level1( FearCtx ctx ) {
		Defend2WherePresence( ctx );
		return Task.CompletedTask;
	}

	static void Defend2WherePresence( FearCtx ctx ) {
		GameState gs = ctx.GameState;
		foreach(var space in gs.Spirits.SelectMany( s => s.Presence.Spaces(ctx.GameState) ).Distinct())
			gs.Tokens[space].Defend.Add(2);
	}

	[FearLevel( 2, "Defend 2 in all lands with Presence. Each Spirit gains 1 Energy per SacredSite they have in lands with Invaders." )]
	public Task Level2( FearCtx ctx ) {
		Defend2WherePresence( ctx );
		foreach(var spirit in ctx.Spirits)
			spirit.Self.Energy += spirit.Self.Presence.SacredSites( ctx.GameState, ctx.GameState.Island.Terrain_ForFear ).Count( s => spirit.Target(s).HasInvaders );
		return Task.CompletedTask;
	}

	[FearLevel( 3, "Each player chooses a different land and removes up to 2 Health worth of Invaders per Presence there." )]
	public Task Level3( FearCtx ctx ) {
		return Cmd.EachSpirit(
			Cmd.PickDifferentLandThenTakeAction(
				"Remove up to 2 Health worth of Invaders per presence.",
				Cmd.RemoveHealthOfInvaders("Remove 2 Health worth of invaders per Presence there.", ctx => 2 * ctx.Presence.Count )
			)
		).Execute(ctx.GameState);

	}

}
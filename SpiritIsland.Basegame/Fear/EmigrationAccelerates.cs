namespace SpiritIsland.Basegame;

public class EmigrationAccelerates : IFearOptions {

	public const string Name = "Emigration Accelerates";
	string IFearOptions.Name => Name;

	[FearLevel( 1, "Each player removes 1 Explorer from a Coastal land." )]
	public Task Level1( FearCtx ctx ) {
		var gs = ctx.GameState;
		return Cmd.EachSpirit(
			Cmd.RemoveExplorers(1)
				.From(x=>x.IsCoastal,"a Coastal land")
		).Execute(gs);
	}

	[FearLevel( 2, "Each player removes 1 Explorer / Town from a Coastal land." )]
	public Task Level2( FearCtx ctx ) {
		var gs = ctx.GameState;
		return Cmd.EachSpirit(
			Cmd.RemoveExplorersOrTowns( 1 )
				.From( x => x.IsCoastal, "a Coastal land" )
		).Execute( gs );
	}

	[FearLevel( 3, "Each player removes 1 Explorer / Town from any land." )]
	public Task Level3( FearCtx ctx ) {
		var gs = ctx.GameState;
		return Cmd.EachSpirit(
			Cmd.RemoveExplorersOrTowns( 1 )
			.InAnyLand()
		).Execute( gs );
	}

}
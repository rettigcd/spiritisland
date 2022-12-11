namespace SpiritIsland.Basegame;

public class EmigrationAccelerates : IFearCard {

	public const string Name = "Emigration Accelerates";
	public string Text => Name;
	public int? Activation { get; set; }
	public bool Flipped { get; set; }

	[FearLevel( 1, "Each player removes 1 Explorer from a Coastal land." )]
	public Task Level1( GameCtx ctx ) {
		return Cmd.EachSpirit(
			Cmd.RemoveExplorers(1)
				.From(x=>x.IsCoastal,"a Coastal land")
		).Execute(ctx);
	}

	[FearLevel( 2, "Each player removes 1 Explorer / Town from a Coastal land." )]
	public Task Level2( GameCtx ctx ) {
		return Cmd.EachSpirit(
			Cmd.RemoveExplorersOrTowns( 1 )
				.From( x => x.IsCoastal, "a Coastal land" )
		).Execute( ctx );
	}

	[FearLevel( 3, "Each player removes 1 Explorer / Town from any land." )]
	public Task Level3( GameCtx ctx ) {
		return Cmd.EachSpirit(
			Cmd.RemoveExplorersOrTowns( 1 )
			.InAnyLand()
		).Execute( ctx );
	}

}
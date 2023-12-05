namespace SpiritIsland;

public class NullFearCard : FearCardBase, IFearCard {
	
	public const string Name = "Null Fear Card";
	public string Text => Name;

	[FearLevel(1,"x")]
	public Task Level1( GameState gs ) { return Task.CompletedTask; }
	[FearLevel( 2, "x" )]
	public Task Level2( GameState gs ) { return Task.CompletedTask; }
	[FearLevel( 3, "x" )]
	public Task Level3( GameState gs ) { return Task.CompletedTask; }
}
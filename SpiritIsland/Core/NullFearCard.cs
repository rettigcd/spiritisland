namespace SpiritIsland;

public class NullFearCard : IFearOptions {
	
	public const string Name = "Null Fear Card";
	string IFearOptions.Name => Name;

	[FearLevel(1,"x")]
	public Task Level1( FearCtx gs ) { return Task.CompletedTask; }
	[FearLevel( 2, "x" )]
	public Task Level2( FearCtx gs ) { return Task.CompletedTask; }
	[FearLevel( 3, "x" )]
	public Task Level3( FearCtx gs ) { return Task.CompletedTask; }
}
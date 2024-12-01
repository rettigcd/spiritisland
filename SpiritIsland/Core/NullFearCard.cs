namespace SpiritIsland;

public class NullFearCard : FearCardBase, IFearCard {

	string IOption.Text => Name;
	public const string Name = "Null Fear Card";

	[FearLevel("x")]
	public override Task Level1( GameState gs ) { return Task.CompletedTask; }
	[FearLevel("x")]
	public override Task Level2( GameState gs ) { return Task.CompletedTask; }
	[FearLevel("x")]
	public override Task Level3( GameState gs ) { return Task.CompletedTask; }
}
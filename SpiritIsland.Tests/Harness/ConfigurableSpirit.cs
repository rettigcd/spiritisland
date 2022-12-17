namespace SpiritIsland.Tests;

public class ConfigurableSpirit : Spirit {
	public ConfigurableSpirit(SpiritPresence presence):base( presence ){}
	public override string Text => "Configurable Spirit";

	public override SpecialRule[] SpecialRules => throw new NotImplementedException();

	protected override void InitializeInternal( Board board, GameState gameState ) { }
}

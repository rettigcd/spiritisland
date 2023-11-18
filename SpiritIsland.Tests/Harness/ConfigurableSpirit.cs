namespace SpiritIsland.Tests;

public class ConfigurableSpirit : Spirit {
	public ConfigurableSpirit(Func<Spirit,SpiritPresence> init):base( init ){}
	public override string Text => "Configurable Spirit";

	public override SpecialRule[] SpecialRules => throw new NotImplementedException();

	protected override void InitializeInternal( Board board, GameState gameState ) { }
}

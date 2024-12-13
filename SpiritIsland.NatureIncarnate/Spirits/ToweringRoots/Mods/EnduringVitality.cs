namespace SpiritIsland.NatureIncarnate;

/// <remarks>Growth</remarks>
public class EnduringVitality : SpiritAction {

	static public SpecialRule Rule => new SpecialRule(
		"Enduring Vitality", 
		"Some of your Actions Add Vitality Tokens."
	);

	public EnduringVitality():base( "Add Vitality to Incarna" ) { }
	public override async Task ActAsync( Spirit self ) {
		if(self is ToweringRootsOfTheJungle roots && roots.Incarna.IsPlaced)
			await roots.Incarna.Space.AddAsync(Token.Vitality,1);
	}
}

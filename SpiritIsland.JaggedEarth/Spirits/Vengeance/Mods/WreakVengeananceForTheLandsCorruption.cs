namespace SpiritIsland.JaggedEarth;

public class WreakVengeananceForTheLandsCorruption : ISpiritMod, IConfigureMyActions {
	public const string Name = "Wreak Vengeance for the Land's Corruption";
	const string Description = "Your actions treat blight on the island as also being badlands";
	static public SpecialRule Rule => new SpecialRule( Name, Description );

	void IConfigureMyActions.Configure(Spirit spirit, ActionScope scope) {
		Token.Blight.BonusTag = Token.Badlands;
		scope.AtEndOfThisAction(action => Token.Blight.BonusTag = null);
	}
}
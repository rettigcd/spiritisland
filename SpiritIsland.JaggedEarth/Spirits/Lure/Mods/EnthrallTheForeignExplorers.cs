namespace SpiritIsland.JaggedEarth;

public class EnthrallTheForeignExplorers( Spirit self )
	: SpiritPresenceToken(self)
	, IConfigRavages
{

	public const string Name = "Enthrall the Foreign Explorers";
	const string Description = "For each of your presence in a land, ignore up to 2 explorer during the Ravage Step and any Ravage Action.";
	static public readonly SpecialRule Rule = new SpecialRule( Name, Description );

	async Task IConfigRavages.Config( Space space ) {
		await space.SourceSelector
			.UseQuota(new Quota().AddGroup( Self.Presence.CountOn( space ) * 2, Human.Explorer ))
			.SelectFightersAndSitThemOut( Self );
	}

	// No override/registration of its own needed - no extra state beyond Self, so
	// SpiritPresenceToken's shared base ToJson/reader (resolving via the spirit's own
	// Presence.Token) already covers this.

}

namespace SpiritIsland.JaggedEarth;

public class LingeringPestilencePresenceToken( Spirit spirit ) : SpiritPresenceToken( spirit ) {

	static public SpecialRule Rule => new SpecialRule(
		"Lingering Pestilence",
		"When your presence is destroyed by anything except a Spirit action, add 1 disease where each destroyed presence was."
	);

	protected override async Task OnPresenceDestroyed( ITokenRemovedArgs args ){ 
		await base.OnPresenceDestroyed( args );
		if( ActionScope.Current.Category != ActionCategory.Spirit_Power )
			await ((Space)args.From).Tokens.Disease.AddAsync( 1 );
	}
}
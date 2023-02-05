namespace SpiritIsland.JaggedEarth;

public class LingeringPestilenceToken : SpiritPresenceToken {

	static public SpecialRule Rule => new SpecialRule(
		"Lingering Pestilence",
		"When your presence is destroyed by anything except a Spirit action, add 1 disease where each destroyed presence was."
	);

	protected override async Task OnPresenceDestroyed( ITokenRemovedArgs args ){ 
		await base.OnPresenceDestroyed( args );
		if( args.ActionScope.Category != ActionCategory.Spirit_Power )
			await args.RemovedFrom.Disease.Bind( args.ActionScope ).Add( 1 );
	}
}
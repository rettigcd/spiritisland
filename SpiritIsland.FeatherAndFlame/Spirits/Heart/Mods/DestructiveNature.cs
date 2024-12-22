namespace SpiritIsland.FeatherAndFlame;

public class DestructiveNature(Spirit spirit) : SpiritPresenceToken(spirit)
	, IModifyRemovingToken
	, IHandleTokenAdded {

	static public readonly SpecialRule Rule = new SpecialRule(
		"Destructive Nature",
		"Blight added due to Spirit Effects (Powers, Special Rules, Scenario-based Rituals, etc) does not destroy your presence. (including cascades)"
	);

	public virtual Task ModifyRemovingAsync(RemovingTokenArgs args) {
		// Blight added due to Spirit effects( Powers, Special Rules, Scenario-based Rituals, etc) does not destroy your Presence. ( This includes cascades.)
		var addedBlight = BlightToken.ScopeConfig.BlightFromCardTrigger;
		if( DestroysMyPresence(args)
			&& (addedBlight is null || !addedBlight.Reason.IsOneOf(AddReason.Ravage, AddReason.BlightedIsland, AddReason.None))
		) {
			ActionScope.Current.Log(new Log.Debug($"Blight added due do Spirit effects does not destroy Wildfire presence."));
			args.Count = 0;
		}
		return Task.CompletedTask;
	}

	public async Task HandleTokenAddedAsync(Space to, ITokenAddedArgs args) {
		if( args.Added != this ) return;
		// !! There is a bug here somehow that after placing the 2nd fire, track, still returned only 1 
		// !! maybe we need to make Elements smarter so it is easier to calculate, like breaking it into:
		//	(track elements, prepared elements, card elements)
		int fireCount = Self.Presence.TrackElements[Element.Fire];
		var ctx = Self.Target(to.SpaceSpec);
		// For each fire showing, do 1 damage
		await ctx.DamageInvaders(fireCount);
		// if 2 fire or more are showing, add 1 blight
		if( 2 <= fireCount )
			await ctx.AddBlight(1, AddReason.SpecialRule);
	}
}

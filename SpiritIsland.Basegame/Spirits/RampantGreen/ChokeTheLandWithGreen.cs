namespace SpiritIsland.Basegame;

/// <summary>
/// Token used by Rampant Green that stops builds and ravages
/// </summary>
public class ChokeTheLandWithGreen : SpiritPresenceToken , ISkipBuilds, ISkipRavages  {

	static public SpecialRule Rule => new SpecialRule(
		"Choke the land with green",
		"Whenever invaders would ravage or build in a land with your sacred site, you may prevent it by destroying one of your presense in that land."
	);

	readonly ASpreadOfRampantGreen _self;

	public ChokeTheLandWithGreen( ASpreadOfRampantGreen self ) {
		_self = self;
	}

	public UsageCost Cost => UsageCost.Extreme; // we lose presence!

	bool IsSacredSite( SpaceState space ) => 2 <= space[this]; // !! could make public and promote to base class and have SpiritPresence use it.

	async Task<bool> ISkipRavages.Skip( GameState gameState, SpaceState space ) {
		return await SkipInvaderAction( gameState, space,"ravage");
	}

	async Task<bool> ISkipBuilds.Skip( GameCtx gameCtx, SpaceState space, TokenClass buildClass )
		=> await SkipInvaderAction( gameCtx.GameState, space, $"build of {buildClass.Label}" );

	async Task<bool> SkipInvaderAction( GameState gs, SpaceState space, string actionDescription ) {
		if(!IsSacredSite( space )) return false;

		int energyCost = gs.BlightCard.CardFlipped ? 1 : 0;
		if(_self.Energy < energyCost) return false;

		var stop = await _self.Gateway.Decision( new Select.Space( $"Stop {actionDescription} on {space.Space.Text} by destroying 1 presence", new Space[] { space.Space }, Present.Done ) );
		if(stop == null) return false;

		var unitOfWork = gs.StartAction( ActionCategory.Spirit ); // Special Rules!
		await _self.Presence.Destroy( stop, gs, DestoryPresenceCause.SkipInvaderAction, unitOfWork ); // it is the invader actions we are stopping
		_self.Energy -= energyCost;

		return true;
	}
}
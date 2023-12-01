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

	public ChokeTheLandWithGreen( ASpreadOfRampantGreen self ):base(self) {
		_self = self;
	}

	public UsageCost Cost => UsageCost.Extreme; // we lose presence!

	bool IsSacredSite( SpaceState space ) => 2 <= space[this]; // !! could make public and promote to base class and have SpiritPresence use it.

	async Task<bool> ISkipRavages.Skip( SpaceState space ) {
		return await SkipInvaderAction( space, "ravage" );
	}

	string ISkipBuilds.Text => SpaceAbreviation;
	async Task<bool> ISkipBuilds.Skip( SpaceState space )
		=> await SkipInvaderAction( space, $"build of {BuildEngine.InvaderToAdd.Value.Label}" );

	async Task<bool> SkipInvaderAction( SpaceState space, string actionDescription ) {
		if(!IsSacredSite( space )) return false;

		GameState gs = GameState.Current;
		int energyCost = gs.BlightCard.CardFlipped ? 1 : 0;
		if(_self.Energy < energyCost) return false;

		var stop = await _self.SelectAsync( new A.Space( $"Stop {actionDescription} on {space.Space.Text} by destroying 1 presence", new Space[] { space.Space }, Present.Done ) );
		if(stop == null) return false;

		await using var actionScope = await ActionScope.Start(ActionCategory.Spirit_SpecialRule); // Special Rules! - it is the invader actions we are stopping
		await stop.Tokens.Destroy( this, 1 );
		_self.Energy -= energyCost;

		return true;
	}
}
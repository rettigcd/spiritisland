namespace SpiritIsland.JaggedEarth;

class LingeringPestilence : SpiritPresence.DefaultDestroyBehavior {

	static public SpecialRule Rule => new SpecialRule(
		"Lingering Pestilence",
		"When your presence is destroyed by anything except a Spirit action, add 1 disease where each destroyed presence was."
	);

	public override async Task DestroyPresenceApi( SpiritPresence presence, Space space, GameState gs, DestoryPresenceCause actionType, Guid actionId ) {
		if( actionType != DestoryPresenceCause.SpiritPower )
			await gs.Tokens[space].Disease.Bind( actionId ).Add(1);
		await base.DestroyPresenceApi(presence,space,gs,actionType,actionId);
	}

}
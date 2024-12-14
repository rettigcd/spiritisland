namespace SpiritIsland.JaggedEarth;

class ExplosiveHelper(Spirit spirit):ElementMgr(spirit) {

	public override async Task<IDrawableInnateTier> SelectInnateTierToActivate(IEnumerable<IDrawableInnateTier> innateOptions) {

		IDrawableInnateTier match = null;
		int destroyedThisAction = VolcanoPresence.GetPresenceDestroyedThisAction();
		foreach( var option in innateOptions.OrderBy(o => o.Elements.Total) ) {
			if( option is ExplosiveInnateOptionAttribute ex && destroyedThisAction < ex.DestroyedPresenceThreshold )
				continue;

			if( await HasElement(option.Elements, "Innate Tier", ThresholdType.Innate) )
				match = option;
		}
		return match;
	}

}
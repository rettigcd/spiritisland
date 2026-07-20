namespace SpiritIsland;

#nullable enable

/// <summary>
/// Shared by every expansion's GameComponentProvider.RegisterSerialization() to also harvest
/// aspect-exclusive PowerCards/InnatePowers - ones an aspect swaps in that no base spirit's
/// Hand/InnatePowers ever exposes (e.g. Lightning's Pandemonium replacing ThunderingDestruction with
/// LightningTornSkiesIncitePandemonium). Reads each aspect's own IAspect.NewCards/NewInnates rather than
/// constructing a spirit and calling ModSpirit - purely declarative, so seeding never has to trust an
/// aspect's ModSpirit to be safe/idempotent when invoked outside a real game (see Bringer.DreamMod for
/// why that trust once broke).
/// </summary>
public static class GameComponentProviderSeeding {

	public static void RegisterAspectExclusiveCards( IGameComponentProvider provider ) {
		foreach( AspectConfigKey key in provider.AspectNames ) {
			IAspect aspect = provider.MakeAspect( key )
				?? throw new InvalidOperationException( $"AspectNames listed '{key.Aspect}' but MakeAspect returned null for it." );
			foreach( PowerCard card in aspect.NewCards ) PowerCardRegistry.Register( card );
			foreach( InnatePower innate in aspect.NewInnates ) InnatePowerRegistry.Register( innate );
		}
	}

}

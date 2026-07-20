namespace SpiritIsland.NatureIncarnate;

public class SlowDissolutionOfWill : BlightCard {

	public SlowDissolutionOfWill()
		:base("Slow Dissolution of Will",
			"Immediately: Each Spirit chooses one of Badlands, Beast, or Wilds. Each Invader Phase: Each Spirit Replaces 1 Presence with their chosen type of Spirit Token.",
			3
		)
	{}

	// The recurring Invader-Phase behavior - and the per-spirit token choice it depends on - lives on
	// its own SlowDissolutionOfWillMod (IRunBeforeInvaderPhase only, not this BlightCard) once
	// triggered, so it's addressable/serializable independently of this card's own identity.
	public override IActOn<GameState> Immediately => new BaseCmd<GameState>( Description, async gs => {
		var mod = new SlowDissolutionOfWillMod();
		await new BaseCmd<Spirit>( SlowDissolutionOfWillMod.ChooseTokenPrompt, mod.ChooseToken ).ForEachSpirit().ActAsync( gs );
		gs.AddPreInvaderPhaseAction( mod );
	} );

	[ModuleInitializer]
	internal static void RegisterSerialization() => BlightCardRegistry.Register( nameof( SlowDissolutionOfWill ), ( json, ctx ) => new SlowDissolutionOfWill() );

}

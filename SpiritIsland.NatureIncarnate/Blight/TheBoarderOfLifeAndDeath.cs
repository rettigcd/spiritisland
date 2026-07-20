namespace SpiritIsland.NatureIncarnate;

public sealed class TheBorderOfLifeAndDeath : StillHealthyBlightCard, IRunBeforeInvaderPhase {

	public TheBorderOfLifeAndDeath():base(
		"The Border of Life and Death",
		"Now and Each Invader Phase (until this card is replaced): Each Spirit with at least 2 Presence on the island Destroys 1 Presence and may discard a Power Card to gain 1 Energy",
		1
	) {}

	public override IActOn<GameState> Immediately
		=> Cmd.Multiple(
			CardAction,
			RunAtTheStartOfEachInvadorPhase(this)
		);

	bool IRunBeforeInvaderPhase.RemoveAfterRun => false;

	Task IRunBeforeInvaderPhase.BeforeInvaderPhase( GameState gameState )
		=> CardAction.ActAsync( gameState );

	static IActOn<GameState> CardAction => DiscardPowerCardForEnergy
		.ForEachSpirit()
		.Who(Has.AtLeastNPresenceOnIsland(2));

	static SpiritAction DiscardPowerCardForEnergy => new SpiritAction(
		"Discard 1 Power Card to gain 1 Energy.",
		async spirit => {
			var action = new DiscardCards(1).ConfigAsOptional();
			await action.ActAsync(spirit);
			if(action.Discarded!.Count != 0)
				spirit.Energy++;
		}
	);

	// Real finding: unlike its 6 siblings (DownwardSpiral, MemoryFadesToDust, PowerCorrodesTheSpirit,
	// UntendedLandCrumbles, AttenuatedEssence, BlightCorrodesTheSpirit), this card had no
	// [ModuleInitializer] registration at all - it didn't round-trip through BlightCardRegistry despite
	// section 7 of docs/GameSerialization-Roadmap.md claiming full coverage. Fixed here alongside the
	// section 10 identity-resolution work, since both are needed for this card to serialize at all.
	[ModuleInitializer]
	internal static void RegisterSerialization() {
		BlightCardRegistry.Register( nameof( TheBorderOfLifeAndDeath ), ( json, ctx ) => new TheBorderOfLifeAndDeath() );
		PreInvaderPhaseActionRegistry.Register( nameof( TheBorderOfLifeAndDeath ), ( json, ctx ) => (IRunBeforeInvaderPhase)ctx.BlightCard );
	}

}
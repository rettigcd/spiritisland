namespace SpiritIsland.NatureIncarnate;

public class AttenuatedEssence : BlightCard, IRunBeforeInvaderPhase {

	public AttenuatedEssence()
		:base("Attenuated Essence",
			"Each Invader Phase: Each Spirit with at least 5 Presence on the island Destroys 1 Presence.",
			4
		)
	{}

	public override IActOn<GameState> Immediately => RunAtTheStartOfEachInvadorPhase(this);

	bool IRunBeforeInvaderPhase.RemoveAfterRun => false;

	Task IRunBeforeInvaderPhase.BeforeInvaderPhase( GameState gameState )
		=> Cmd.DestroyPresence(1)
			.ForEachSpirit()
			.Who(Has.AtLeastNPresenceOnIsland(5))
			.ActAsync( gameState );

	

}
namespace SpiritIsland.NatureIncarnate;

public class AttenuatedEssence : BlightCard {

	public AttenuatedEssence()
		:base("Attenuated Essence", 
			"Each Invader Phase: Each Spirit with at least 5 Presence on the island Destroys 1 Presence.", 
			4
		) 
	{}

	public override IActOn<GameState> Immediately 
		=> Cmd.DestroyPresence(1)
			.ForEachSpirit()
			.Who(Has.AtLeastNPresenceOnIsland(5))
			.AtTheStartOfEachInvaderPhase();

}
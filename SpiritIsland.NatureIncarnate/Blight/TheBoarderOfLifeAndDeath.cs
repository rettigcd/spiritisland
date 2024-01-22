namespace SpiritIsland.NatureIncarnate;

public sealed class TheBorderOfLifeAndDeath : StillHealthyBlightCard {

	public TheBorderOfLifeAndDeath():base(
		"The Border of Life and Death", 
		"Now and Each Invader Phase (until this card is replaced): Each Spirit with at least 2 Presence on the island Destroys 1 Presence and may discard a Power Card to gain 1 Energy", 
		1
	) {}

	public override IActOn<GameState> Immediately 
		=> Cmd.Multiple(
			CardAction,
			CardAction.AtTheStartOfEachInvaderPhase()
		);

	static IActOn<GameState> CardAction => DiscardPowerCardForEnergy
		.ForEachSpirit()
		.Who(Has.AtLeastNPresenceOnIsland(2));

	static SpiritAction DiscardPowerCardForEnergy => new SpiritAction(
		"Discard 1 Power Card to gain 1 Energy.",
		async spirit => {
			var action = new DiscardCards(1).ConfigAsOptional();
			await action.ActAsync(spirit);
			if(action.Discarded.Count != 0)
				spirit.Energy++;
		}
	);

}
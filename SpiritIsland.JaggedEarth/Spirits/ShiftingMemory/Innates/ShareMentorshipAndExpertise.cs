namespace SpiritIsland.JaggedEarth;

[InnatePower(Name), Fast, AnotherSpirit]
public class ShareMentorshipAndExpertise {

	public const string Name = "Share Mentorship and Expertise";

	[InnateTier("1 air", "Put a Power Card from your hand or discard into target Spirit's hand.", 0)]
	static public Task Option1(TargetSpiritCtx ctx) {
		// Put a Power Card from your hand or discard into target Spirit's hand.
		return Task.CompletedTask;
	}

	[InnateTier("3 air,2 earth", "Target Spirit may play that Power Card now by paying its cost.", 1)]
	static public Task Option2(TargetSpiritCtx ctx) {
		// Target Spirit may play that Power Card now by paying its cost.
		return Task.CompletedTask;
	}

	[InnateTier("1 sun,4 air,3 earth", "Target Spirit may Repeat that Power Card once this turn by paying its cost.", 2)]
	static public Task Option3(TargetSpiritCtx ctx) {
		// Target Spirit may Repeat that Power Card once this turn by paying its cost.
		return Task.CompletedTask;
	}

	[InnateTier("1 air", "Prepare 1 Element Market matching an Element on that Power Card.Prepare 1 Element Marker of your choice.", 3)]
	static public Task Option4(TargetSpiritCtx ctx) {
		// Prepare 1 Element Market matching an Element on that Power Card. Prepare 1 Element Marker of your choice.
		return Task.CompletedTask;
	}


}

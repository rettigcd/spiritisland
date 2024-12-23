namespace SpiritIsland.JaggedEarth;

[InnatePower(Name), Fast, AnotherSpirit]
public class ShareMentorshipAndExpertise {

	public const string Name = "Share Mentorship and Expertise";

	[InnateTier("1 air", "Put a Power Card from your hand or discard into target Spirit's hand.")]
	static public async Task Option1(TargetSpiritCtx ctx) {
		await PutCardInTargetSpiritsHand(ctx);
	}

	[InnateTier("3 air,2 earth", "Target Spirit may play that Power Card now by paying its cost.")]
	static public async Task Option2(TargetSpiritCtx ctx) {
		var card = await PutCardInTargetSpiritsHand(ctx);
		if(card is null) return;
		// Target Spirit may play that Power Card now by paying its cost.
		await PlayNowByPayingCost(card,ctx.Other);
	}

	[InnateTier("1 sun,4 air,3 earth", "Target Spirit may Repeat that Power Card once this turn by paying its cost.")]
	static public async Task Option3(TargetSpiritCtx ctx) {
		var card = await PutCardInTargetSpiritsHand(ctx);
		if( card is null ) return;
		bool played = await PlayNowByPayingCost(card, ctx.Other);
		// Target Spirit may Repeat that Power Card once this turn by paying its cost.
		if( played )
			ctx.Other.AddActionFactory(new RepeatSpecificCardForCost(card));
	}

	[InnateTier("1 air", "Prepare 1 Element matching an Element on that Power Card. Prepare 1 Element of your choice.", 3)]
	static public async Task Option4(TargetSpiritCtx ctx) {
		var card = await PutCardInTargetSpiritsHand(ctx);
		if( card is null ) return;
		bool played = await PlayNowByPayingCost(card, ctx.Other);
		if( played )
			ctx.Other.AddActionFactory(new RepeatSpecificCardForCost(card));
		// Prepare 1 Element Marker matching an Element on that Power Card.
		if(ctx.Self is not ShiftingMemoryOfAges smoa) return;
		await smoa.PreparedElementMgr.Prepare("Prepare element from card.", [..card.Elements.Keys]);
		// Prepare 1 Element Marker of your choice.
		await smoa.PreparedElementMgr.Prepare("Prepare any element.");
	}

	#region private helper

	static async Task<PowerCard?> PutCardInTargetSpiritsHand(TargetSpiritCtx ctx) {
		// Put a Power Card from your hand or discard into target Spirit's hand.
		var card = await ctx.Self.SelectAlways("Put Power Card into target Spirit's hand.", ctx.Self.Hand.Union(ctx.Self.DiscardPile));
		if( card == null ) return null; // all cards played, nothing in hand + discard

		if( !ctx.Self.Hand.Remove(card) )
			ctx.Self.DiscardPile.Remove(card);
		ctx.Other.Hand.Add(card);
		return card;
	}

	static async Task<bool> PlayNowByPayingCost(PowerCard card, Spirit other) {
		bool played = card.Cost <= other.Energy && await other.UserSelectsFirstText($"Play {card.Title} now by paying {card.Cost} Energy?", "Yes, play it!", "No thanks");
		if( played )
			other.PlayCard(card);
		return played;
	}

	#endregion private helper

}

public class RepeatSpecificCardForCost(PowerCard card) : RepeatCardForCost([]) {
	public override PowerCard[] GetCardOptions(Spirit self, Phase phase) {
		return base.GetCardOptions(self, phase).Where(x => x == card).ToArray();
	}
}
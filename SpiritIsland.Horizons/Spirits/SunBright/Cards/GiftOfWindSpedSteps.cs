namespace SpiritIsland.Horizons;

public class GiftOfWindSpedSteps {

	public const string Name = "Gift of Wind-Sped Steps";

	[SpiritCard(Name, 1, Element.Sun, Element.Air, Element.Animal), Fast, AnySpirit]
	[Instructions("Once this turn, Target Spirit may choose 1 of their Slow Powers with a Push or Gather instruction and make that Power Fast.  If you target another Spirit, they gain 1 Energy."), Artist(Artists.LucasDurham)]
	static public Task ActAsync(TargetSpiritCtx ctx) {
		// Once this turn, Target Spirit may choose 1 of their Slow Powers with a Push or Gather instruction and make that Power Fast.
		ctx.Other.Mods.Add(new Run1SlowPushOrGatherAsFast(ctx.Other));
		// If you target another Spirit, they gain 1 Energy.
		if( ctx.Other != ctx.Self )
			ctx.Other.Energy++;
		return Task.CompletedTask;
	}

}

class Run1SlowPushOrGatherAsFast(Spirit spirit) : RunSlowCardsAsFastMod_EveryRound(spirit), IEndWhenTimePasses {

	protected override int AllowedCount => 1;

	protected override bool EvaluateAction(IActionFactory action)
		=> base.EvaluateAction(action) && IsPushOrGather(action);

	static bool IsPushOrGather(IActionFactory slowAction) {
		if(slowAction is PowerCard pc )
			return ContainsGatherPush( pc.Instructions );
		if(slowAction is InnatePower ip )
			return ip.DrawableOptions.Any(i=>ContainsGatherPush(i.Description));
		return false;
	}

	static bool ContainsGatherPush(string s) {
		s = s.ToLower();
		return s.Contains("push") || s.Contains("gather");
	}

}

namespace SpiritIsland.Basegame;

[InnatePower(DarknessSwallowsTheUnwary.Name), Fast]
[FromPresence(2)]
public class StretchOutCoilsOfForebodingDread {

	public const string Name = "Stretch Out Coils of Foreboding Dread";

	[InnateTier("2 air", "Your other Powers may ignore Range when targeting the target land.")]
	public static Task Gather1Explorer(TargetSpaceCtx ctx) {
		// Your other Powers may ignore Range when targeting the target land.
		ctx.Self.PowerRangeCalc = new IncludeLand(ctx.Self, ctx.Self.PowerRangeCalc, ctx.Space);
		return Task.CompletedTask;
	}

	[InnateTier("1 moon", "After an Action generates Fear in target land, including from Destroying Towns/Cities: Push up to 1 Explorer per Fear / 1 Town per 2 Fear.")]
	public static Task Plus_Destroy2Explorers(TargetSpaceCtx ctx) {
		// After an Action generates Fear in target land, (including from Destroying Towns/Cities):
		// Push up to 1 Explorer per Fear / 1 Town per 2 Fear. (You may mix-and-match.)
		ctx.Space.Init(new FearPushesInvaders(),1);
		return Task.CompletedTask;
	}

	[InnateTier("2 fire", "1 fear")]
	public static Task Plus_3Damage(TargetSpaceCtx ctx) {
		// 1 Fear.
		return ctx.AddFear(1);
	}

	[InnateTier("2 moon,4 air", "2 Fear.")]
	public static Task Plus_3Damag1222e(TargetSpaceCtx ctx) {
		// 2 Fear.
		return ctx.AddFear(2);
	}


}

class FearPushesInvaders : IReactToLandFear, IEndWhenTimePasses {
	public Task HandleFearAddedAsync(Space space, int fearAdded, FearType fearType) {
		if (space[this] == 1 ) {
			ActionScope.Current.AtEndOfThisAction( (actionScope) => ApplyFear(actionScope,space) );
		}
		space.Adjust(this,fearAdded); // HACK
		return Task.CompletedTask;
	}

	async Task ApplyFear(ActionScope scope, Space space) {
		int pushFear = space[this] - 1; // HACK
		space.Init(this, 1);

		var spirit = scope.Owner;

		// DO MOVE
		HumanToken[] tokens = pushFear switch { 0 => [], 1 => space.HumanOfTag(Human.Explorer), _ => space.HumanOfAnyTag(Human.Explorer_Town) };
		while( 0 < tokens.Length ) {
			// Select token to push
			var token = await spirit.SelectAsync(new A.SpaceTokenDecision($"{pushFear} fear - Push Invader",tokens.On(space), Present.Done ));
			// if null; break
			if(token is null) break;
			
			var destination = await spirit.SelectAsync(new A.SpaceDecision("Push to", space.Adjacent,Present.Always));
			if(destination is null) break; // should not happen

			pushFear -= token.Token.HasTag(Human.Town) ? 2 : 1;
			await token.MoveToAsync(destination);
		}
		
	}
}


class IncludeLand(Spirit spirit, ICalcRange previous, Space target) : DefaultRangeCalculator(previous) {
	public override TargetRoutes GetTargetingRoute(Space source, TargetCriteria tc) {
		var routes = Previous.GetTargetingRoute(source, tc);
		routes.AddRoutes(RoutesToTarget(tc));
		return routes;
	}

	IEnumerable<TargetRoute> RoutesToTarget(TargetCriteria tc) => spirit.Presence.Lands
		.Where(tc.Matches)
		.Select(s => new TargetRoute(s, target));

}
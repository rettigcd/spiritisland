namespace SpiritIsland.Basegame;

/// <summary>
/// Mod used by Stretch Out Coils Of Foreboding Dread
/// </summary>
class FearPushesInvaders : ISpaceEntity, IReactToLandFear, IEndWhenTimePasses {

	public Task HandleFearAddedAsync(Space space, int fearAdded, FearType fearType) {

		if (space[this] == 1 )
			ActionScope.Current.AtEndOfThisAction( (actionScope) => ApplyFear(actionScope,space) );

		space.Adjust(this,fearAdded); // HACK
		return Task.CompletedTask;
	}

	async Task ApplyFear(ActionScope scope, Space space) {
		int pushFear = space[this] - 1; // HACK
		space.Init(this, 1);

		// ! This is only called from within an Innate so always have an owner
		var spirit = scope.Owner!;

		// DO MOVE
		HumanToken[] tokens = pushFear switch { 0 => [], 1 => space.HumanOfTag(Human.Explorer), _ => space.HumanOfAnyTag(Human.Explorer_Town) };
		while( 0 < tokens.Length ) {
			// Select token to push
			var token = await spirit.Select(new A.SpaceTokenDecision($"{pushFear} fear - Push Invader",tokens.On(space), Present.Done ));
			// if null; break
			if(token is null) break;
			
			var destination = await spirit.Select("Push to", space.Adjacent,Present.Always);
			if(destination is null) break; // should not happen

			pushFear -= token.Token.HasTag(Human.Town) ? 2 : 1;
			await token.MoveToAsync(destination);
		}
		
	}
}

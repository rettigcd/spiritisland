namespace SpiritIsland.NatureIncarnate;

public class ExaltationOfTheIncandescentSky {

	public const string Name = "Exaltation of the Incandescent Sky";

	[MajorCard(Name,7,"sun,fire,air,water"),Fast]
	[AnotherSpirit]
	[Instructions( "Target Spirit may play 1 Power Card by paying its cost, make up to 2 of their Powers Fast this turn, and do 3 Damage in one of their lands. You may do likewise. -If you have- 3 sun,3 fire,4 air,2 water: In any 4 lands on the island, Skip 1 Invader Action. 5 fear (total)." ), Artist( Artists.KatGuevara )]
	static public async Task ActAsync(TargetSpiritCtx ctx){
		// Target Spirit may:
		await PlayCardMakeFastDoDamage(ctx.Other);

		// You may do likewise.
		await PlayCardMakeFastDoDamage(ctx.Self);

		// -If you have- 3 sun,3 fire,4 air,2 water:
		if(await ctx.YouHave("3 sun,2 fire,4 air,2 water" )) {
			// In any 4 lands on the island,
			var spaceOptions = ActionScope.Current.Spaces.ToList();
			int count = 4;
			while(0 < count--) {
				var space = await ctx.Self.Select($"Skip Invader Action ({count + 1} of 4)", spaceOptions, Present.Done);
				if(space is null) break;
				spaceOptions.Remove(space);
				// Skip 1 Invader Action.
				space.Skip1InvaderAction(Name,ctx.Self);
			}

			// 5 fear (total).
			ctx.AddFear(5); // Not exatly correct, fear is supposed to be in 4 lands, but this is good enough.
		}
	}

	static async Task PlayCardMakeFastDoDamage(Spirit self) {
		// play 1 Power Card by paying its cost
		await new PlayCardForCost(Present.Done).ActAsync(self);

		// make up to 2 of their Powers Fast this turn, and
		self.AddActionFactory(new ResolveSlowDuringFast()); 
		self.AddActionFactory(new ResolveSlowDuringFast());

		// do 3 Damage in one of their lands.
		Space? space = await self.Select("Do 3 Damage in land", self.Presence.Lands, Present.Done);
		if(space is null) return;
		await self.Target(space).DamageInvaders(3);
	}
}

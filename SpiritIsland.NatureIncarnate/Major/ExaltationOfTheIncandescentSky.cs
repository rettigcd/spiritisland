namespace SpiritIsland.NatureIncarnate;

public class ExaltationOfTheIncandescentSky {

	public const string Name = "Exaltation of the Incandescent Sky";

	[MajorCard(Name,7,"sun,fire,air,water"),Fast]
	[AnotherSpirit]
	[Instructions( "Target Spirit may play 1 Power Card by paying its cost, make up to 2 of their Powers Fast this turn, and do 3 Damage in one of their lands. You may do likewise. -If you have- 3 sun,3 fire,4 air,2 water: In any 4 lands on the island, Skip 1 Invader Action. 5 fear (total)." ), Artist( Artists.KatGuevara )]
	static public async Task ActAsync(TargetSpiritCtx ctx){
		// Target Spirit may:
		await PlayCardMakeFastDoDamage(ctx.OtherCtx);

		// You may do likewise.
		await PlayCardMakeFastDoDamage(ctx);

		// -If you have- 3 sun,3 fire,4 air,2 water:
		if(await ctx.YouHave("3 sun,2 fire,4 air,2 water" )) {
			// In any 4 lands on the island,
			var spaceOptions = GameState.Current.Spaces.ToList();
			int count = 4;
			while(0 < count--) {
				var space = await ctx.Self.Select(new A.Space($"Skip Invader Action ({count+1} of 4)",spaceOptions,Present.Done));
				if(space == null) break;
				spaceOptions.Remove(space);
				// Skip 1 Invader Action.
				space.Tokens.Skip1InvaderAction(Name,ctx.Self);
			}

			// 5 fear (total).
			ctx.AddFear(5); // Not exatly correct, fear is supposed to be in 4 lands, but this is good enough.
		}
	}

	static async Task PlayCardMakeFastDoDamage(SelfCtx ctx ) {
		// play 1 Power Card by paying its cost
		await new PlayCardForCost(Present.Done).ActivateAsync(ctx);

		// make up to 2 of their Powers Fast this turn, and
		ctx.Self.AddActionFactory(new ResolveSlowDuringFast()); 
		ctx.Self.AddActionFactory(new ResolveSlowDuringFast());

		// do 3 Damage in one of their lands.
		Space space = await ctx.Self.Select(new A.Space("Do 3 Damage in land",ctx.Self.Presence.Spaces,Present.Done));
		if(space == null) return;
		await space.Tokens.DamageInvaders(ctx.Self,3);
	}
}

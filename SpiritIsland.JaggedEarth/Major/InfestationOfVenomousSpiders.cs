namespace SpiritIsland.JaggedEarth;

public class InfestationOfVenomousSpiders {

	public const string Name = "Infestation of Venomous Spiders";

	[MajorCard(Name,4,Element.Air,Element.Earth,Element.Plant,Element.Animal), Fast, FromSacredSite(2,Target.Invaders)]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// add 1 beast
		await ctx.Beasts.Add(1);

		// Gather up to 1 beast.
		await ctx.GatherUpTo(1, Token.Beast);

		// if you have 2 air 2 earth 3 animal: after this power causes invaders to skip an action, 4 damage.
		Func<GameState,SpaceState,Task> causeAdditionalDamage = await ctx.YouHave("2 air,3 animal")
			? (GameState gs,SpaceState space) => ctx.Target(space.Space).DamageInvaders(4) // this correctly uses Bringers ctx to do Dream damage
			: null;

		// For each beast,
		int count = ctx.Beasts.Count;
		// 1 fear (max 4) and
		ctx.AddFear( System.Math.Min(4,count) );
		for(int i = 0; i < count; ++i)
			ctx.Tokens.Adjust( new SkipAnyInvaderAction( Name, ctx.Self, causeAdditionalDamage ), 1 );

		// !!! Issue 1 - Shouldn't be able to skip ravage twice if there is only 1 ravage occurring
		// !!! Issue 2 - Shouldn't be able to skip actions that don't happen
		// !!! Issue 3 - Shouldn't have to pick action until it happens.
		// This will help limit damage to 4 instead of 4 * # of beasts.
		// !!! If there are multiple 'skips' players should be able to decide which ones to take and in which order.
	}

}
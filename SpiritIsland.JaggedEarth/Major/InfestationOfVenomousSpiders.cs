namespace SpiritIsland.JaggedEarth;

public class InfestationOfVenomousSpiders {

	public const string Name = "Infestation of Venomous Spiders";

	[MajorCard(Name,4,Element.Air,Element.Earth,Element.Plant,Element.Animal), Fast, FromSacredSite(2,Target.Invaders)]
	[Instructions( "Add 1 Beasts. Gather up to 1 Beasts. For each Beasts, 1 Fear (max. 4) and Invaders skip one Action in target land. -If you have- 2 Air, 2 Earth, 3 Animal; After this Power causes Invaders to skip an Action, 4 Damage." ), Artist( Artists.LucasDurham )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// add 1 beast
		await ctx.Beasts.AddAsync(1);

		// Gather up to 1 beast.
		await ctx.GatherUpTo(1, Token.Beast);

		// if you have 2 air 2 earth 3 animal: after this power causes invaders to skip an action, 4 damage.
		Func<SpaceState,Task> causeAdditionalDamage = await ctx.YouHave("2 air,3 animal")
			? (SpaceState space) => ctx.Target(space.Space).DamageInvaders(4) // this correctly uses Bringers ctx to do Dream damage
			: null;

		// For each beast,
		int count = ctx.Beasts.Count;
		// 1 fear (max 4) and
		ctx.AddFear( System.Math.Min(4,count) );
		for(int i = 0; i < count; ++i)
			ctx.Tokens.Skip1InvaderAction( Name, ctx.Self, causeAdditionalDamage ); // !!! derive a new type instead of this alt-action nonsense
	}

}
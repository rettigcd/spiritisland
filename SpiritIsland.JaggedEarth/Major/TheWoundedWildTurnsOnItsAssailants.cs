namespace SpiritIsland.JaggedEarth;

public class TheWoundedWildTurnsOnItsAssailants {

	const string Name = "The Wounded Wild Turns on Its Assailants";

	[MajorCard(Name,4,Element.Fire,Element.Plant,Element.Animal), Slow, FromPresence(1,Filter.Blight)]
	[Instructions( "Add 2 Badlands. Gather up to 2 Beasts. 1 Damage per Blight / Beasts / Wilds. -If you have- 2 Fire, 3 Plant, 2 Animal: 2 Fear per Invader destroyed by the Power (max. 8 Fear)." ), Artist( Artists.JoshuaWright )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// Add 2 badlands
		await ctx.Badlands.AddAsync( 2 );

		// Gather up to 2 beast
		await ctx.GatherUpTo(2,Token.Beast);

		// (watch for invaders destroyed in this land)
		// the only way a token will be removed, is if it is destroyed
		var destroyedCounter = new CountDestroyedTokens();
		ctx.Space.Adjust(destroyedCounter, 1 );

		// 1 damamge per blight/beast/wilds.
		await ctx.DamageInvaders( ctx.Blight.Count + ctx.Beasts.Count + ctx.Wilds.Count );

		// if you have 2 fire, 3 air, 2 animal
		if( await ctx.YouHave("2 fire,3 air,2 animal"))
			// 2 fear per invader destroyed by this Power (max 8 fear)
			await ctx.AddFear(System.Math.Min(8, destroyedCounter.Count * 2));

	}

	public class CountDestroyedTokens : BaseModEntity, IHandleTokenRemoved, ISerializableSpaceEntity {
		public int Count { get; private set; }
		public Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
			Count++;
			return Task.CompletedTask;
		}

		JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, Count );

		const string Tag = "CountDestroyedTokens";

		[ModuleInitializer]
		internal static void RegisterSerialization()
			=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new CountDestroyedTokens() { Count = json[1]!.GetValue<int>() } );
	}

}
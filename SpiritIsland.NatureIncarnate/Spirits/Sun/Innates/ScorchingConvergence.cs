namespace SpiritIsland.NatureIncarnate;

[InnatePower("Scorching Convergence")]
[Slow,FromSacredSite(1)]
public class ScorchingConvergence {

	[InnateTier("2 sun","Move all of your Presence from origin land directly to target land. 1 Damage to Town/City only.")]
	public static async Task Option1(TargetSpaceCtx ctx ) {
		// Move all of your Presence from origin land directly to target land.
		var originOptions = TargetSpaceAttribute.TargettedSpace!.Sources;

		await new TokenMover( ctx.Self, "Move",
			new SourceSelector( originOptions ).FromASingleLand(),
			new DestinationSelector( ctx.Space )
		)
			.AddAll( ctx.Self.Presence )
			.DoN();

		// record Starting Invaders / Dahan
		ActionScope.Current[StartingInvaders] = GetSummary( ctx.Space, Human.Invader);
		ActionScope.Current[StartingDahan] = GetSummary( ctx.Space, Human.Dahan );

		// 1 Damage to Town/City only.
		await ctx.DamageInvaders(1, Human.Town_City);
	}

	const string StartingInvaders = "starting invaders";
	const string StartingDahan = "starting dahan";

	static string GetSummary(Space space, params HumanTokenClass[] classes) => space.HumanOfAnyTag(classes)
		.OrderBy(x=>x.FullHealth).ThenBy(x=>x.Damage)
		.Select( x => space[x]+x.HumanClass.Label )
		.Join(",");

	[InnateTier("3 sun,1 fire","3 Damage to Invaders. 3 Damage to Dahan. Add 1 Blight without cascading.")]
	public static async Task Option2(TargetSpaceCtx ctx ) {
		// 3 Damage to Invaders.
		await ctx.DamageInvaders( 3 );
		// 3 Damage to Dahan.
		await ctx.DamageDahan( 3 );
		// Add 1 Blight without cascading.
		BlightToken.ScopeConfig.ShouldCascade = false;
		await ctx.AddBlight(1);
	}

	[InnateTier("4 sun,2 fire,1 air","3 Fear if this Power destroyed only Invaders.")]
	public static Task Option3(TargetSpaceCtx ctx ) {
		var actionScope = ActionScope.Current;
		// 3 Fear if this Power destroyed only Invaders.
		return actionScope.SafeGet(StartingInvaders,"") != GetSummary( ctx.Space, Human.Invader )
			&& actionScope.SafeGet(StartingDahan,"") == GetSummary( ctx.Space, Human.Dahan )
			? ctx.AddFear( 3 )
			: Task.CompletedTask;
	}

	[InnateTier( "5 sun,3 fire,2 air", "1 Damage per remaining Presence of yours in target land." )]
	public static async Task Option4( TargetSpaceCtx ctx ) {
		// 1 Damage per remaining Presence of yours in target land.
		await ctx.DamageInvaders( ctx.PresenceCount );
	}

}
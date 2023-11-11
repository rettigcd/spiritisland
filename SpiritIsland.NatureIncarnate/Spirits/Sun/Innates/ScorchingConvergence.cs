using System.Runtime.CompilerServices;

namespace SpiritIsland.NatureIncarnate;

[InnatePower("Scorching Convergence")]
[Slow,FromSacredSite(1)]
public class ScorchingConvergence {

	[InnateOption("2 sun","Move all of your Presence from origin land directly to target land. 1 Damage to Town/City only.")]
	public static async Task Option1(TargetSpaceCtx ctx ) {
		// Move all of your Presence from origin land directly to target land.
		IEnumerable<Space> sourceOptions = ctx.Self
			.FindTargettingSourcesFor(
				ctx.Space,
				new TargetingSourceCriteria( From.SacredSite ),
				new TargetCriteria( 1 )
			)
			.Downgrade();

		var from = await ctx.Self.Select(A.SpaceToken.ToCollect("Move all presence", ctx.Self.Presence.Token.On( sourceOptions ), Present.Always, ctx.Space ));
		if(from != null && from.Space != ctx.Space )
			while( from != null && from.Exists )
				await from.MoveTo(ctx.Tokens);

		// record Starting Invaders / Dahan
		ActionScope.Current[StartingInvaders] = GetSummary( ctx.Tokens, Human.Invader);
		ActionScope.Current[StartingDahan] = GetSummary( ctx.Tokens, Human.Dahan );

		// 1 Damage to Town/City only.
		await ctx.DamageInvaders(1, Human.Town_City);
	}

	const string StartingInvaders = "starting invaders";
	const string StartingDahan = "starting dahan";

	static string GetSummary(SpaceState tokens, params HumanTokenClass[] classes) => tokens.OfAnyHumanClass(classes)
		.OrderBy(x=>x.FullHealth).ThenBy(x=>x.Damage)
		.Select( x => tokens[x]+x.Class.Label )
		.Join(",");

	[InnateOption("3 sun,1 fire","3 Damage to Invaders. 3 Damage to Dahan. Add 1 Blight without cascading.")]
	public static async Task Option2(TargetSpaceCtx ctx ) {
		// 3 Damage to Invaders.
		await ctx.DamageInvaders( 3 );
		// 3 Damage to Dahan.
		await ctx.DamageDahan( 3 );
		// Add 1 Blight without cascading.
		BlightToken.ForThisAction.ShouldCascade = false;
		await ctx.AddBlight(1);
	}

	[InnateOption("4 sun,2 fire,1 air","3 Fear if this Power destroyed only Invaders.")]
	public static Task Option3(TargetSpaceCtx ctx ) {
		var actionScope = ActionScope.Current;
		// 3 Fear if this Power destroyed only Invaders.
		if(actionScope.SafeGet<string>(StartingInvaders) != GetSummary( ctx.Tokens, Human.Invader )
			&& actionScope.SafeGet<string>(StartingDahan) == GetSummary( ctx.Tokens, Human.Dahan )
        )
			ctx.AddFear( 3 );
		return Task.CompletedTask;
	}

	[InnateOption( "5 sun,3 fire,2 air", "1 Damage per remaining Presence of yours in target land." )]
	public static async Task Option4( TargetSpaceCtx ctx ) {
		// 1 Damage per remaining Presence of yours in target land.
		await ctx.DamageInvaders( ctx.PresenceCount );
	}

	static HashSet<SpaceState> FindSacredSitesOrigin( TargetSpaceCtx ctx, TargetCriteria tc ) {
		return ctx.Self.Presence.SacredSites
			.Where( ss => ctx.Self.IsOriginFor( ss, ctx.Space, tc ) )
			.ToHashSet();
	}


}
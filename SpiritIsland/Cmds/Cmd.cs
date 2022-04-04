namespace SpiritIsland;

public static partial class Cmd {

	static public SpaceAction Destroy2FewerDahan => new SpaceAction(
		"Each time Dahan would be Destroyed in target land, Destroy 2 fewer Dahan.", 
		// !!! This only stops Ravage destroys, not other. (Are there any other?)
		ctx=> ctx.GameState.ModifyRavage(ctx.Space, cfg => { 
			var oldDestroy = cfg.DestroyDahan;
			cfg.DestroyDahan = (dahan,count,health) => oldDestroy(dahan,count-2,health);
		} ) 
	);

	// Gather
	static public SpaceAction GatherUpToNDahan( int count ) => new SpaceAction( $"Gather up to {count} Dahan", ctx => ctx.GatherUpToNDahan( count ) );
	static public SpaceAction GatherUpToNExplorers( int count ) => new SpaceAction( $"Gather up to {count} Explorers", ctx => ctx.GatherUpTo(count,Invader.Explorer));
	static public SpaceAction GatherUpToNInvaders( int count, params TokenClass[] classes ) => new SpaceAction( $"Gather up to {count} " + classes.Select(c=>c.Label).Join("/"), ctx => ctx.GatherUpTo( count, classes ) );

	// Push
	static public SpaceAction PushUpToNDahan( int count ) => new SpaceAction( $"Push up to {count} Dahan", ctx => ctx.PushUpToNDahan( count ) ).Matches(ctx=>ctx.Dahan.Any );
	static public SpaceAction PushNDahan(int count ) => new SpaceAction( $"Push {count} dahan", ctx => ctx.PushDahan( count ) ).Matches( ctx=>ctx.Dahan.Any );
	static public SpaceAction PushUpToNExplorers( int count ) => new SpaceAction( $"Push up to {count} Explorers", ctx => ctx.PushUpTo(count,Invader.Explorer)).Matches( ctx=>ctx.Tokens.Has( Invader.Explorer ) );
	static public SpaceAction PushUpToNTowns( int count ) => new SpaceAction( $"Push up to {count} Towns", ctx=>ctx.PushUpTo(count,Invader.Town)).Matches( ctx=>ctx.Tokens.Has( Invader.Town ) );
	static public SpaceAction PushUpToNInvaders( int count, params TokenClass[] classes ) => new SpaceAction( $"Push up to {count} "+ classes.Select( c => c.Label ).Join( "/" ), ctx => ctx.PushUpTo( count, classes ) ).Matches( ctx => ctx.Tokens.HasAny( classes ) );
	static public SpaceAction PushExplorersOrTowns( int count ) => new SpaceAction( $"Push {count} explorers or towns", ctx => ctx.Push( count, Invader.Town, Invader.Explorer ) ).Matches( ctx=>ctx.Tokens.HasAny( Invader.Explorer, Invader.Town ) );

	// -- Add ---
	static public SpaceAction AddDahan( int count ) => new SpaceAction( count == 1 ? "Add 1 Dahan" : $"Add {count} Dahan", ctx => ctx.Tokens.Add( TokenType.Dahan.Default, count ) );
	static public SpaceAction AddTown( int count ) => new SpaceAction( count == 1 ? "Add 1 Town" : $"Add {count} Towns", ctx => ctx.Tokens.Add(Invader.Town.Default, count ) );
	static public SpaceAction AddCity( int count ) => new SpaceAction( count == 1 ? "Add 1 City" : $"Add {count} Cities", ctx => ctx.Tokens.Add(Invader.City.Default, count ) );
	static public SpaceAction AddBlightedIslandBlight => new SpaceAction("Add 1 blight", ctx => ctx.AddBlight(1,AddReason.SpecialRule) );
	static public SpaceAction AddWilds( int count ) => new SpaceAction( $"Add {count} Wilds.", ctx => ctx.Wilds.Add(count) );
	static public SpaceAction AddBadlands( int badLandCount ) => new SpaceAction( $"Add {badLandCount} badlands", ctx => ctx.Badlands.Add( badLandCount ) );
	static public SpaceAction AddStrife(int count) => count == 1
		? new SpaceAction( "Add 1 Strife.",  ctx => ctx.AddStrife() )
		: new SpaceAction( $"Add {count} Strife.",  async ctx => { for(int i=0;i<count;++i) await ctx.AddStrife(); } );

	// -- Remove --
	static public SpaceAction RemoveBlight => new SpaceAction("Remove 1 blight", ctx => ctx.Blight.Remove(1, RemoveReason.ReturnedToCard ));

	static public SpaceAction RemoveExplorers(int count) => RemoveUpToNTokens(count,Invader.Explorer);
	static public SpaceAction RemoveExplorersOrTowns(int count) => RemoveUpToNTokens(count,Invader.Explorer,Invader.Town);
	static public SpaceAction RemoveTowns(int count) => RemoveUpToNTokens(count,Invader.Explorer,Invader.Town);
	static public SpaceAction RemoveCities(int count) => RemoveUpToNTokens(count,Invader.Explorer,Invader.City);
	static public SpaceAction RemoveInvaders(int count) => RemoveUpToNTokens(count,Invader.Explorer,Invader.Explorer,Invader.Town,Invader.City);

	static public SpaceAction RemoveUpToNTokens(int count,params TokenClass[] tokenClasses) {
		Func<TokenClass,string> selector = count==1 ? t=>t.Label : t=>t.Label+"s";
		return new SpaceAction($"Remove {count} " + tokenClasses.Select( selector ).Join_WithLast(", "," or "),
			ctx => new TokenRemover(ctx).AddGroup(count, tokenClasses).RemoveUpToN()
		).Matches( x => x.Tokens.HasAny(tokenClasses));
	}

	static public SpaceAction RemoveHealthOfInvaders(string description, Func<TargetSpaceCtx,int> calcHealthToRemove) => new SpaceAction(description, async ctx=>{
		int remaining = calcHealthToRemove(ctx);
		Token pick;
		while(0 < remaining
			&& (pick = await ctx.Decision( Select.Invader.ToRemoveByHealth( ctx.Space, ctx.Tokens.Invaders(), remaining ) ) ) != null
		) {
			await ctx.Tokens.Remove( pick, 1 );
			remaining -= pick.Health;
		}
	} );

	static public SpaceAction RemoveUpToNHealthOfInvaders(int health) => RemoveHealthOfInvaders($"Remove up to {health} worth of invaders.",_=>health);


	// -- Destroy --
	static public SpaceAction DestroyBeast(int count) => new SpaceAction($"Destroy {count} Beast", ctx=>ctx.Beasts.Destroy(count)).Matches(x=>x.Beasts.Any);
	static public SpaceAction Defend1PerDahan => new SpaceAction("Defend 1 per Dahan.", ctx => ctx.Defend(ctx.Dahan.Count));
	static public SpaceAction Defend(int defend) => new SpaceAction( $"Defend {defend}.", ctx => ctx.Defend( defend ) );

	// -- Damage --
	static public SpaceAction DamageToTownOrExplorer(int damage) => new SpaceAction($"{damage} damage to Explorer or Town", ctx => ExplorerTownsTakeDamage(ctx,damage) );
	static Task ExplorerTownsTakeDamage(TargetSpaceCtx ctx, int damage) => ctx.DamageInvaders(damage,Invader.Explorer,Invader.Town);
	// -- Destroy --
	static public SpaceAction DestroyTown( int count ) => new SpaceAction($"Destroy {count} Towns", ctx=>ctx.Invaders.Destroy(count,Invader.Town)).Matches(x=>x.Tokens.Has(Invader.Town));

	// -- Fear --
	static public SpaceAction AddFear(int count) => new SpaceAction($"Add {count} Fear.", ctx => ctx.AddFear(count) );

	// AND / OR
	static public ActionOption<T> Multiple<T>( string title, params ActionOption<T>[] actions ) => new ActionOption<T>(
		title,
		async ctx => {
			foreach(var action in actions)
				await action.Execute( ctx );
		}
	);
	static public ActionOption<T> Multiple<T>( params ActionOption<T>[] actions) => new ActionOption<T>(
		actions.Select(a=>a.Description).Join("  "),
		async ctx => {
			foreach( var action in actions )
				await action.Execute(ctx);
		}
	);

	static public ActionOption<T> Pick1<T>( params IExecuteOn<T>[] actions ) where T : SelfCtx
		=> Pick1<T>(actions.Select(a=>a.Description).Join_WithLast(", ", " OR "), actions );

	static public ActionOption<T> Pick1<T>( string description, params IExecuteOn<T>[] actions ) where T : SelfCtx
		=> new ActionOption<T>(
			description,
			async ctx => {

				IExecuteOn<T>[] applicable = actions.Where( opt => opt != null && opt.IsApplicable(ctx) ).ToArray();
				string text = await ctx.Self.SelectText( "Select action", applicable.Select( a => a.Description ).ToArray(), Present.Always );
				if(text != null && text != TextOption.Done.Text) {
					var selectedOption = applicable.Single( a => a.Description == text );
					await selectedOption.Execute( ctx );
				}
			}
		);

}
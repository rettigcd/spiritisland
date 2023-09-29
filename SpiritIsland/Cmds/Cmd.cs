namespace SpiritIsland;

public static partial class Cmd {

	static public DecisionOption<T> Describe<T>( string description, Action<T> action) => new DecisionOption<T>(description, action);
	static public DecisionOption<T> Describe<T>( string description, Func<T,Task> func ) => new DecisionOption<T>( description, func );

	// Misc
	static public SpaceAction Isolate => new SpaceAction( $"Isolate target land.", ctx => ctx.Isolate() );

	// Gather
	static public SpaceAction GatherUpToNDahan( int count ) => new SpaceAction( $"Gather up to {count} Dahan", ctx => ctx.GatherUpToNDahan( count ) );
	static public SpaceAction GatherUpToNExplorers( int count ) => new SpaceAction( $"Gather up to {count} Explorers", ctx => ctx.GatherUpTo(count,Human.Explorer));
	static public SpaceAction GatherUpToNInvaders( int count, params IEntityClass[] classes ) => new SpaceAction( $"Gather up to {count} " + classes.Select(c=>c.Label).Join("/"), ctx => ctx.GatherUpTo( count, classes ) );

	// Push
	static public SpaceAction PushUpToNDahan( int count ) => new SpaceAction( $"Push up to {count} Dahan", ctx => ctx.PushUpToNDahan( count ) ).OnlyExecuteIf(ctx=>ctx.Dahan.Any );
	static public SpaceAction PushNDahan(int count ) => new SpaceAction( $"Push {count} dahan", ctx => ctx.PushDahan( count ) ).OnlyExecuteIf( ctx=>ctx.Dahan.Any );
	static public SpaceAction PushUpToNExplorers( int count ) => new SpaceAction( $"Push up to {count} Explorers", ctx => ctx.PushUpTo(count,Human.Explorer)).OnlyExecuteIf( ctx=>ctx.Tokens.Has( Human.Explorer ) );
	static public SpaceAction PushUpToNTowns( int count ) => new SpaceAction( $"Push up to {count} Towns", ctx=>ctx.PushUpTo(count,Human.Town)).OnlyExecuteIf( ctx=>ctx.Tokens.Has( Human.Town ) );
	static public SpaceAction PushUpToNInvaders( int count, params IEntityClass[] classes ) => new SpaceAction( $"Push up to {count} "+ classes.Select( c => c.Label ).Join( "/" ), ctx => ctx.PushUpTo( count, classes ) ).OnlyExecuteIf( ctx => ctx.Tokens.HasAny( classes ) );
	static public SpaceAction PushExplorersOrTowns( int count ) => new SpaceAction( $"Push {count} explorers or towns", ctx => ctx.Push( count, Human.Explorer_Town ) ).OnlyExecuteIf( ctx=>ctx.Tokens.HasAny( Human.Explorer_Town ) );

	// -- Add ---
	static public SpaceAction AddDahan( int count ) => new SpaceAction( count == 1 ? "Add 1 Dahan" : $"Add {count} Dahan", ctx => ctx.Tokens.AddDefault( Human.Dahan, count ) );
	static public SpaceAction AddTown( int count ) => new SpaceAction( count == 1 ? "Add 1 Town" : $"Add {count} Towns", ctx => ctx.Tokens.AddDefault(Human.Town, count ) );
	static public SpaceAction AddCity( int count ) => new SpaceAction( count == 1 ? "Add 1 City" : $"Add {count} Cities", ctx => ctx.Tokens.AddDefault(Human.City, count ) );
	static public SpaceAction AddBlightedIslandBlight => new SpaceAction("Add 1 blight", ctx => ctx.AddBlight(1,AddReason.SpecialRule) );
	static public SpaceAction AddWilds( int count ) => new SpaceAction( $"Add {count} Wilds.", ctx => ctx.Wilds.Add(count) );
	static public SpaceAction AddVitality( int count ) => new SpaceAction( $"Add {count} Vitality.", ctx => ctx.Vitality.Add( count ) );
	static public SpaceAction AddBadlands( int badLandCount ) => new SpaceAction( $"Add {badLandCount} badlands", ctx => ctx.Badlands.Add( badLandCount ) );
	static public SpaceAction AddStrife(int count) => new SpaceAction( $"Add {count} Strife.",  async ctx => { for(int i=0;i<count;++i) await ctx.AddStrife(); } );
	static public SpaceAction AddStrifeTo( int count, params HumanTokenClass[] tokenClasses ) => new SpaceAction( 
			$"Add {count} Strife to "+String.Join(",",tokenClasses.Select(x=>x.Label)), 
			async ctx => { for(int i = 0; i < count; ++i) await ctx.AddStrife( tokenClasses ); }
		); 
	static public SpaceAction Adjust1Token( string description, ISpaceEntity token ) => new SpaceAction( description, ctx => ctx.Tokens.Adjust(token,1) );
	// -- Screwy Strife Stuff --
	static public DecisionOption<GameCtx> StrifePenalizesHealth => new DecisionOption<GameCtx>( "Invaders reduce Health per strife", StrifedRavage.InvadersReduceHealthByStrifeCount );
	static public SpaceAction EachStrifeDamagesInvader => new SpaceAction( "Invaders take 1 damage per strife", async ctx=>{ 
		var tokens = ctx.Tokens.OfAnyHumanClass( Human.Invader ).Where( x => 0 < x.StrifeCount ).ToArray();
		foreach(var token in tokens) {
			int count = ctx.Tokens[token];
			while(0 < count--)
				await ctx.Invaders.ApplyDamageTo1(1,token);
		}
	} );
	// -- Remove --
	static public SpaceAction RemoveBlight => new SpaceAction("Remove 1 blight", ctx => ctx.Blight.Remove(1));

	static public SpaceAction RemoveExplorers(int count) => RemoveUpToNTokens(count,Human.Explorer);
	static public SpaceAction RemoveExplorersOrTowns(int count) => RemoveUpToNTokens(count,Human.Explorer_Town);
	static public SpaceAction RemoveTowns(int count) => RemoveUpToNTokens(count,Human.Explorer_Town);
	static public SpaceAction RemoveCities(int count) => RemoveUpToNTokens(count,Human.City);
	static public SpaceAction RemoveInvaders(int count) => RemoveUpToNTokens(count,Human.Invader);

	static public SpaceAction RemoveUpToNTokens(int count,params IEntityClass[] tokenClasses) {
		Func<IEntityClass,string> selector = count==1 ? t=>t.Label : t=>t.Label+"s";
		return new SpaceAction($"Remove {count} " + tokenClasses.Select( selector ).Join_WithLast(", "," or "),
			ctx => new TokenRemover(ctx).AddGroup(count, tokenClasses).RemoveUpToN()
		).OnlyExecuteIf( x => x.Tokens.HasAny(tokenClasses));
	}

	static public SpaceAction RemoveHealthOfInvaders(string description, Func<TargetSpaceCtx,int> calcHealthToRemove) => new SpaceAction(description, async ctx=>{
		int remaining = calcHealthToRemove(ctx);
		HumanToken pick;
		while(0 < remaining
			&& (pick = (await ctx.Decision( Select.Invader.ToRemoveByHealth( ctx.Space, ctx.Tokens.InvaderTokens(), remaining )) )?.Token.AsHuman()) != null
		) {
			await ctx.Remove( pick, 1 );
			remaining -= pick.RemainingHealth;
		}
	} ).OnlyExecuteIf( ctx => ctx.Tokens.HasInvaders() );

	static public SpaceAction RemoveUpToNHealthOfInvaders(int health) => RemoveHealthOfInvaders($"Remove up to {health} worth of invaders.",_=>health);


	// -- Destroy --
	static public SpaceAction DestroyBeast(int count) => new SpaceAction($"Destroy {count} Beast", ctx=>ctx.Beasts.Destroy(count)).OnlyExecuteIf(x=>x.Beasts.Any);
	static public SpaceAction Defend1PerDahan => new SpaceAction("Defend 1 per Dahan.", ctx => ctx.Defend(ctx.Dahan.CountAll));
	static public SpaceAction Defend(int defend) => new SpaceAction( $"Defend {defend}.", ctx => ctx.Defend( defend ) );

	// -- Damage --
	static public SpaceAction DamageToTownOrExplorer(int damage) => new SpaceAction($"{damage} damage to Explorer or Town", ctx => ExplorerTownsTakeDamage(ctx,damage) );
	static Task ExplorerTownsTakeDamage(TargetSpaceCtx ctx, int damage) => ctx.DamageInvaders(damage,Human.Explorer_Town);
	static public SpaceAction OneDamagePerDahan => new SpaceAction( "1 damage per dahan", ctx => ctx.DamageInvaders( ctx.Dahan.CountAll ) ).OnlyExecuteIf( x => x.Dahan.Any && x.HasInvaders );

	// -- Destroy --
	static public SpaceAction DestroyTown( int count ) => new SpaceAction($"Destroy {count} Towns", ctx=>ctx.Invaders.DestroyNOfClass(count,Human.Town)).OnlyExecuteIf(x=>x.Tokens.Has(Human.Town));

	// -- Fear --
	static public SpaceAction AddFear(int count) => new SpaceAction($"Add {count} Fear.", ctx => ctx.AddFear(count) );

	// AND / OR
	static public DecisionOption<T> Multiple<T>( string title, params IExecuteOn<T>[] actions ) => new DecisionOption<T>(
		title,
		async ctx => {
			foreach(var action in actions)
				await action.Execute( ctx );
		}
	);
	static public DecisionOption<T> Multiple<T>( params IExecuteOn<T>[] actions) => new DecisionOption<T>(
		actions.Select(a=>a.Description).Join("  "),
		async ctx => {
			foreach( var action in actions )
				await action.Execute(ctx);
		}
	);

	static public DecisionOption<T> Pick1<T>( params IExecuteOn<T>[] actions ) where T : SelfCtx
		=> Pick1<T>(actions.Select(a=>a.Description).Join_WithLast(", ", " OR "), actions );

	static public DecisionOption<T> Pick1<T>( string description, params IExecuteOn<T>[] actions ) where T : SelfCtx
		=> new DecisionOption<T>(
			description,
			async ctx => {

				IExecuteOn<T>[] applicable = actions.Where( opt => opt != null && opt.IsApplicable(ctx) ).ToArray();
				string text = await ctx.Self.SelectText( "Select action", applicable.Select( a => a.Description ).ToArray(), Present.AutoSelectSingle );
				if(text != null && text != TextOption.Done.Text) {
					var selectedOption = applicable.Single( a => a.Description == text );
					await selectedOption.Execute( ctx );
				}
			}
		);

	// Save Dahan
	static public SpaceAction NextTimeDestroy2FewerDahan => new SpaceAction( // !! needs tests
		"The next time dahan would be destroyed in target land, Destroy 2 fewer dahan.",
		DahanSaver.DestroyFewer( 2, 1 )
	);
	static public SpaceAction EachTimeDestroy2FewerDahan => new SpaceAction(
		"Each time dahan would be destroyed in target land, Destroy 2 fewer dahan.",
		DahanSaver.DestroyFewer( 2, int.MaxValue )
	);

	static public class Skip {
		static public SpaceAction Build( string name ) => new SpaceAction( "Stop the next Build", ctx => ctx.Tokens.Skip1Build( name ) );
		static public SpaceAction Explore( string name ) => new SpaceAction( "Skip the next Explore", ctx => ctx.Tokens.Skip1Explore( name ) );
		static public SpaceAction AllInvaderActions( string name ) => new SpaceAction( "Skip All Invader Actions", ctx => ctx.Tokens.SkipAllInvaderActions( name ) );
		static public SpaceAction AllRavages( string name ) => new SpaceAction( "Invaders do not ravage there this turn.", ctx => { ctx.Tokens.SkipRavage( name, UsageDuration.SkipAllThisTurn ); } );
	}


	// WTH are these doing in here?
	static public SelfAction DestroyPresence() => new SelfAction( "Destroy 1 presence.", ctx => ctx.Self.PickPresenceToDestroy() );
	static public SelfAction ForgetPowerCard => new SelfAction( "Forget Power card", ctx => ctx.Self.ForgetOne() );
	static public SelfAction DestroyPresence( int count ) => new SelfAction( 
		$"Destroy {count} presence", 
		async ctx => { for(int i = 0; i < count; ++i) await ctx.Self.PickPresenceToDestroy();}
	);

}

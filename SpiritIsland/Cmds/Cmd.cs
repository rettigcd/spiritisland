namespace SpiritIsland;

public static partial class Cmd {

	static public BaseCmd<T> Describe<T>( string description, Action<T> action) => new BaseCmd<T>(description, action);
	static public BaseCmd<T> Describe<T>( string description, Func<T,Task> func ) => new BaseCmd<T>( description, func );

	// Misc
	static public SpaceCmd Isolate => new SpaceCmd( $"Isolate target land.", ctx => ctx.Isolate() );

	// Gather
	static public SpaceCmd GatherUpToNDahan( int count ) => new SpaceCmd( $"Gather up to {count} Dahan", ctx => ctx.GatherUpToNDahan( count ) );
	static public SpaceCmd GatherUpToNExplorers( int count ) => new SpaceCmd( $"Gather up to {count} Explorers", ctx => ctx.GatherUpTo(count,Human.Explorer));
	static public SpaceCmd GatherUpToNInvaders( int count, params IEntityClass[] classes ) => new SpaceCmd( $"Gather up to {count} " + classes.Select(c=>c.Label).Join("/"), ctx => ctx.GatherUpTo( count, classes ) );

	// Push
	static public SpaceCmd PushUpToNDahan( int count ) => new SpaceCmd( $"Push up to {count} Dahan", ctx => ctx.PushUpToNDahan( count ) ).OnlyExecuteIf(ctx=>ctx.Dahan.Any );
	static public SpaceCmd PushNDahan(int count ) => new SpaceCmd( $"Push {count} dahan", ctx => ctx.PushDahan( count ) ).OnlyExecuteIf( ctx=>ctx.Dahan.Any );
	static public SpaceCmd PushUpToNExplorers( int count ) => new SpaceCmd( $"Push up to {count} Explorers", ctx => ctx.PushUpTo(count,Human.Explorer)).OnlyExecuteIf( ctx=>ctx.Tokens.Has( Human.Explorer ) );
	static public SpaceCmd PushUpToNTowns( int count ) => new SpaceCmd( $"Push up to {count} Towns", ctx=>ctx.PushUpTo(count,Human.Town)).OnlyExecuteIf( ctx=>ctx.Tokens.Has( Human.Town ) );
	static public SpaceCmd PushUpToNInvaders( int count, params IEntityClass[] classes ) => new SpaceCmd( $"Push up to {count} "+ classes.Select( c => c.Label ).Join( "/" ), ctx => ctx.PushUpTo( count, classes ) ).OnlyExecuteIf( ctx => ctx.Tokens.HasAny( classes ) );
	static public SpaceCmd PushExplorersOrTowns( int count ) => new SpaceCmd( $"Push {count} explorers or towns", ctx => ctx.Push( count, Human.Explorer_Town ) ).OnlyExecuteIf( ctx=>ctx.Tokens.HasAny( Human.Explorer_Town ) );

	// -- Add ---
	static public SpaceCmd AddDahan( int count ) => new SpaceCmd( count == 1 ? "Add 1 Dahan" : $"Add {count} Dahan", ctx => ctx.Tokens.AddDefault( Human.Dahan, count ) );
	static public SpaceCmd AddTown( int count ) => new SpaceCmd( count == 1 ? "Add 1 Town" : $"Add {count} Towns", ctx => ctx.Tokens.AddDefault(Human.Town, count ) );
	static public SpaceCmd AddCity( int count ) => new SpaceCmd( count == 1 ? "Add 1 City" : $"Add {count} Cities", ctx => ctx.Tokens.AddDefault(Human.City, count ) );
	static public SpaceCmd AddBlightedIslandBlight => new SpaceCmd("Add 1 blight", ctx => ctx.AddBlight(1,AddReason.SpecialRule) );
	static public SpaceCmd AddWilds( int count ) => new SpaceCmd( $"Add {count} Wilds.", ctx => ctx.Wilds.AddAsync(count) );
	static public SpaceCmd AddVitality( int count ) => new SpaceCmd( $"Add {count} Vitality.", ctx => ctx.Vitality.AddAsync( count ) );
	static public SpaceCmd AddBadlands( int badLandCount ) => new SpaceCmd( $"Add {badLandCount} badlands", ctx => ctx.Badlands.AddAsync( badLandCount ) );
	static public SpaceCmd AddStrife(int count) => new SpaceCmd( $"Add {count} Strife.",  async ctx => { for(int i=0;i<count;++i) await ctx.AddStrife(); } );
	static public SpaceCmd AddStrifeTo( int count, params HumanTokenClass[] tokenClasses ) => new SpaceCmd( 
			$"Add {count} Strife to "+String.Join(",",tokenClasses.Select(x=>x.Label)), 
			async ctx => { for(int i = 0; i < count; ++i) await ctx.AddStrife( tokenClasses ); }
		); 
	static public SpaceCmd Adjust1Token( string description, ISpaceEntity token ) => new SpaceCmd( description, ctx => ctx.Tokens.Adjust(token,1) );
	// -- Screwy Strife Stuff --
	static public BaseCmd<GameCtx> StrifePenalizesHealth => new BaseCmd<GameCtx>( "Invaders reduce Health per strife", StrifedRavage.InvadersReduceHealthByStrifeCount );
	static public SpaceCmd EachStrifeDamagesInvader => new SpaceCmd( "Invaders take 1 damage per strife", async ctx=>{ 
		var tokens = ctx.Tokens.OfAnyHumanClass( Human.Invader ).Where( x => 0 < x.StrifeCount ).ToArray();
		foreach(var token in tokens) {
			int count = ctx.Tokens[token];
			while(0 < count--)
				await ctx.Invaders.ApplyDamageTo1(1,token);
		}
	} );
	// -- Remove --
	static public SpaceCmd RemoveBlight => new SpaceCmd("Remove 1 blight", ctx => ctx.Blight.Remove(1));

	static public SpaceCmd RemoveExplorers(int count) => RemoveUpToNTokens(count,Human.Explorer);
	static public SpaceCmd RemoveExplorersOrTowns(int count) => RemoveUpToNTokens(count,Human.Explorer_Town);
	static public SpaceCmd RemoveTowns(int count) => RemoveUpToNTokens(count,Human.Explorer_Town);
	static public SpaceCmd RemoveCities(int count) => RemoveUpToNTokens(count,Human.City);
	static public SpaceCmd RemoveInvaders(int count) => RemoveUpToNTokens(count,Human.Invader);

	static public SpaceCmd RemoveUpToNTokens(int count,params IEntityClass[] tokenClasses) {
		Func<IEntityClass,string> selector = count==1 ? t=>t.Label : t=>t.Label+"s";
		return new SpaceCmd($"Remove {count} " + tokenClasses.Select( selector ).Join_WithLast(", "," or "),
			ctx => new TokenRemover(ctx).AddGroup(count, tokenClasses).RemoveUpToN()
		).OnlyExecuteIf( x => x.Tokens.HasAny(tokenClasses));
	}

	static public SpaceCmd RemoveHealthOfInvaders(string description, Func<TargetSpaceCtx,int> calcHealthToRemove) => new SpaceCmd(description, async ctx=>{
		int remaining = calcHealthToRemove(ctx);
		HumanToken pick;
		while(0 < remaining
			&& (pick = (await ctx.SelectAsync( An.Invader.ToRemoveByHealth( ctx.Space, ctx.Tokens.InvaderTokens(), remaining )) )?.Token.AsHuman()) != null
		) {
			await ctx.Remove( pick, 1 );
			remaining -= pick.RemainingHealth;
		}
	} ).OnlyExecuteIf( ctx => ctx.Tokens.HasInvaders() );

	static public SpaceCmd RemoveUpToNHealthOfInvaders(int health) => RemoveHealthOfInvaders($"Remove up to {health} worth of invaders.",_=>health);


	// -- Destroy --
	static public SpaceCmd DestroyBeast(int count) => new SpaceCmd($"Destroy {count} Beast", ctx=>ctx.Beasts.Destroy(count)).OnlyExecuteIf(x=>x.Beasts.Any);
	static public SpaceCmd Defend1PerDahan => new SpaceCmd("Defend 1 per Dahan.", ctx => ctx.Defend(ctx.Dahan.CountAll));
	static public SpaceCmd Defend(int defend) => new SpaceCmd( $"Defend {defend}.", ctx => ctx.Defend( defend ) );

	// -- Damage --
	static public SpaceCmd DamageToTownOrExplorer(int damage) => new SpaceCmd($"{damage} damage to Explorer or Town", ctx => ExplorerTownsTakeDamage(ctx,damage) );
	static Task ExplorerTownsTakeDamage(TargetSpaceCtx ctx, int damage) => ctx.DamageInvaders(damage,Human.Explorer_Town);
	static public SpaceCmd OneDamagePerDahan => new SpaceCmd( "1 damage per dahan", ctx => ctx.DamageInvaders( ctx.Dahan.CountAll ) ).OnlyExecuteIf( x => x.Dahan.Any && x.HasInvaders );

	static public SpaceCmd DamageInvaders( int damage, params HumanTokenClass[] tokenClasses ) => new SpaceCmd( 
		$"{damage} damage to Invaders", 
		ctx => ctx.DamageInvaders( damage, tokenClasses ?? Human.Invader )
	);


	// -- Destroy --
	static public SpaceCmd DestroyTown( int count ) => new SpaceCmd($"Destroy {count} Towns", ctx=>ctx.Invaders.DestroyNOfClass(count,Human.Town)).OnlyExecuteIf(x=>x.Tokens.Has(Human.Town));

	// -- Fear --
	static public SpaceCmd AddFear(int count) => new SpaceCmd($"Add {count} Fear.", ctx => ctx.AddFear(count) );

	// -- Skip Invader Actions ==
	static public SpaceCmd Skip1Ravage(string causeName) => new SpaceCmd(
		"Skip 1 Ravage", 
		ctx=>ctx.Tokens.SkipRavage(causeName,UsageDuration.SkipOneThisTurn)
	);

	static public SpaceCmd Skip1InvaderAction(string causeName) => new SpaceCmd(
		"Skip 1 Invader Action",
		ctx => ctx.Tokens.Adjust( new SkipAnyInvaderAction( causeName, ctx.Self ), 1 )
	);

	// AND / OR
	static public BaseCmd<T> Multiple<T>( string title, params IActOn<T>[] actions ) => new BaseCmd<T>(
		title,
		async ctx => {
			foreach(var action in actions)
				await action.ActAsync( ctx );
		}
	);
	static public BaseCmd<T> Multiple<T>( params IActOn<T>[] actions) => new BaseCmd<T>(
		actions.Select(a=>a.Description).Join("  "),
		async ctx => {
			foreach(IActOn<T> action in actions )
				await action.ActAsync(ctx);
		}
	);

	static public BaseCmd<T> Pick1<T>( params IActOn<T>[] actions ) where T : SelfCtx
		=> Pick1<T>(actions.Select(a=>a.Description).Join_WithLast(", ", " OR "), actions );

	static public BaseCmd<T> Pick1<T>( string description, params IActOn<T>[] actions ) where T : SelfCtx
		=> new BaseCmd<T>(
			description,
			async ctx => {

				IActOn<T>[] applicable = actions.Where( opt => opt != null && opt.IsApplicable(ctx) ).ToArray();
				string text = await ctx.Self.SelectText( "Select action", applicable.Select( a => a.Description ).ToArray(), Present.AutoSelectSingle );
				if(text != null && text != TextOption.Done.Text) {
					var selectedOption = applicable.Single( a => a.Description == text );
					await selectedOption.ActAsync( ctx );
				}
			}
		);

	// Save Dahan
	static public SpaceCmd NextTimeDestroy2FewerDahan => new SpaceCmd( // !! needs tests
		"The next time dahan would be destroyed in target land, Destroy 2 fewer dahan.",
		DahanSaver.DestroyFewer( 2, 1 )
	);
	static public SpaceCmd EachTimeDestroy2FewerDahan => new SpaceCmd(
		"Each time dahan would be destroyed in target land, Destroy 2 fewer dahan.",
		DahanSaver.DestroyFewer( 2, int.MaxValue )
	);

	static public class Skip {
		static public SpaceCmd Build( string name ) => new SpaceCmd( "Stop the next Build", ctx => ctx.Tokens.Skip1Build( name ) );
		static public SpaceCmd Explore( string name ) => new SpaceCmd( "Skip the next Explore", ctx => ctx.Tokens.Skip1Explore( name ) );
		static public SpaceCmd AllInvaderActions( string name ) => new SpaceCmd( "Skip All Invader Actions", ctx => ctx.Tokens.SkipAllInvaderActions( name ) );
		static public SpaceCmd AllRavages( string name ) => new SpaceCmd( "Invaders do not ravage there this turn.", ctx => { ctx.Tokens.SkipRavage( name, UsageDuration.SkipAllThisTurn ); } );
	}

	static public SpiritAction ForgetPowerCard => new SpiritAction( "Forget Power card", ctx => ctx.Self.ForgetACard() );

	// ========
	// Presence
	// ========

	static public SpiritAction PushUpTo1Presence( Func<Space, Space, Task> callback = null ) 
		=> new SpiritAction( "Push up to 1 Presence", async ctx => {

			// Select source
			var source = await ctx.Self.Select( new A.SpaceToken( "Select Presence to push.", ctx.Self.Presence.Movable, Present.Done ) );
			if(source == null) return;

			// Select destination
			Space destination = await ctx.Self.Select( A.Space.ToPushPresence( source.Space, source.Space.Tokens.Adjacent.Downgrade(), Present.Always, source.Token ) );
			await source.MoveTo( destination.Tokens );

			// Calback
			if(callback != null)
				await callback( source.Space, destination );
		});

	static public SpiritAction DestroyPresence( string prompt = "Select Presence to Destroy" ) => new SpiritAction( "Destroy 1 presence.", async ctx => {
		var spaceToken = await ctx.Self.Select( new A.SpaceToken( prompt, ctx.Self.Presence.Deployed, Present.Always ) );
		await spaceToken.Destroy();
	} );

	static public SpiritAction DestroyPresence( int count ) => new SpiritAction( 
		$"Destroy {count} presence", 
		async ctx => { 
			var destroyOne = Cmd.DestroyPresence();
			for(int i = 0; i < count; ++i) 
				await destroyOne.ActAsync(ctx);
		}
	);

	static public SpiritAction ReturnUpToNDestroyedToTrack( int count ) => new SpiritAction("Return up to N Destroyed Presence to Track", async ctx => {
		var self = ctx.Self;
		count = Math.Max( count, self.Presence.Destroyed );
		while(count > 0) {
			var dst = await self.Select( A.TrackSlot.ToCover( self ) );
			if(dst == null) break;
			await self.Presence.ReturnDestroyedToTrack( dst );
			--count;
		}
	});

	static public SpiritAction PlacePresenceWithin( int range ) => new PlacePresence(range);

	static public SpiritAction PlacePresenceOn( params SpaceState[] destinationOptions ) => new SpiritAction(
		"Place Presence",
		async ctx => {
			var self = ctx.Self;
			IOption from = await self.SelectSourcePresence();
			IToken token = from is SpaceToken sp ? sp.Token : self.Presence.Token; // We could expose this as the Default Token
			Space to = await self.Select( A.Space.ToPlacePresence( destinationOptions.Downgrade(), Present.Always, token ) );
			await self.Presence.Place( from, to );
		} );

	static public SpiritAction Reclaim1CardInsteadOfDiscarding => new SpiritAction( "Reclaims 1 card instead of discarding it", ctx => {
		GameState.Current.TimePasses_ThisRound.Push( new Reclaim1InsteadOfDiscard( ctx.Self ).Reclaim );
	} );

}

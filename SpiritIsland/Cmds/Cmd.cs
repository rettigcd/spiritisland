namespace SpiritIsland;

public static partial class Cmd {

	static public BaseCmd<T> Describe<T>( string description, Action<T> action) => new BaseCmd<T>(description, action);
	static public BaseCmd<T> Describe<T>( string description, Func<T,Task> func ) => new BaseCmd<T>( description, func );

	// Misc
	static public SpaceAction Isolate => new SpaceAction( $"Isolate target land.", ctx => ctx.Isolate() );

	// Gather
	static public SpaceAction GatherUpToNDahan( int count ) => new SpaceAction( $"Gather up to {count} Dahan", ctx => ctx.GatherUpToNDahan( count ) );
	static public SpaceAction GatherUpToNExplorers( int count ) => new SpaceAction( $"Gather up to {count} Explorers", ctx => ctx.GatherUpTo(count,Human.Explorer));
	static public SpaceAction GatherUpToNInvaders( int count, params ITokenClass[] classes ) => new SpaceAction( $"Gather up to {count} " + classes.Select(c=>c.Label).Join("/"), ctx => ctx.GatherUpTo( count, classes ) );

	// Push
	static public SpaceAction PushUpToNDahan( int count ) => new SpaceAction( $"Push up to {count} Dahan", ctx => ctx.PushUpToNDahan( count ) ).OnlyExecuteIf(ctx=>ctx.Dahan.Any );
	static public SpaceAction PushNDahan(int count ) => new SpaceAction( $"Push {count} dahan", ctx => ctx.PushDahan( count ) ).OnlyExecuteIf( ctx=>ctx.Dahan.Any );
	static public SpaceAction PushUpToNExplorers( int count ) => new SpaceAction( $"Push up to {count} Explorers", ctx => ctx.PushUpTo(count,Human.Explorer)).OnlyExecuteIf( ctx=>ctx.Space.Has( Human.Explorer ) );
	static public SpaceAction PushUpToNTowns( int count ) => new SpaceAction( $"Push up to {count} Towns", ctx=>ctx.PushUpTo(count,Human.Town)).OnlyExecuteIf( ctx=>ctx.Space.Has( Human.Town ) );
	static public SpaceAction PushUpToNInvaders( int count, params ITokenClass[] classes ) => new SpaceAction( $"Push up to {count} "+ classes.Select( c => c.Label ).Join( "/" ), ctx => ctx.PushUpTo( count, classes ) ).OnlyExecuteIf( ctx => ctx.Space.HasAny( classes ) );
	static public SpaceAction PushExplorersOrTowns( int count ) => new SpaceAction( $"Push {count} explorers or towns", ctx => ctx.Push( count, Human.Explorer_Town ) ).OnlyExecuteIf( ctx=>ctx.Space.HasAny( Human.Explorer_Town ) );

	// -- Add ---
	static public SpaceAction AddHuman( int count, HumanTokenClass tokenClass, string suffixDescription = "" )
		=> new SpaceAction( $"Add {tokenClass.ToCountString(count)}{suffixDescription}", ctx => ctx.Space.AddDefaultAsync(tokenClass, count ) );
	static public SpaceAction AddBlightedIslandBlight => new SpaceAction("Add 1 blight", ctx => ctx.AddBlight(1,AddReason.SpecialRule) );
	static public SpaceAction AddWilds( int count ) => new SpaceAction( $"Add {count} Wilds.", ctx => ctx.Wilds.AddAsync(count) );
	static public SpaceAction AddVitality( int count ) => new SpaceAction( $"Add {count} Vitality.", ctx => ctx.Vitality.AddAsync( count ) );
	static public SpaceAction AddBadlands( int badLandCount ) => new SpaceAction( $"Add {badLandCount} badlands", ctx => ctx.Badlands.AddAsync( badLandCount ) );
	static public SpaceAction AddStrife(int count) => new SpaceAction( $"Add {count} Strife.",  ctx => ctx.AddStrife(count) );
	static public SpaceAction AddStrifeTo( int count, params HumanTokenClass[] tokenClasses ) => new SpaceAction( 
			$"Add {count} Strife to "+String.Join(",",tokenClasses.Select(x=>x.Label)), 
			ctx => ctx.AddStrife( count, tokenClasses )
		); 
	static public SpaceAction Adjust1Token( string description, ISpaceEntity token ) => new SpaceAction( description, ctx => ctx.Space.Adjust(token,1) );
	// -- Screwy Strife Stuff --
	static public BaseCmd<GameState> StrifePenalizesHealth => new BaseCmd<GameState>( "Invaders reduce Health per strife", ReduceHealthByStrife.Init );
	static public SpaceAction EachStrifeDamagesInvader => new SpaceAction( "Invaders take 1 damage per strife", async ctx=>{ 
		var tokens = ctx.Space.HumanOfAnyTag( Human.Invader ).Where( x => 0 < x.StrifeCount ).ToArray();
		foreach(var token in tokens) {
			int count = ctx.Space[token];
			while(0 < count--)
				await ctx.Invaders.ApplyDamageTo1(1,token);
		}
	} );
	// -- Remove --
	static public SpaceAction RemoveBlight => new SpaceAction("Remove 1 blight", ctx => ctx.Blight.Remove(1));

	// Remove *UpTo*
	static public SpaceAction RemoveExplorers(int count) => RemoveUpToNTokens(count,Human.Explorer);
	static public SpaceAction RemoveExplorersOrTowns(int count) => RemoveUpToNTokens(count,Human.Explorer_Town);
	static public SpaceAction RemoveTowns(int count) => RemoveUpToNTokens(count,Human.Town);
	static public SpaceAction RemoveCities(int count) => RemoveUpToNTokens(count,Human.City);
	static public SpaceAction RemoveInvaders(int count) => RemoveUpToNTokens(count,Human.Invader);

	static public SpaceAction RemoveUpToNTokens(int count,params ITokenClass[] tokenClasses) {
		Func<ITokenClass,string> selector = count==1 ? t=>t.Label : t=>t.Label+"s";
		return new SpaceAction($"Remove {count} " + tokenClasses.Select( selector ).Join_WithLast(", "," or "),
			ctx => ctx.SourceSelector.AddGroup(count, tokenClasses).RemoveUpToN(ctx.Self)
		).OnlyExecuteIf( x => x.Space.HasAny(tokenClasses));
	}
	static public SpaceAction RemoveNTokens( int count, params ITokenClass[] tokenClasses ) {
		Func<ITokenClass, string> selector = count == 1 ? t => t.Label : t => t.Label + "s";
		return new SpaceAction( $"Remove {count} " + tokenClasses.Select( selector ).Join_WithLast( ", ", " or " ),
			ctx => ctx.SourceSelector.AddGroup( count, tokenClasses ).RemoveN(ctx.Self)
		).OnlyExecuteIf( x => x.Space.HasAny( tokenClasses ) );
	}

	// Removes "Up To"
	static public SpaceAction RemoveUpToNHealthOfInvaders(int health) => RemoveUpToVariableHealthOfInvaders($"Remove up to {health} worth of Invaders",_=>health);

	// Removes "Up To"
	static public SpaceAction RemoveUpToVariableHealthOfInvaders(string description, Func<TargetSpaceCtx,int> calcMaxHealthToRemove) => new SpaceAction(description, async ctx=>{
		int remaining = calcMaxHealthToRemove(ctx);
		HumanToken pick;
		while(0 < remaining
			&& (pick = (await ctx.SelectAsync( An.Invader.ToRemoveByHealth( ctx.Space.InvaderTokens().On(ctx.Space), remaining )) )?.Token.AsHuman()) != null
		) {
			await ctx.Remove( pick, 1 );
			remaining -= pick.RemainingHealth;
		}
	} ).OnlyExecuteIf( ctx => ctx.Space.HasInvaders() );


	// -- Destroy --
	static public SpaceAction DestroyBeast(int count) => new SpaceAction($"Destroy {count} Beast", ctx=>ctx.Beasts.Destroy(count)).OnlyExecuteIf(x=>x.Beasts.Any);
	static public SpaceAction Defend1PerDahan => new SpaceAction("Defend 1 per Dahan.", ctx => ctx.Defend(ctx.Dahan.CountAll));
	static public SpaceAction Defend(int defend) => new SpaceAction( $"Defend {defend}.", ctx => ctx.Defend( defend ) );

	// -- Damage --
	static public SpaceAction DamageToTownOrExplorer(int damage) => new SpaceAction($"{damage} damage to Explorer or Town", ctx => ExplorerTownsTakeDamage(ctx,damage) );
	static Task ExplorerTownsTakeDamage(TargetSpaceCtx ctx, int damage) => ctx.DamageInvaders(damage,Human.Explorer_Town);
	static public SpaceAction OneDamagePerDahan => new SpaceAction( "1 damage per dahan", ctx => ctx.DamageInvaders( ctx.Dahan.CountAll ) ).OnlyExecuteIf( x => x.Dahan.Any && x.HasInvaders );

	static public SpaceAction DamageInvaders( int damage, params HumanTokenClass[] tokenClasses ) => new SpaceAction( 
		$"{damage} damage to Invaders", 
		ctx => ctx.DamageInvaders( damage, tokenClasses ?? Human.Invader )
	);


	// -- Destroy --
	static public SpaceAction DestroyTown( int count ) => new SpaceAction($"Destroy {count} Towns", ctx=>ctx.Invaders.DestroyNOfClass(count,Human.Town)).OnlyExecuteIf(x=>x.Space.Has(Human.Town));

	// -- Fear --
	static public SpaceAction AddFear(int count) => new SpaceAction($"Add {count} Fear.", ctx => ctx.AddFear(count) );

	// -- Skip Invader Actions ==
	static public SpaceAction Skip1Ravage(string causeName) => new SpaceAction(
		"Skip 1 Ravage", 
		ctx=>ctx.Space.SkipRavage(causeName,UsageDuration.SkipOneThisTurn)
	);

	static public SpaceAction SkipAllBuilds(string causeName) => new SpaceAction("Skip All Build", ctx=>ctx.Space.SkipAllBuilds(causeName) );
	static public SpaceAction Skip1Build(string causeName) => new SpaceAction("Skip 1 Build", ctx=>ctx.Space.Skip1Build(causeName) );


	static public SpaceAction Skip1InvaderAction(string causeName) => new SpaceAction(
		"Skip 1 Invader Action",
		ctx => ctx.Space.Skip1InvaderAction( causeName, ctx.Self )
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

	static public BaseCmd<T> Pick1<T>( params IActOn<T>[] actions ) where T : IHaveASpirit
		=> Pick1<T>(actions.Select(a=>a.Description).Join_WithLast(", ", " OR "), actions );

	static public BaseCmd<T> Pick1<T>( string description, params IActOn<T>[] actions ) where T : IHaveASpirit
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
	static public SpaceAction NextTimeDestroy2FewerDahan => new SpaceAction( // !! needs tests
		"The next time dahan would be destroyed in target land, Destroy 2 fewer dahan.",
		SaveDahan.DestroyFewer( 2, 1 )
	);
	static public SpaceAction EachTimeDestroy2FewerDahan => new SpaceAction(
		"Each time dahan would be destroyed in target land, Destroy 2 fewer dahan.",
		SaveDahan.DestroyFewer( 2, int.MaxValue )
	);

	static public class Skip {
		static public SpaceAction Build( string name ) => new SpaceAction( "Stop the next Build", ctx => ctx.Space.Skip1Build( name ) );
		static public SpaceAction Explore( string name ) => new SpaceAction( "Skip the next Explore", ctx => ctx.Space.Skip1Explore( name ) );
		static public SpaceAction AllInvaderActions( string name ) => new SpaceAction( "Skip All Invader Actions", ctx => ctx.Space.SkipAllInvaderActions( name ) );
		static public SpaceAction AllRavages( string name ) => new SpaceAction( "Stop Invaders Ravage this Turn", ctx => { ctx.Space.SkipRavage( name, UsageDuration.SkipAllThisTurn ); } );
	}

	static public SpiritAction ForgetPowerCard => new SpiritAction( "Forget Power card", spirit => spirit.ForgetACard() );

	// ========
	// Presence
	// ========

	static public SpiritAction PushUpTo1Presence( Func<Space, Space, Task> callback = null ) 
		=> new SpiritAction( "Push up to 1 Presence", async self => {

			// Select source
			var source = await self.SelectAsync( new A.SpaceTokenDecision( "Select Presence to push.", self.Presence.Movable, Present.Done ) );
			if(source == null) return;

			// Select destination
			Space destination = await self.SelectAsync( A.SpaceDecision.ToPushPresence( source.Space, source.Space.Adjacent, Present.Always, source.Token ) );
			await source.MoveTo( destination );

			// Calback
			if(callback != null)
				await callback( source.Space, destination );
		});

	static public SpiritAction DestroyPresence( string prompt = "Select Presence to Destroy" ) 
		=> new SpiritAction( "Destroy 1 presence.", async self => {
				SpaceToken spaceToken = await self.SelectAsync( A.SpaceTokenDecision.OfDeployedPresence( prompt, self ) );
				await spaceToken.Destroy();
			}
		);

	static public SpiritAction DestroyPresence( int count ) => new SpiritAction( 
		$"Destroy {count} presence", 
		async self => { 
			var destroyOne = Cmd.DestroyPresence();
			for(int i = 0; i < count; ++i) 
				await destroyOne.ActAsync(self);
		}
	);

	//static public IActOn<T> Repeat<T>( this IActOn<T> action, int count ) => new BaseCmd<T>(
	//	$"{action.Description} x{count}",
	//	async t => { for(int i=0;i<count;++i) await action.ActAsync(t); }
	//);

	static public SpiritAction ReturnUpToNDestroyedToTrack( int count ) => new SpiritAction("Return up to N Destroyed Presence to Track", async self => {
		count = Math.Max( count, self.Presence.Destroyed.Count );
		while(count > 0) {
			var dst = await self.SelectAsync( A.TrackSlot.ToCover( self ) );
			if(dst == null) break;
			await self.Presence.Destroyed.MoveToAsync( dst );
			--count;
		}
	});

	static public SpiritAction PlacePresenceWithin( int range ) => new PlacePresence(range);

	static public SpiritAction PlacePresenceOn( params Space[] destinationOptions ) => new SpiritAction(
		"Place Presence",
		async self => {
			TokenLocation from = await self.SelectSourcePresence();
			Space to = await self.SelectAsync( A.SpaceDecision.ToPlacePresence( destinationOptions, Present.Always, from.Token ) );
			await from.MoveToAsync(to);
		} );

	static public SpiritAction Reclaim1CardInsteadOfDiscarding => new SpiritAction( "Reclaims 1 card instead of discarding it", self => {
		GameState.Current.AddTimePassesAction( new Reclaim1InsteadOfDiscard( self ) );
	} );


	// not a command but I can't find anywhere to put it.
	static public async Task PayPresenceForBargain( this Spirit self, string takeFromTrackElementThreshold ) {
		if(await self.YouHave( takeFromTrackElementThreshold )) {
			var presenceToRemove = await self.SelectSourcePresence( Present.Always, "remove from game" ); // Come from track or board
			await presenceToRemove.RemoveAsync( 1 ); // await self.Presence.TakeFromAsync( presenceToRemove );
		} else {
			SpaceToken presenceToRemove = await self.SelectAsync( new A.SpaceTokenDecision( "Select presence to remove from game.", self.Presence.Deployed, Present.Always ) );
			await presenceToRemove.Remove();
		}
	}

}

using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public static class Cmd {

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

		// Push
		static public SpaceAction PushUpToNDahan( int count ) => new SpaceAction( $"Push up to {count} Dahan", ctx => ctx.PushUpToNDahan( count ) ).Cond(ctx=>ctx.Dahan.Any );
		static public SpaceAction PushNDahan(int count ) => new SpaceAction( $"Push {count} dahan", ctx => ctx.PushDahan( count ) ).Cond( ctx=>ctx.Dahan.Any );
		static public SpaceAction PushUpToNExplorers( int count ) => new SpaceAction( $"Push up to {count} Explorers", ctx => ctx.PushUpTo(count,Invader.Explorer));
		static public SpaceAction PushUpToNTowns( int count ) => new SpaceAction( $"Push up to {count} Towns", ctx=>ctx.PushUpTo(count,Invader.Town));
		static public SpaceAction PushExplorersOrTowns( int count ) => new SpaceAction( $"Push {count} explorers or towns", ctx => ctx.Push( count, Invader.Town, Invader.Explorer ) ).Cond( ctx=>ctx.Tokens.HasAny( Invader.Explorer, Invader.Town ) );

		// - Add / Remove Tokens Counts -
		static public SpaceAction RemoveBlight => new SpaceAction("Remove 1 blight", ctx => ctx.RemoveBlight() );

		static public SpaceAction AddWilds( int count ) => new SpaceAction($"Add {count} Wilds.", ctx => ctx.Wilds.Add(count) );

		static public SpaceAction AddBadlands( int badLandCount ) => new SpaceAction( $"Add {badLandCount} badlands", ctx => ctx.Badlands.Add( badLandCount ) );

		static public SpaceAction AddStrife(int count) => count == 1
			? new SpaceAction( "Add 1 Strife.",  ctx => ctx.AddStrife() )
			: new SpaceAction( $"Add {count} Strife.",  async ctx => { for(int i=0;i<count;++i) await ctx.AddStrife(); } );
		static public SpaceAction Defend1PerDahan => new SpaceAction("Defend 1 per Dahan.", ctx => ctx.Defend(ctx.Dahan.Count));
		static public SpaceAction RemoveExplorers(int count) => new SpaceAction($"Remove {count} Explorers", ctx=>ctx.RemoveUpTo(count,Invader.Explorer));
		static public SpaceAction RemoveExplorersOrTowns(int count) => new SpaceAction($"Remove {count} Explorers/Towns", ctx=>ctx.RemoveUpTo(count,Invader.Explorer,Invader.Town));
		static public SpaceAction RemoveCities(int count) => new SpaceAction($"Remove {count} Cities", ctx=>ctx.RemoveUpTo(count,Invader.City));
		static public SpaceAction RemoveInvaders(int count) => new SpaceAction($"Remove {count} Invaders", ctx=>ctx.RemoveUpTo(count,Invader.Explorer,Invader.Town,Invader.City));

		// -- Damage --
		static public SpaceAction DamageToTownOrExplorer(int damage) => new SpaceAction($"{damage} damage to Explorer or Town", ctx => ExplorerTownsTakeDamage(ctx,damage) );
		static Task ExplorerTownsTakeDamage(TargetSpaceCtx ctx, int damage) => ctx.DamageInvaders(damage,Invader.Explorer,Invader.Town);
		// -- Destory --

		static public SpaceAction DestoryTown( int count ) => new SpaceAction($"Destroy {count} Towns", ctx=>ctx.Invaders.Destroy(count,Invader.Town));


		// -- Fear --
		static public SpaceAction AddFear(int count) => new SpaceAction($"Add {count} Fear.", ctx => ctx.AddFear(count) );

		static public ActionOption<BoardCtx> OnBoardPickSpaceThenTakeAction(string description, SpaceAction spaceAction,Func<Space,bool> spaceFilter)
			=> new ActionOption<BoardCtx>(description, async ctx => {
				var spaceOptions = ctx.Board.Spaces.Where( spaceFilter ).ToArray();
				if(spaceOptions.Length == 0 ) return;

				var spaceCtx = await ctx.SelectSpace("Select space to " + spaceAction.Description, spaceOptions);
				await spaceAction.Execute(spaceCtx);
			});

	}

}

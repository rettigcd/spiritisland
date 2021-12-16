using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	static class Cmd {

		static public SpaceAction Destroy2FewerDahan = new SpaceAction(
			"Each time Dahan would be Destroyed in target land, Destroy 2 fewer Dahan.", 
			// !!! This only stops Ravage destroys, not other. (Are there any other?)
			ctx=> ctx.GameState.ModifyRavage(ctx.Space, cfg => { 
				var oldDestroy = cfg.DestroyDahan;
				cfg.DestroyDahan = (dahan,count,health) => oldDestroy(dahan,count-2,health);
			} ) 
		);

		// Push / Pull
		static public SpaceAction GatherUpToNDahan( int count ) => new SpaceAction( $"Gather up to {count} Dahan", ctx => ctx.GatherUpToNDahan( count ) );
		static public SpaceAction PushUpToNDahan( int count ) => new SpaceAction( $"Gather up to {count} Dahan", ctx => ctx.PushUpToNDahan( count ) );
		static public SpaceAction GatherUpToNExplorers( int count ) => new SpaceAction( $"Gather up to {count} Explorers", ctx => ctx.GatherUpTo(count,Invader.Explorer));
		static public SpaceAction PushUpToNExplorers( int count ) => new SpaceAction( $"Push up to {count} Explorers", ctx => ctx.PushUpTo(count,Invader.Explorer));
		static public SpaceAction PushUpToNTowns( int count ) => new SpaceAction( $"Push up to {count} Towns", ctx=>ctx.PushUpTo(count,Invader.Town));

		// - Adjust Tokens Counts -
		static public SpaceAction Add1Wilds => new SpaceAction("Add 1 Wilds.", ctx => ctx.Wilds.Add(1) );
		static public SpaceAction Add1Badlands => new SpaceAction("Add 1 Badland.", ctx => ctx.Badlands.Add(1) );
		static public SpaceAction Add1Strife => new SpaceAction("Add 1 Strife.", ctx => ctx.AddStrife() );
		static public SpaceAction Defend1PerDahan => new SpaceAction("Defend 1 per Dahan.", ctx => ctx.Defend(ctx.Dahan.Count));

		// -- Damage --
		static public SpaceAction DamageToTownOrExplorer(int damage) => new SpaceAction($"{damage} damage to Explorer or Town", ctx => ExplorerTownsTakeDamage(ctx,damage) );
		static Task ExplorerTownsTakeDamage(TargetSpaceCtx ctx, int damage) => ctx.DamageInvaders(damage,Invader.Explorer,Invader.Town);
		// -- Destory --

		static public SpaceAction DestoryTown( int count ) => new SpaceAction($"Destroy {count} Towns", ctx=>ctx.Invaders.Destroy(count,Invader.Town));


		// -- Fear --
		static public SpaceAction AddFear(int count) => new SpaceAction($"Add {count} Fear.", ctx => ctx.AddFear(count) );
	}

}

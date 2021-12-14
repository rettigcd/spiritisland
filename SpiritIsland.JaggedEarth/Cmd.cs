using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	static class Cmd {

		static public ActionOption Destroy2FewerDahan = new ActionOption(
			"Each time Dahan would be Destroyed in target land, Destroy 2 fewer Dahan.", 
			// !!! This only stops Ravage destroys, not other. (Are there any other?)
			ctx=> ctx.GameState.ModifyRavage(ctx.Space, cfg => { 
				var oldDestroy = cfg.DestroyDahan;
				cfg.DestroyDahan = (dahan,count,health) => oldDestroy(dahan,count-2,health);
			} ) 
		);

		// Push / Pull
		static public ActionOption GatherUpToNDahan( int count ) => new ActionOption( $"Gather up to {count} Dahan", ctx => ctx.GatherUpToNDahan( count ) );
		static public ActionOption PushUpToNDahan( int count ) => new ActionOption( $"Gather up to {count} Dahan", ctx => ctx.PushUpToNDahan( count ) );
		static public ActionOption GatherUpToNExplorers( int count ) => new ActionOption( $"Gather up to {count} Explorers", ctx => ctx.GatherUpTo(count,Invader.Explorer));
		static public ActionOption PushUpToNExplorers( int count ) => new ActionOption( $"Push up to {count} Explorers", ctx => ctx.PushUpTo(count,Invader.Explorer));

		// - Adjust Tokens Counts -
		static public ActionOption Add1Wilds => new ActionOption("Add 1 Wilds.", ctx => ctx.Wilds.Add(1) );
		static public ActionOption Add1Badlands => new ActionOption("Add 1 Badland.", ctx => ctx.Badlands.Add(1) );
		static public ActionOption Add1Strife => new ActionOption("Add 1 Strife.", ctx => ctx.AddStrife() );
		static public ActionOption Defend1PerDahan => new ActionOption("Defend 1 per Dahan.", ctx => ctx.Defend(ctx.Dahan.Count));

		// -- Damage --
		static public ActionOption DamageToTownOrExplorer(int damage) => new ActionOption($"{damage} damage to Explorer or Town", ctx => ExplorerTownsTakeDamage(ctx,damage) );
		static Task ExplorerTownsTakeDamage(TargetSpaceCtx ctx, int damage) => ctx.DamageInvaders(damage,Invader.Explorer,Invader.Town);

		// -- Fear --
		static public ActionOption AddFear(int count) => new ActionOption($"Add {count} Fear.", ctx => ctx.AddFear(count) );
	}

}

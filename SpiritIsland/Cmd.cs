using System.Collections.Generic;
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

		// -- Add ---
		static public SpaceAction AddDahan( int count ) => new SpaceAction( $"Add {count} Dahan", ctx => ctx.Tokens.Add( TokenType.Dahan.Default, count ) );
		static public SpaceAction AddTown( int count ) => new SpaceAction( $"Add {count} Towns", ctx => ctx.Tokens.Add(Invader.Town.Default, count ) );
		static public SpaceAction AddCity( int count ) => new SpaceAction( $"Add {count} Cities", ctx => ctx.Tokens.Add(Invader.City.Default, count ) );
		static public SpaceAction AddBlight => new SpaceAction("Add 1 blight", ctx => ctx.AddBlight() );
		static public SpaceAction AddWilds( int count ) => new SpaceAction($"Add {count} Wilds.", ctx => ctx.Wilds.Add(count) );
		static public SpaceAction AddBadlands( int badLandCount ) => new SpaceAction( $"Add {badLandCount} badlands", ctx => ctx.Badlands.Add( badLandCount ) );
		static public SpaceAction AddStrife(int count) => count == 1
			? new SpaceAction( "Add 1 Strife.",  ctx => ctx.AddStrife() )
			: new SpaceAction( $"Add {count} Strife.",  async ctx => { for(int i=0;i<count;++i) await ctx.AddStrife(); } );

		// -- Remove --
		static public SpaceAction RemoveBlight => new SpaceAction("Remove 1 blight", ctx => ctx.RemoveBlight() );
		static public SpaceAction RemoveExplorers(int count) => new SpaceAction($"Remove {count} Explorers", ctx=>ctx.RemoveUpTo(count,Invader.Explorer));
		static public SpaceAction RemoveExplorersOrTowns(int count) => new SpaceAction($"Remove {count} Explorers/Towns", ctx=>ctx.RemoveUpTo(count,Invader.Explorer,Invader.Town));
		static public SpaceAction RemoveTowns(int count) => new SpaceAction($"Remove {count} Towns", ctx=>ctx.RemoveUpTo(count,Invader.Town));
		static public SpaceAction RemoveCities(int count) => new SpaceAction($"Remove {count} Cities", ctx=>ctx.RemoveUpTo(count,Invader.City));
		static public SpaceAction RemoveInvaders(int count) => new SpaceAction($"Remove {count} Invaders", ctx=>ctx.RemoveUpTo(count,Invader.Explorer,Invader.Town,Invader.City));

		// -- Destroy --
		static public SpaceAction DestoryBeast(int count) => new SpaceAction($"Destory {count} Beast", ctx=>ctx.Beasts.Destroy(count));

		static public SpaceAction Defend1PerDahan => new SpaceAction("Defend 1 per Dahan.", ctx => ctx.Defend(ctx.Dahan.Count));
		static public SpaceAction Defend(int defend) => new SpaceAction( $"Defend {defend}.", ctx => ctx.Defend( defend ) );

		// -- Damage --
		static public SpaceAction DamageToTownOrExplorer(int damage) => new SpaceAction($"{damage} damage to Explorer or Town", ctx => ExplorerTownsTakeDamage(ctx,damage) );
		static Task ExplorerTownsTakeDamage(TargetSpaceCtx ctx, int damage) => ctx.DamageInvaders(damage,Invader.Explorer,Invader.Town);
		// -- Destory --

		static public SpaceAction DestoryTown( int count ) => new SpaceAction($"Destroy {count} Towns", ctx=>ctx.Invaders.Destroy(count,Invader.Town));


		// -- Fear --
		static public SpaceAction AddFear(int count) => new SpaceAction($"Add {count} Fear.", ctx => ctx.AddFear(count) );

		// AND / OR
		static public ActionOption<T> Multiple<T>( params ActionOption<T>[] actions) => new ActionOption<T>(
			actions.Select(a=>a.Description).Join("  "),
			async ctx => {
				foreach( var action in actions )
					await action.Execute(ctx);
			}
		);

		static public ActionOption<T> Pick1<T>( params IExecuteOn<T>[] actions ) where T : SelfCtx
			=> new ActionOption<T>(
				actions.Select(a=>a.Description).Join_WithLast(", ", " OR "),
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

	// Mix ins we could put in a different namespace that we only get when included
	static public class GameStateExtensions {
		static public IEnumerable<SelfCtx> SpiritCtxs(this GameState gs, Cause cause) 
			=> gs.Spirits.Select(s=>new SelfCtx(s,gs,cause));
	}

}

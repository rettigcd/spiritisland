using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	[InnatePower(MassiveFlooding.Name,Speed.Slow)]
	[FromSacredSite(1,Target.TownOrExplorer)]
	public class MassiveFlooding {

		public const string Name = "Massive Flooding";

		[InnateOption(Element.Sun,Element.Water,Element.Water)]
		static public Task Option1Async(TargetSpaceCtx ctx){
			// Push 1 Town/Explorer
			return ctx.PushUpToNInvaders(ctx.Target,1,Invader.Town,Invader.Explorer); 
		}

		[InnateOption(Element.Sun,Element.Sun,Element.Water,Element.Water,Element.Water)]
		static public async Task Option2Async(TargetSpaceCtx ctx){
			await ctx.DamageInvaders(ctx.Target, 2);
			await ctx.PushUpToNInvaders(ctx.Target,3,Invader.Town,Invader.Explorer);
		}

		[InnateOption(Element.Sun,Element.Sun,Element.Sun,Element.Water,Element.Water,Element.Water,Element.Water,Element.Earth)]
		static public async Task Option3Async(TargetSpaceCtx ctx){
			var group = ctx.InvadersOn(ctx.Target);

			var invaderTypes = group.InvaderTypesPresent_Specific.ToDictionary(x=>x,x=>group[x]); // copy so we can modify
			foreach(var (invader,origCount) in invaderTypes.Select(x=>(x.Key,x.Value))){
				for(int i=0;i<origCount;++i)
					await group.ApplyDamageTo1( 2, invader );
			}
		}

	}

}

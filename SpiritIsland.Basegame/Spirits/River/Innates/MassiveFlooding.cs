using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	[InnatePower(MassiveFlooding.Name,Speed.Slow)]
	[FromSacredSite(1,Target.Invaders)]
	public class MassiveFlooding {

		public const string Name = "Massive Flooding";

		[InnateOption("1 sun,2 water")]
		static public Task Option1Async(TargetSpaceCtx ctx){
			// Push 1 Town/Explorer
			return ctx.PushUpToNTokens(1,Invader.Town,Invader.Explorer); 
		}

		[InnateOption("2 sun,3 water")]
		static public async Task Option2Async(TargetSpaceCtx ctx){
			await ctx.DamageInvaders(ctx.Target, 2);
			await ctx.PushUpToNTokens(3,Invader.Town,Invader.Explorer);
		}

		[InnateOption("3 sun, 4 water,1 earth")]
		static public async Task Option3Async(TargetSpaceCtx ctx){
			var group = ctx.PowerInvaders;

			// copy so we can modify
			var invaderTypes = group.Counts.Invaders().ToDictionary(x=>x,x=>group[x]); 

			foreach(var (invader,origCount) in invaderTypes.Select(x=>(x.Key,x.Value)))
				for(int i=0;i<origCount;++i)
					await group.ApplyDamageTo1( 2, invader );
			
		}

	}

}

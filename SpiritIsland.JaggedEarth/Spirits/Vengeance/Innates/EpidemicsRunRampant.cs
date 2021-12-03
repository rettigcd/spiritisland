using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	[InnatePower("Epidemics Run Rampant"), Fast, FromPresence(1,Target.Disease)]
	public class EpidemicsRunRampant {

		// !! add a sub-text desrcription option that dispays
		// "This Power's Damage is dealth (separately) to both Invaders and dahan."

		[InnateOption("1 fire,3 animal","1 Damage per disease.")]
		static public Task Damage1( TargetSpaceCtx ctx ) {
			return DiseaseDamagesInvaders( ctx, 1 );
		}

		[InnateOption("1 water,2 fire,4 animal","+1 Damage per disease.")]
		static public Task Damage2( TargetSpaceCtx ctx ) {
			return DiseaseDamagesInvaders( ctx, 2 );
		}

		[InnateOption("3 water,3 fire,5 animal","+1 Damage per disease. 1 fear per disease (max 5). Remove 1 disease.")]
		static public async Task Damage3( TargetSpaceCtx ctx ) {
			await DiseaseDamagesInvaders( ctx, 2 );
			ctx.AddFear( System.Math.Min(5,ctx.Disease.Count) );
			ctx.Disease.Remove(1); // !!! How are we supposed to remove tokens so shifting-memory can detect them for prepare-element???
		}
	
		static Task DiseaseDamagesInvaders( TargetSpaceCtx ctx, int damagePerDisease=1 ) {
			return ctx.DamageInvaders( ctx.Disease.Count * damagePerDisease );
		}


	}

}

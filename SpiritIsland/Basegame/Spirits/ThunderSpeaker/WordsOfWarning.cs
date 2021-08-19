using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class WordsOfWarning {

		public const string Name = "Words of Warning";

		[SpiritCard( WordsOfWarning.Name, 1, Speed.Fast, Element.Air, Element.Sun, Element.Animal )]
		[FromPresence(1,Target.Dahan)]
		static public Task Act( TargetSpaceCtx ctx ) {
			var target = ctx.Target;

			ctx.GameState.ModRavage( target, cfg => {
				// defend 3
				cfg.Defend += 3;
				// dahan attach simultaneously with dahan
				cfg.RavageSequence = SimultaneousDamage;
			} );

			return Task.CompletedTask;
		}

		static async Task SimultaneousDamage( RavageEngine eng ) {
			int damageFromInvaders = eng.GetDamageInflictedByInvaders();
			int damageFromDahan = eng.GetDamageInflictedByDahan();

			await eng.DamageLand( damageFromInvaders );
			await eng.DamageDahan( damageFromInvaders );
			await eng.DamageInvaders( damageFromDahan );
		}

	}
}

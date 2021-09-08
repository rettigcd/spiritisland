using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class WordsOfWarning {

		public const string Name = "Words of Warning";

		[SpiritCard( WordsOfWarning.Name, 1, Speed.Fast, Element.Air, Element.Sun, Element.Animal )]
		[FromPresence(1,Target.Dahan)]
		static public Task Act( TargetSpaceCtx ctx ) {
			var target = ctx.Target;

			// defend 3
			ctx.Tokens[TokenType.Defend]+=3;
			ctx.GameState.ModifyRavage( target, cfg => {
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

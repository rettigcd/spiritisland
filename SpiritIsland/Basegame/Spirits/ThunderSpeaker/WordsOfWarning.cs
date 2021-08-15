using SpiritIsland;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class WordsOfWarning {

		public const string Name = "Words of Warning";

		[SpiritCard( WordsOfWarning.Name, 1, Speed.Fast, Element.Air, Element.Sun, Element.Animal )]
		[FromPresence(1,Target.Dahan)]
		static public Task Act( ActionEngine engine, Space target ) {
			// defend 3.
			engine.GameState.Defend(target,3);

			// During ravage, dahan in target land deal damange simultaneously with invaders
			_ = new DeadDahanDealDamageOnce( target, engine.GameState );

			return Task.CompletedTask;
		}


		class DeadDahanDealDamageOnce {

			readonly Space space;

			public DeadDahanDealDamageOnce(Space space,GameState gs) {
				this.space = space;
				gs.DahanDestroyed.Handlers.Add( Execute );
				gs.TimePassed += Unbind; // if no dahan destroyed, unbind
			}

			public Task Execute(GameState gs,DahanDestroyedArgs args ) {
				if(args.space == space) {
					ApplyDeadDahanDamage( gs, args );
					Unbind( gs );
				}
				return Task.CompletedTask;
			}

			private void Unbind( GameState gs ) {
				gs.DahanDestroyed.Handlers.Remove( Execute ); // detach event
			}

			void ApplyDeadDahanDamage( GameState gs, DahanDestroyedArgs args ) {
				int expectedLivingDahanDamage = gs.GetDahanOnSpace( space ) * 2;
				int deadDahanDamage = args.count * 2;
				var grp = gs.InvadersOn( args.space );

				while(deadDahanDamage > 0 && grp.InvaderTypesPresent.Any()) {
					// pick best invader knowing total damage
					Invader invaderToDamage = grp.PickSmartInvaderToDamage( expectedLivingDahanDamage + deadDahanDamage );
					// apply our dead-dahan-damage to that invader
					deadDahanDamage -= grp.ApplyDamageTo1( deadDahanDamage, invaderToDamage );
				}
				// any left over damage will be finished off by the living dahan
			}
		}

	}
}

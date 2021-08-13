using SpiritIsland.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class ReachingGrasp {

		[MinorCard( "Reaching Grasp", 0, Speed.Fast, Element.Sun, Element.Air, Element.Water)]
		[TargetSpirit]
		static public Task Act( ActionEngine engine, Spirit target ) {
			// target spirit gets +2 range with all their Powers
			var original = target.PowerCardApi;
			target.PowerCardApi = new ExtendRange( 2, original );

			engine.GameState.EndOfRoundCleanupAction.Push((_)=>{ 
				target.PowerCardApi = original;
			});
			return Task.CompletedTask;
		}

		class ExtendRange : PowerCardApi {
			readonly int extension;
			readonly PowerCardApi original;
			public ExtendRange(int extension,PowerCardApi original ) {
				this.extension = extension;
				this.original = original;
			}

			public override Task<Space> TargetSpace( ActionEngine engine, IEnumerable<Space> source, int range, Func<Space, bool> filter = null )
				=> original.TargetSpace( engine, source, range + extension, filter );

		}

	}
}

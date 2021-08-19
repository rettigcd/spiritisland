using System;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class SapTheStrengthOfMultitudes {

		[MinorCard( "Sap the Strength of Multitudes", 0, Speed.Fast, "water, animal" )]
		[SapTheStrengthOfMultitudes.FromPresence]
		static public Task ActAsync( TargetSpaceCtx ctx) {
			// defend 5
			ctx.Defend(5);
			return Task.CompletedTask;
		}

		// range 0
		[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
		public class FromPresenceAttribute : SpiritIsland.TargetSpaceAttribute {
			public FromPresenceAttribute() : base( From.Presence, null, 0, Target.Any ) { }
			protected override int CalcRange( IMakeGamestateDecisions ctx ) => range
				// if you have 1 air, increate this power's range by 1
				+ (ctx.Self.Elements.Contains("1 air") ? 1 : 0);
		}

	}

}

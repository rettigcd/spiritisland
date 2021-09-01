using System;

namespace SpiritIsland.BranchAndClaw {

	static public class IInvaderCountExtensions {

		static public void AddStrifeTo(this IInvaderCounts counts, InvaderSpecific invader, int count = 1 ) {
			var concreate = (InvaderCounts)counts;

			// Remove old type from 
			if(concreate[invader]<count)
				throw new ArgumentOutOfRangeException($"collection does not contain {count} {invader.Summary}");
			concreate[invader] -= count;

			// Add new strifed
			int curStrifeCount = invader is StrifedInvader si ? si.StrifeCount : 0;
			var strifed = StrifedInvader.Generator.WithStrife(invader, curStrifeCount +1 );

			concreate[strifed] += count;
		}

		/// <summary>
		/// Gets strifed token with exactly # of strife listed.
		/// </summary>
		static public InvaderSpecific WithStrife(this InvaderSpecific orig, int strifeCount ) {
			return StrifedInvader.Generator.WithStrife( orig, strifeCount );
		}

		static public int Strife( this InvaderSpecific orig ) {
			return orig is StrifedInvader si 
				? si.StrifeCount
				: 0;
		}


		static public InvaderSpecific AddStrife(this InvaderSpecific orig, int deltaStrife ) {
			return orig.WithStrife( orig.Strife()+ deltaStrife );
		}

	}

}

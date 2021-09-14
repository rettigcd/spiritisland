using System;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	static public class IInvaderCountExtensions {

		static public void AddStrifeTo(this TokenCountDictionary counts, Token invader, int count = 1 ) {
			var concreate = (TokenCountDictionary)counts;

			// Remove old type from 
			if(concreate[invader]<count)
				throw new ArgumentOutOfRangeException($"collection does not contain {count} {invader.Summary}");
			concreate[invader] -= count;

			// Add new strifed
			int curStrifeCount = invader is StrifedInvader si ? si.StrifeCount : 0;
			var strifed = StrifedInvader.Generator.WithStrife(invader, curStrifeCount +1 );

			concreate[strifed] += count;
		}

		/// <summary> Gets strifed token with exactly # of strife listed. </summary>
		static public Token WithStrife(this Token orig, int strifeCount ) {
			return StrifedInvader.Generator.WithStrife( orig, strifeCount );
		}

		static public int Strife( this Token orig ) {
			return orig is StrifedInvader si 
				? si.StrifeCount
				: 0;
		}


		static public Token AddStrife(this Token orig, int deltaStrife ) {
			return orig.WithStrife( orig.Strife()+ deltaStrife );
		}

	}

}

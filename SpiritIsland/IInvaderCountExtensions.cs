using System;

namespace SpiritIsland {

	static public class IInvaderCountExtensions {

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

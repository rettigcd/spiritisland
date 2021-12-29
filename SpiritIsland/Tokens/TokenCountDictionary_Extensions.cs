﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public static class TokenCountDictionary_Extensions {

		#region Generic - Single

		static public Token[] OfType( this TokenCountDictionary counts, TokenClass healthyType )
			=> counts.Keys.Where( specific => healthyType == specific.Class ).ToArray();

		static public bool Has( this TokenCountDictionary counts, TokenClass inv )
			=> counts.OfType( inv ).Any();

		static public int Sum( this TokenCountDictionary counts, TokenClass generic )
			=> counts.OfType( generic ).Sum( k => counts[k] );

		#endregion

		#region Generic - Multiple (Any)

		static public Token[] OfAnyType( this TokenCountDictionary counts, params TokenClass[] healthyTypes )
			=> counts.Keys.Where( specific => healthyTypes.Contains( specific.Class ) ).ToArray();

		static public bool HasAny( this TokenCountDictionary counts, params TokenClass[] healthyInvaders )
			=> counts.OfAnyType( healthyInvaders ).Any();

		static public int SumAny( this TokenCountDictionary counts, params TokenClass[] healthyInvaders )
			=> counts.OfAnyType( healthyInvaders ).Sum( k => counts[k] );


		#endregion

	}

}

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public static class TokenCountDictionary_Extensions {

		#region Invader Specific

		static public IEnumerable<Token> Invaders( this TokenCountDictionary counts )
			=> counts.OfAnyType(Invader.City,Invader.Town,Invader.Explorer);

		static public bool HasInvaders( this TokenCountDictionary counts )
			=> counts.Invaders().Any();

		static public int TownsAndCitiesCount( this TokenCountDictionary counts ) => counts.SumAny( Invader.Town, Invader.City );

		static public int InvaderTotal( this TokenCountDictionary counts ) => counts.Invaders().Sum(i=>counts[i]);

		#endregion

		#region Generic - Single

		static public Token[] OfType( this TokenCountDictionary counts, TokenGroup healthyType )
			=> counts.Keys.Where( specific => healthyType == specific.Generic ).ToArray();

		static public bool Has( this TokenCountDictionary counts, TokenGroup inv )
			=> counts.OfType( inv ).Any();

		static public bool Has( this TokenCountDictionary counts, Token token )	=> counts[token]>0;

		static public int Sum( this TokenCountDictionary counts, TokenGroup generic )
			=> counts.OfType( generic ).Sum( k => counts[k] );

		#endregion

		#region Generic - Multiple (Any)

		static public Token[] OfAnyType( this TokenCountDictionary counts, params TokenGroup[] healthyTypes )
			=> counts.Keys.Where( specific => healthyTypes.Contains( specific.Generic ) ).ToArray();

		static public bool HasAny( this TokenCountDictionary counts, params TokenGroup[] healthyInvaders )
			=> counts.OfAnyType( healthyInvaders ).Any();

		static public int SumAny( this TokenCountDictionary counts, params TokenGroup[] healthyInvaders )
			=> counts.OfAnyType( healthyInvaders ).Sum( k => counts[k] );


		#endregion


		static public void Adjust( this TokenCountDictionary counts, Token specific, int delta ) {
			if(specific.Health == 0) throw new System.ArgumentException( "Don't try to track dead invaders." );
			counts[specific] += delta;
		}

	}

}

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
	public interface IInvaderCounts {
		int this[InvaderSpecific invader] { get; set; }
		IEnumerable<InvaderSpecific> Keys { get; }
		int Total { get; }
	}

	public static class IInvaderCountsExtensions {

		static public bool HasAny( this IInvaderCounts counts, params Invader[] healthyInvaders ) 
			=> counts.Keys.Any( k => healthyInvaders.Contains( k.Generic ) );

		static public bool Has( this IInvaderCounts counts, Invader inv ) 
			=> counts.Keys.Any( k => k.Generic == inv );

		static public int SumEach( this IInvaderCounts counts, Invader generic ) 
			=> counts.Keys.Where( k => k.Generic == generic ).Sum( k => counts[k] );

		static public int TownsAndCitiesCount( this IInvaderCounts counts ) => counts.SumEach( Invader.Town ) + counts.SumEach( Invader.City );

		static public InvaderSpecific[] FilterBy( this IInvaderCounts counts, params Invader[] healthyTypes ) 
			=> counts.Keys.Where( specific => healthyTypes.Contains( specific.Generic ) ).ToArray();

		static public void Adjust( this IInvaderCounts counts, InvaderSpecific specific, int delta ) {
			if(specific.Health == 0) throw new System.ArgumentException( "Don't try to track dead invaders." );
			counts[specific] += delta;
		}

		static public void Add( this IInvaderCounts counts, Invader generic, int delta=1 ) {
			if(delta<0) throw new ArgumentOutOfRangeException(nameof(delta));
			counts[generic.Healthy] += delta;
		}

		static public string ToSummary(this IInvaderCounts counts) {
			static int Order_CitiesTownsExplorers( InvaderSpecific invader )
				=> -(invader.FullHealth * 10 + invader.Health);
			return counts.Keys
				.Where( specific => counts[specific] > 0 )
				.OrderBy( Order_CitiesTownsExplorers )
				.Select( invader => counts[invader] + invader.Summary )
				.Join( "," );
		}


	}

}

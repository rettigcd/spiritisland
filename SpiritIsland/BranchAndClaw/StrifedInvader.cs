using System.Collections.Generic;

namespace SpiritIsland.BranchAndClaw {

	public class StrifedInvader : InvaderSpecific {

		static internal readonly StrifedInvaderGenerator Generator = new StrifedInvaderGenerator();

		public StrifedInvader(Invader generic,InvaderSpecific[] seq,int health, int strifeCount)
			:base( generic, seq, health ) {
			StrifeCount = strifeCount;
		}
		public override string Summary => base.Summary + new string( '^', StrifeCount);
		public int StrifeCount { get; }

		#region class Generator

		public class StrifedInvaderGenerator {

			public InvaderSpecific WithStrife( InvaderSpecific orig, int strifeCount ) {
				if( strifeCount<0 )
					throw new System.ArgumentOutOfRangeException(nameof(strifeCount));
				if(strifeCount == 0)
					return orig.Generic[orig.Health];
				var seq = GetSequence( orig.Generic, strifeCount );

				return seq[orig.Health];
			}

			// Creates an invader health sequence and caches it.
			// uses Invader *Generic* as the key
			StrifedInvader[] GetSequence( Invader generic, int strifeCount ) {
				string key = generic.Label+ strifeCount;
				if(sequenceCache.ContainsKey( key )) return sequenceCache[key];
				var seq = BuildHealthSequence( generic, strifeCount );
				sequenceCache.Add( key, seq );
				return seq;
			}

			StrifedInvader[] BuildHealthSequence( Invader generic, int strifeCount ) {
				int fullHealth = generic.Healthy.Health;
				var seq = new StrifedInvader[fullHealth + 1];
				for(int h = 0; h <= fullHealth; ++h)
					seq[h] = new StrifedInvader( generic, seq, h, strifeCount );
				return seq;
			}

			Dictionary<string, StrifedInvader[]> sequenceCache = new Dictionary<string, StrifedInvader[]>();

		}

		#endregion

	}

}

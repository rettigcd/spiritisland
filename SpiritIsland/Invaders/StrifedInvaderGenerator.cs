namespace SpiritIsland;

public partial class StrifedInvader {

	public class StrifedInvaderGenerator {

		public Token WithStrife( Token orig, int strifeCount ) {
			if( strifeCount<0 )
				throw new System.ArgumentOutOfRangeException(nameof(strifeCount));
			if(strifeCount == 0)
				return orig.Class[orig.Health];
			var seq = GetSequence( orig.Class, strifeCount );

			return seq[orig.Health];
		}

		// Creates an invader health sequence and caches it.
		// uses Invader *Generic* as the key
		StrifedInvader[] GetSequence( TokenClass generic, int strifeCount ) {
			string key = generic.Label+ strifeCount;
			if(sequenceCache.ContainsKey( key )) return sequenceCache[key];
			var seq = BuildHealthSequence( generic, strifeCount );
			sequenceCache.Add( key, seq );
			return seq;
		}

		static StrifedInvader[] BuildHealthSequence( TokenClass generic, int strifeCount ) {
			int fullHealth = generic.Default.Health;
			var seq = new StrifedInvader[fullHealth + 1];
			for(int h = 0; h <= fullHealth; ++h)
				seq[h] = new StrifedInvader( generic, seq, h, strifeCount );
			return seq;
		}

		readonly Dictionary<string, StrifedInvader[]> sequenceCache = new();

	}

}
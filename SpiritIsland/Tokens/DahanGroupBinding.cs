using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class DahanGroupBinding {

		public bool Frozen { get; set; }

		readonly TokenCountDictionary counts;
		readonly TokenGroup tokenGroup;
		readonly IDestroySpaceTokens tokenDestroyer;

		public DahanGroupBinding( TokenCountDictionary tokens, TokenGroup tokenGroup, IDestroySpaceTokens tokenDestroyer ) {
			this.counts = tokens;
			this.tokenGroup = tokenGroup;
			this.tokenDestroyer = tokenDestroyer;
		}

		public IEnumerable<Token> Keys => counts.OfType(tokenGroup);

		public bool Any => Count > 0;
		public int Count => counts.Sum(tokenGroup);

		public int this[int index] {
			get => counts[tokenGroup[index]];
			private set => counts[tokenGroup[index]] = value; // This is called for: (a) initializaion, (b) replacing, (c) damage
		}

		public static implicit operator int( DahanGroupBinding b ) => b.Count;

		/// <summary> Adds a Dahan from the bag, or out of thin air. </summary>
		public void Add( Token token ) => counts[token]++;
		public void Add(int count) => counts[tokenGroup[2]] += count;
		/// <summary> Adds a damaged Dahan from the bag, or out of thin air. </summary>
		public void AddDamaged(int count) => counts[tokenGroup[1]] += count;

		/// <summary> Returns the Token removed </summary>
		public Token Remove1( Token desiredToken = null ) {
			if(Frozen) return null;

			var t1 = tokenGroup[1];
			if( t1 == (desiredToken ?? t1) && counts[t1]>0 ){ counts[t1]--;  return t1; }

			var t2 = tokenGroup[2];
			if(t2 == (desiredToken ?? t2) && counts[t2] > 0) { counts[t2]--; return t2; }

			return null;
		}

		#region Damage

		public async Task Apply1DamageToAll( Cause cause ) {
			if(Frozen) return;

			// Destroy all 1-health dahan
			await Destroy( this[1], 1, cause );

			// Downgrade 2-health to 1 health
			this[1] = this[2];
			this[2] = 0;
		}

		public async Task ApplyDamage( int damageToDahan, Cause cause ) {
			if(Frozen) return;

			while(damageToDahan--> 0 && Any) {
				// destroy damaged
				if(0 < this[1])
					await Destroy(1,1,cause);
				// othwise, damage healthy
				else if(0 < this[2]) {
					this[1]++;
					this[2]--;
				}
			}

		}

		#endregion

		#region Destory

		public async Task Destroy( int countToDestroy, Cause cause ) {
			if(Frozen) return;

			int damagedToDestroy = System.Math.Min(countToDestroy,this[1]);
			await Destroy(damagedToDestroy, 1, cause);
			await Destroy(countToDestroy - damagedToDestroy, 2, cause);
		}

		public async Task Destroy( int count, int originalHealth, Cause cause ) {
			if(Frozen) return;

			await tokenDestroyer.DestroyToken(count, tokenGroup[originalHealth], cause);
		}

		public void RemoveAll() {
			if(Frozen) return;

			this[1] = 0;
			this[2] = 0;
		}

		#endregion
	}

}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class DahanGroupBinding {

		public bool Frozen { get; set; }

		readonly TokenCountDictionary tokens;
		readonly TokenCategory tokenGroup;

		public DahanGroupBinding( TokenCountDictionary tokens ) {
			this.tokens = tokens;
			this.tokenGroup = TokenType.Dahan;
		}

		public IEnumerable<Token> Keys => tokens.OfType(tokenGroup);

		public bool Any => Count > 0;
		public int Count => tokens.Sum(tokenGroup);

		public int this[int index] => tokens[tokenGroup[index]];

		public static implicit operator int( DahanGroupBinding b ) => b.Count;

		/// <summary> Adds a Dahan from the bag, or out of thin air. </summary>
		public Task Add( int count, AddReason reason = AddReason.Added ) {
			return tokens.Add(TokenType.Dahan.Default,count, reason );
		}

		/// <summary> Adds a damaged Dahan from the bag, or out of thin air. </summary>
		public void InitDamaged( int count ) => tokens.Init( TokenType.Dahan[1], count );

		public void Init(int count ) => tokens.Init(TokenType.Dahan.Default, count );

		public void Adjust(Token token, int delta ) => tokens.Adjust(token,delta);
		public void Init(Token token, int count ) => tokens.Init(token,count);


		/// <summary> Returns the Token removed </summary>
		public async Task<Token> Remove1( RemoveReason reason ) {
			if(Frozen) return null;

			var t1 = tokenGroup[1];
			if( tokens[t1]>0 ){
				await tokens.Remove(t1,1, reason );
				return t1;
			}

			var t2 = tokenGroup[2];
			if( tokens[t2] > 0) { 
				await tokens.Remove(t2,1, reason );
				return t2;
			}

			return null;
		}

		public async Task<bool> Remove1( Token desiredToken, RemoveReason reason ) {
			if( !Frozen && 0<tokens[desiredToken] ){ 
				await tokens.Remove(desiredToken,1, reason );
				return true;
			}

			return false; // unable to remove desired token
		}

		public void Drown() {

			// !!! need to change this to async so we publish the .Destroy event and UI will update.

			if(Frozen) return;
			InitDamaged( 0 );
			Init( 0 );
		}

		#region Damage

		public async Task Apply1DamageToAll( Cause cause ) { // Called By Power (i.e. not invaders)
			if(Frozen) return;

			// Destroy all 1-health dahan
			await Destroy( this[1], 1, cause );

			// Downgrade 2-health to 1 health
			this.InitDamaged( this[2] ); // no token events
			this.Init( 0 );
		}

		public async Task ApplyDamage( int damageToDahan, Cause cause ) {
			if(Frozen) return;

			// Destroy injured first
			int damagedToDestroy = System.Math.Min( this[1], damageToDahan );
			if( 0 < damagedToDestroy ) {
				await Destroy(damagedToDestroy,1, cause);
				damageToDahan -= damagedToDestroy;
			}

			// Destroy healthy next
			int healthyToDestroy = System.Math.Min( this[2], damagedToDestroy/2 );
			if( 0 < healthyToDestroy ) {
				await Destroy(healthyToDestroy,2,cause);
				damageToDahan -= healthyToDestroy * 2;
			}

			// Injure remaining
			if( damageToDahan == 1 && 0 < this[2]) {
				this.InitDamaged( 1 );
				this.Init( this[2]-1 );
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

			var reason = cause == Cause.Invaders 
				? RemoveReason.DestroyedInBattle 
				: RemoveReason.Destroyed;

			await tokens.Remove( tokenGroup[originalHealth], count, reason );
		}

		#endregion
	}

}

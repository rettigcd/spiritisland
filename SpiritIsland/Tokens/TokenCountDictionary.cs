using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class TokenCountDictionary : IDestroySpaceTokens {

		#region constructor

		public TokenCountDictionary( Space space, CountDictionary<Token> counts, IIslandTokenApi tokenApi ) {
			this.Space = space;
			this.counts = counts;
			this.tokenApi = tokenApi;
		}

		/// <summary> Clone / copy constructor </summary>
		public TokenCountDictionary( TokenCountDictionary src ) {
			this.Space = src.Space;
			counts = src.counts.Clone();
			this.tokenApi = src.tokenApi;
			this.virtualDefends = src.virtualDefends;
		}

		#endregion
		public Space Space { get; }

		public int this[Token specific] {
			get {
				if( specific == TokenType.Defend ) return Defend.Count;
				ValidateIsAlive( specific );
				return counts[specific];
			}
			set {
				ValidateIsAlive( specific );
				counts[specific] = value; 
			}
		}

		public IEnumerable<Token> Keys => counts.Keys; // !! This won't list virtual (defend) tokens

		public string InvaderSummary { get {
			static int Order_CitiesTownsExplorers( Token invader )
				=> -(invader.FullHealth * 10 + invader.Health);
			return this.Invaders()
				.OrderBy( Order_CitiesTownsExplorers )
				.Select( invader => counts[invader] + invader.Summary )
				.Join( "," );
		} }

		public string Summary { get {
			return this.Keys
				.OrderBy( k=>k.Summary )
				.Select( invader => counts[invader] + invader.Summary )
				.Join( "," );
		} }

		public override string ToString() => Space.Label + ":" + Summary;

		public TokenBinding Blight => new TokenBinding( this, TokenType.Blight);
		public IDefendTokenBindings Defend => new DefendTokenBinding( Space, counts, tokenApi.GetVirtualDefendFor );
		public TokenBinding Beasts => new ( this, TokenType.Beast );
		public TokenBinding Disease => new ( this, TokenType.Disease );
		public TokenBinding Wilds => new ( this, TokenType.Wilds );
		public TokenBinding Badlands => new ( this, TokenType.Badlands ); // This should not be used directly from inside Actions
		public DahanGroupBinding Dahan{
			get => _dahan ??= new ( this, TokenType.Dahan, this ); // ! change the ??= to ?? and we would not need to hang on to the binding.
			set => _dahan = value; // Allows Dahan behavior to be overridden
		}
		DahanGroupBinding _dahan;

		#region private

		static void ValidateIsAlive( Token specific ) {
			if(specific.Health == 0) 
				throw new ArgumentException( "We don't store dead counts" );
		}

		public readonly CountDictionary<Token> counts; // !!! public for Tokens_ForIsland Memento, create own momento.
		readonly IIslandTokenApi tokenApi;
		readonly List<Func<Space,int>> virtualDefends;

		#endregion

		public void Adjust( Token specific, int delta ) {
			if(specific.Health == 0) throw new System.ArgumentException( "Don't try to track dead tokens." );
			counts[specific] += delta;
		}

		public void AddStrifeTo( Token invader, int count = 1 ) {

			// Remove old type from 
			if(this[invader]<count)
				throw new ArgumentOutOfRangeException($"collection does not contain {count} {invader.Summary}");
			this[invader] -= count;

			// Add new strifed
			int curStrifeCount = invader is StrifedInvader si ? si.StrifeCount : 0;
			var strifed = StrifedInvader.Generator.WithStrife(invader, curStrifeCount +1 );

			this[strifed] += count;
		}

		public Token RemoveStrife( StrifedInvader orig, int tokenCount ) {
			Token lessStrifed = orig.AddStrife( -1 );
			this[lessStrifed] += tokenCount;
			this[orig] -= tokenCount;
			return lessStrifed;
		}

		/// <summary> Adds .Space to context and calls Parent
		public Task DestroyToken( int countToDestroy, Token token, Cause cause )
			=> tokenApi.DestroyIslandToken( Space, countToDestroy, token, cause);
		
	}

	public interface IDestroySpaceTokens {
		Task DestroyToken( int countToDestroy, Token token, Cause cause );
	}



}

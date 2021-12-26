using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class TokenCountDictionary {

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
			tokenApi = src.tokenApi;
		}

		#endregion
		public Space Space { get; }

		public int this[Token specific] {
			get {
				ValidateIsAlive( specific );
				int count = counts[specific];
				if( specific is UniqueToken ut )
					count += tokenApi.GetDynamicTokenFor(Space, ut);
				return count;
			}
			private set {
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
				.Where(t=>t.Class.Category != TokenCategory.Presence)
				.OrderBy( k=>k.Summary )
				.Select( invader => counts[invader] + invader.Summary )
				.Join( "," );
		} }

		public override string ToString() => Space.Label + ":" + Summary;

		public TokenBinding Blight => new TokenBinding( this, TokenType.Blight);
		public IDefendTokenBinding Defend => new DefendTokenBinding( this );
		public TokenBinding Beasts => new ( this, TokenType.Beast );
		public TokenBinding Disease => new ( this, TokenType.Disease );
		public TokenBinding Wilds => new ( this, TokenType.Wilds );
		public TokenBinding Badlands => new ( this, TokenType.Badlands ); // This should not be used directly from inside Actions
		public DahanGroupBinding Dahan{
			get => _dahan ??= new ( this ); // ! change the ??= to ?? and we would not need to hang on to the binding.
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

		#endregion

		/// <summary> Non-event-triggering setup </summary>
		public void Adjust( Token specific, int delta ) {
			if(specific.Health == 0) throw new System.ArgumentException( "Don't try to track dead tokens." );
			counts[specific] += delta;
		}

		/// <summary> Non-event-triggering setup </summary>
		public void Init( Token specific, int value ) {
			counts[specific] = value;
		}

		public Task Add( Token token, int count, AddReason addReason = AddReason.Added ) {
			if(count < 0) throw new System.ArgumentOutOfRangeException( nameof( count ) );
			this[token] += count;
			return tokenApi.Publish_Added( Space, token, count, addReason );
		}

		public Task Remove( Token token, int count, RemoveReason reason = RemoveReason.Removed ) {
			count = System.Math.Min( count, this[token] );

			if(count==0) return Task.CompletedTask;
			if(count < 0) throw new System.ArgumentOutOfRangeException( nameof( count ) );

			this[token] -= count;

			return tokenApi.Publish_Removed( Space, token, count, reason );
		}

		// Convenience only
		public Task Destroy( Token token, int count ) => Remove(token, count, RemoveReason.Destroyed );

		public async Task MoveTo(Token token, Space destination ) {

			// Remove from source
			if( token.Class != TokenType.Dahan)
				Adjust( token, -1 );
			else if( ! (await Dahan.Remove1(token, RemoveReason.MovedFrom)) ) // !!! Moving publishes a Move event, don't publish this Remove event
				return;

			// Add to destination
			tokenApi.GetTokensFor( destination ).Adjust( token, 1 );

			// Publish
			await tokenApi.Publish_Moved( token, Space, destination );

		}

		#region Invader Specific

		public IEnumerable<Token> Invaders() => this.OfAnyType( Invader.City, Invader.Town, Invader.Explorer );

		public bool HasInvaders() => Invaders().Any();

		public bool HasStrife => Keys.OfType<StrifedInvader>().Any();

		public int CountStrife() => Keys.OfType<StrifedInvader>().Sum( t => counts[t] );

		public int TownsAndCitiesCount() => this.SumAny( Invader.Town, Invader.City );

		public int InvaderTotal() => Invaders().Sum( i => counts[i] );

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

		#endregion

		public Token RemoveStrife( StrifedInvader orig, int tokenCount ) {
			Token lessStrifed = orig.AddStrife( -1 );
			this[lessStrifed] += tokenCount;
			this[orig] -= tokenCount;
			return lessStrifed;
		}

	}

}

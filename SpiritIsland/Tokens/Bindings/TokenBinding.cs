using System.Threading.Tasks;

namespace SpiritIsland {

	public class TokenBinding {

		readonly protected TokenCountDictionary tokens;
		readonly Token token;

		public TokenBinding( TokenCountDictionary tokens, Token token ) {
			this.tokens = tokens;
			this.token = token;
		}
		public bool Any => Count > 0;

		public virtual int Count => tokens[token];

		public void Init(int count ) => tokens.Init(token,count);
		public void Adjust( int delta ) => tokens.Adjust(token, delta);

		public Task Add(int count, AddReason reason = AddReason.Added) => tokens.Add(token,count, reason);
		public virtual Task Remove(int count, RemoveReason reason) => tokens.Remove(token,count,reason);

		public static implicit operator int( TokenBinding b ) => b.Count;

	}

}

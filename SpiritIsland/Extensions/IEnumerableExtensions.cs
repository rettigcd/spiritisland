namespace SpiritIsland;

static public class IEnumerableExtensions {
	public static string Join(this IEnumerable<string> items) => string.Join(string.Empty,items);
	public static string Join(this IEnumerable<string> items, string glue ) => string.Join(glue,items);
	public static string Join_WithLast(this IEnumerable<string> items, string glue, string lastGlue ) {
		string[] itemArray = items.ToArray();
		int last = itemArray.Length-1;
		StringBuilder buf = new StringBuilder();
		for(int i=0;i<itemArray.Length;++i) {
			if(0 < i)
				buf.Append( i==last ? lastGlue : glue);
			buf.Append( itemArray[i] );
		}
		return buf.ToString();
	}

	public static T VerboseSingle<T>(this IEnumerable<T> items, Func<T,bool> predicate){
		var result = items.Where(predicate).ToList();
		if( result.Count == 1 ) return result[0];

		string name = typeof(T).Name;
		throw new InvalidOperationException($"Expected 1 but found {result.Count} items of type {name}");
	}

	public static T VerboseSingle<T>(this IEnumerable<T> items,string msg){
		var result = items.ToList();
		if( result.Count == 1 ) return result[0];

		string name = typeof(T).Name;
		throw new InvalidOperationException($"{msg} Expected 1 but found {result.Count} items of type {name}");
	}

	static public void Shuffle<T>( this Random randomizer, IList<T> list ) {
		int n = list.Count;
		while(n > 1) {
			n--;
			int k = randomizer.Next( n + 1 );
			(list[n], list[k]) = (list[k], list[n]);
		}
	}

	static public bool IsOneOf<T>( this T needle, params T[] haystack ) where T : Enum
		=> haystack.Contains( needle );

	static public bool IsOneOf( this ITokenClass needle, params ITokenClass[] haystack )
		=> haystack.Contains( needle );

	static public bool IsDestroyingPresence( this RemoveReason reason ) => reason.IsOneOf( RemoveReason.Destroyed, RemoveReason.Replaced, RemoveReason.Removed );

	static public void SetItems<T>(this List<T> list, params T[] items ) { list.Clear(); list.AddRange(items);}

	static public void SetItems<T>(this HashSet<T> hashSet, params T[] items ) { hashSet.Clear(); foreach(var item in items) hashSet.Add(item);}

	/// <summary> Includes an item on the end IF it is not already there. </summary>
	static public IEnumerable<T> Include<T>(this IEnumerable<T> orig, T addition ) {
		bool addIt = true;
		foreach(var item in orig){
			if(item.Equals(addition)) addIt = false;
			yield return item;
		}
		if(addIt)
			yield return addition;
	}

	/// <summary>
	/// [0] => top of stack, [^1] => bottom of stack
	/// </summary>
	static public void SetItems<T>(this Stack<T> stack, params T[] saved ) { 
		stack.Clear(); 				
		for(int i=saved.Length;i-->0;) 
			stack.Push(saved[i]);
	}

	static public IEnumerable<T> Order<T>(this IEnumerable<T> src) => src.OrderBy(x => x);

	public static IEnumerable<Space> Downgrade(this IEnumerable<SpaceState> spaceStates) => spaceStates.Select(x=>x.Space);

	public static IEnumerable<SpaceState> Tokens(this IEnumerable<Space> spaces) => spaces.Select( ActionScope.Current.AccessTokens );
	public static IEnumerable<Space> IsInPlay( this IEnumerable<Space> spaces ) => spaces.Where( TerrainMapper.Current.IsInPlay );

	public static IEnumerable<SpaceToken> WhereIsOn( this IEnumerable<SpaceToken> spaceTokens, IEnumerable<SpaceState> spaceStates ) {
		var validSpaces = spaceStates.Select(x=>x.Space).ToHashSet();
		return spaceTokens.Where(t=>validSpaces.Contains(t.Space));
	}

	public static IEnumerable<SpaceState> IsInPlay( this IEnumerable<SpaceState> spaces ) 
		=> spaces.Where( x=>TerrainMapper.Current.IsInPlay(x.Space) );

	public static IEnumerable<Space> ISInPlay( this IEnumerable<Space> spaces )
		=> spaces.Where( TerrainMapper.Current.IsInPlay );

	public static IEnumerable<string> SelectLabels(this IEnumerable<SpaceState> spaceStates) => spaceStates.Select(x=>x.Space.Text);

	public static Value Get<Key,Value>(this IDictionary<Key,Value> dict, Key key, Func<Value> newValueGenerator) {
		if(dict.ContainsKey(key)) return dict[key];
		Value newValue = newValueGenerator();
		dict.Add(key,newValue);
		return newValue;
	}

	public static Value Get<Key, Value>( this IDictionary<Key, Value> dict, Key key, Value defaultValue = default ) {
		if(dict.ContainsKey( key )) return dict[key];
		Value newValue = defaultValue;
		dict.Add( key, newValue );
		return newValue;
	}


}

static public class SpaceTokenExtensions {
	public static SpaceToken On( this IToken token, Space space ) => new SpaceToken( space, token );

	/// <summary>Shows the Space in the SpaceToken's description.</summary>
	public static IEnumerable<SpaceToken> On( this IEnumerable<IToken> tokens, Space space ) 
		=> tokens.Select( t => t.On( space ) );

	/// <summary>Convenience method.  Downgrades space-states to spaces.</summary>
	public static IEnumerable<SpaceToken> On( this IToken token, IEnumerable<SpaceState> spaces ) => token.On( spaces.Downgrade() );
	public static IEnumerable<SpaceToken> On( this IToken token, IEnumerable<Space> spaces ) => spaces.Select( space => token.On( space ) );

}
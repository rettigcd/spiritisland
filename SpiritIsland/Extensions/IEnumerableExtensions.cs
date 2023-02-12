namespace SpiritIsland;

static public class IEnumerableExtensions {
	public static string Join(this IEnumerable<string> items) => string.Join(string.Empty,items);
	public static string Join(this IEnumerable<string> items, string glue ) => string.Join(glue,items);
	public static string Join_WithLast(this IEnumerable<string> items, string glue, string lastGlue ) {
		var itemArray = items.ToArray();
		int last = itemArray.Length-1;
		var buf = new StringBuilder();
		for(int i=0;i<itemArray.Length;++i) {
			if(i > 0)
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

	// shorter syntax:
	// space.Terrain.IsIn(Terrain.Wetland,Terrain.Sand)
	// vs.
	// new Terraion[]{Terrain.Wetland,Terrain.Sand}.Contains(space.Terrain);
	static public bool IsOneOf<T>( this T needle, params T[] haystack ) where T : Enum
		=> haystack.Contains( needle );

	static public bool IsOneOf( this IEntityClass needle, params IEntityClass[] haystack )
		=> haystack.Contains( needle );

	static public bool IsDestroyingPresence( this RemoveReason reason ) => reason.IsOneOf( RemoveReason.Destroyed, RemoveReason.Replaced, RemoveReason.Removed );

	static public void SetItems<T>(this List<T> list, params T[] items ) { list.Clear(); list.AddRange(items);}

	static public void SetItems<T>(this HashSet<T> hashSet, params T[] items ) { hashSet.Clear(); foreach(var item in items) hashSet.Add(item);}

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

	public static IEnumerable<SpaceState> Upgrade(this IEnumerable<Space> spaces) => GameState.Current.Tokens.Upgrade(spaces);
	public static IEnumerable<Space> IsInPlay( this IEnumerable<Space> spaces ) => spaces.Where( TerrainMapper.Current.IsInPlay );

	public static IEnumerable<SpaceState> IsInPlay( this IEnumerable<SpaceState> spaces ) 
		=> spaces.Where( x=>TerrainMapper.Current.IsInPlay(x.Space) );

	public static IEnumerable<string> SelectLabels(this IEnumerable<SpaceState> spaceStates) => spaceStates.Select(x=>x.Space.Text);

	public static Value Get<Key,Value>(this IDictionary<Key,Value> dict, Key key, Func<Value> newValueGenerator) {
		if(dict.ContainsKey(key)) return dict[key];
		Value newValue = newValueGenerator();
		dict.Add(key,newValue);
		return newValue;
	}

}
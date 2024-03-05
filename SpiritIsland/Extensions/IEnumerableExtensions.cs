namespace SpiritIsland;

static public class IEnumerableExtensions {

	// Strings
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

	/// <summary> For Maui </summary>
	static public string ToResourceName( this string text, string suffix = "" ) {
		var buf = new StringBuilder();
		foreach(char c in text) {
			if(c == '\'') continue;
			buf.Append( c switch {
				char low when char.IsLower( low ) => low,
				char dig when char.IsDigit( dig ) => dig,
				char cap when char.IsUpper( cap ) => char.ToLower( cap ),
				_ => '_',
			} );
		}
		// strip off trainling '_'
		while(1 < buf.Length && buf[^1] == '_') buf.Length--;
		// Maui resource must start in a letter
		if(buf.Length == 0) 
			throw new InvalidOperationException();
		if(!char.IsLower( buf[0] )) buf.Insert(0,'z');
		// Maui resource must end in a letter.
		if(!char.IsLower( buf[^1] )) buf.Append( 'z' );
		buf.Append( suffix );
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

	public static Value Get<Key,Value>(this IDictionary<Key,Value> dict, Key key, Func<Value> newValueGenerator) {
		if(dict.TryGetValue( key, out Value value )) return value;
		Value newValue = newValueGenerator();
		dict.Add(key,newValue);
		return newValue;
	}

	public static Value Get<Key, Value>( this IDictionary<Key, Value> dict, Key key, Value defaultValue = default ) {
		if(dict.TryGetValue( key, out Value value )) return value;
		Value newValue = defaultValue;
		dict.Add( key, newValue );
		return newValue;
	}


}

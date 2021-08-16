﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SpiritIsland {

	/// <summary>
	/// Creates a dictionary of int Values that defaults to 0 when key not present.
	/// </summary>
	public class CountDictionary<K> : IDictionary<K,int> {

		#region constructor

		public CountDictionary(){
			_inner = new Dictionary<K, int>();
		}

		public CountDictionary(IEnumerable<K> items) {
			_inner = items.GroupBy(x=>x)
				.ToDictionary(grp=>grp.Key,grp=>grp.Count() );
		}


		public CountDictionary(Dictionary<K,int> inner){
			this._inner = inner;
			foreach(K key in _inner.Keys.ToArray())
				if(_inner[key] == 0)
					_inner.Remove(key);
		}

		#endregion

		public int this[K key] {
			get { return _inner.ContainsKey( key ) ? _inner[key] : 0; }
			set {
				if(value != 0)
					_inner[key] = value;
				else if(_inner.ContainsKey( key ))
					_inner.Remove( key );
			}
		}

		public bool Contains( params K[] requiredElements ) => Contains( new CountDictionary<K>( requiredElements ) );

		public bool Contains( IDictionary<K, int> needed ) {
			return needed.All( pair => pair.Value <= this[pair.Key] );
		}

		public void AddRange(IEnumerable<K> items) { foreach(var item in items) ++this[item]; }

		#region IDictionary<Key,int> implementation
		public void Clear() => _inner.Clear();
		public Dictionary<K, int>.KeyCollection Keys => _inner.Keys;
		ICollection<K> IDictionary<K, int>.Keys => _inner.Keys;
		public ICollection<int> Values => _inner.Values;
		public int Count => _inner.Count;
		public bool IsReadOnly => false;
		public void Add( K key, int value ){ this[key]=value; }
		public bool ContainsKey( K key ) => _inner.ContainsKey(key);
		public bool Remove( K key ) => this.Remove(key);
		public bool TryGetValue( K key, [MaybeNullWhen( false )] out int value ){ value = this[key]; return true; }
		public void Add( KeyValuePair<K, int> item ) => this[item.Key] = item.Value;
		public bool Contains( KeyValuePair<K, int> item ) => _inner.Contains(item);
		public void CopyTo( KeyValuePair<K, int>[] array, int arrayIndex ) => ((IDictionary<K,int>)_inner).CopyTo(array,arrayIndex);
		public bool Remove( KeyValuePair<K, int> item ) => ((IDictionary<K, int>)_inner).Remove(item);
		public IEnumerator<KeyValuePair<K, int>> GetEnumerator() => _inner.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();
		#endregion

		readonly Dictionary<K,int> _inner; 
	}


	public static class ExtendDictionary {
		static public CountDictionary<T> ToCountDict<T>(this Dictionary<T,int> inner)
			=> new CountDictionary<T>(inner);
	}

}

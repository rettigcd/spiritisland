
using System;
using System.Collections.Generic;

namespace SpiritIsland {

	public class GameState {

		public Island Island { get; set; }

		public void AddBeast( Space space ){ beastCount[space]++; }
		public void AddBlight( Space space ){ blightCount[space]++; }
		public void AddDahan( Space space ){ dahanCount[space]++; }
		public void AddCity( Space space ){ cityCount[space]++; }
		public void AddExplorer( Space space ){ explorerCount[space]++; }
		public void AddTown( Space space ){ townCount[space]++; }
		public void AddWilds( Space space ){ wildsCount[space]++; }

		public int GetDahanOnSpace( Space space ){ return dahanCount[space]; }

		public bool HasDahan( Space space ) => GetDahanOnSpace(space)>0;
		public bool HasInvaders( Space space ) => explorerCount[space]>0||townCount[space]>0||cityCount[space]>0;
		public bool HasWilds( Space s ) => wildsCount[s] > 0;
		public bool HasBlight( Space s ) => blightCount[s] > 0;
		public bool HasBeasts( Space s ) => beastCount[s] > 0;


		readonly DefaultDictionary<Space,int> blightCount = new DefaultDictionary<Space, int>();
		readonly DefaultDictionary<Space,int> beastCount = new DefaultDictionary<Space, int>();

		readonly DefaultDictionary<Space,int> cityCount = new DefaultDictionary<Space, int>();
		readonly DefaultDictionary<Space,int> townCount = new DefaultDictionary<Space, int>();
		readonly DefaultDictionary<Space,int> explorerCount = new DefaultDictionary<Space, int>();

		readonly DefaultDictionary<Space,int> dahanCount = new DefaultDictionary<Space, int>();

		readonly DefaultDictionary<Space,int> wildsCount = new DefaultDictionary<Space, int>();

	}

	class DefaultDictionary<K,V> {
		public DefaultDictionary(){}
		public V this[K key]{
			get{ return _dict.ContainsKey(key) ? _dict[key] : default; }
			set{ _dict[key] = value; }
		}
		readonly Dictionary<K,V> _dict = new Dictionary<K, V>(); 
	}

}

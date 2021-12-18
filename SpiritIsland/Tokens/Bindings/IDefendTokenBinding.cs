namespace SpiritIsland {

	/// <summary> For Defend tokens </summary>
	public interface IDefendTokenBinding {
		int Count { get; }
		void Add( int count );
		void Clear();
	}

}

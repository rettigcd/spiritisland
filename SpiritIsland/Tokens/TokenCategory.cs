
namespace SpiritIsland {

	public interface TokenCategory {

		Token Default { get; }

		char Initial { get; }

		string Label { get; }

		void ExtendHealthRange( int newMaxHealth );

		Token this[int i] { get; }

		bool IsInvader {  get; }
	}

}

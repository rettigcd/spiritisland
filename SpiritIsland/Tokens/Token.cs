
namespace SpiritIsland {

	public interface Token : IOption {

		TokenCategory Category { get; } // originally: readonly

		Img Img { get; }

		string Summary { get; }

		public char Initial { get; }

		Token ResultingDamagedInvader(int damage);

		int Health {get;}

		Token Healthy { get; }

		int FullHealth {get; }

	}

}

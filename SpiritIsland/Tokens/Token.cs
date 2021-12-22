
namespace SpiritIsland {

	public interface Token : IOption {

		TokenClass Class { get; } // originally: readonly

		Img Img { get; }

		string Summary { get; }

		public char Initial { get; }

		Token ResultingDamagedInvader(int damage);

		int Health {get;}

		Token Healthy { get; }

		int FullHealth {get; }

	}

}

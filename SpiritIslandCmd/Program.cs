namespace SpiritIslandCmd {

	class Program {

		static void Main(string[] _) {
			new GamePlayer().Play();
		}

	}

	public enum PhaseType { Growth, Fast, Invader, Slow, TimePasses, Done }

}

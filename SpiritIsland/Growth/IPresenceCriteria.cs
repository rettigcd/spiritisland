namespace SpiritIsland {
	public interface IPresenceCriteria{
		int Range {get; }
		bool IsValid(Space bs,GameState gs);
	}


}


namespace SpiritIsland {
	public interface IQuota {
		IEnumerable<SpaceToken> GetSourceOptionsOn1Space(Space sourceSpace);
		void MarkTokenUsed(IToken token);
		string RemainingTokenDescriptionOn(Space[] sourceSpaces);
	}
}
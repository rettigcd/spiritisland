
namespace SpiritIsland {
	public interface IQuota {
		IEnumerable<SpaceToken> GetSourceOptionsOn1Space(Space sourceSpace);
		void MarkTokenUsed(ITokenLocation tokenLocation);
		string RemainingTokenDescriptionOn(Space[] sourceSpaces);
	}
}
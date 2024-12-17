namespace SpiritIsland;

public interface IHandleCardPlayed : ISpiritMod {
	Task Handle(Spirit spirit, PowerCard card);
}

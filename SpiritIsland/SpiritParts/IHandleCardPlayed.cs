namespace SpiritIsland;

public interface IHandleCardPlayed {
	Task Handle(Spirit spirit, PowerCard card);
}

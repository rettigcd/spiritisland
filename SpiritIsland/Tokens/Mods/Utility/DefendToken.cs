namespace SpiritIsland;

/// <summary> Base for dynamic Defend tokens - THIS ROUND ONLY. </summary>
public abstract class DefendToken(string badge) : DynamicToken(new TokenVariety(Token.Defend, badge)) {}

/// <summary> Defend equal to a spirit's presence count in the land. </summary>
public class PresenceCountDefend(SpiritPresence presence, string badge) : DefendToken(badge) {
	protected override int GetCount(Space space) => presence.CountOn(space);
}

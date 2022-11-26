namespace SpiritIsland;

public class TokenRemovedArgs : ITokenRemovedArgs {

	public TokenRemovedArgs(Token token, RemoveReason reason, UnitOfWork actionId, SpaceState space, int count ) {
		Token = token;
		Reason = reason;
		ActionId = actionId;
		Space = space;
		Count = count;
	}

	public Token Token { get; }
	public SpaceState Space { get; set; }
	public int Count { get; set; }
	public RemoveReason Reason { get; }
	public UnitOfWork ActionId { get; }

	public GameState GameState { get; set; }// set by the token-publisher because TokenCountDictionary doesn't have this info

};


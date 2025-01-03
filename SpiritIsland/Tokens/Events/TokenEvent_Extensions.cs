namespace SpiritIsland;

public static class TokenEvent_Extensions {
	static public bool TokenIsNew(this ITokenAddedArgs args) => args.To is Space space && space[args.Added] == args.Count;
	static public bool TokenIsRetired(this ITokenRemovedArgs args) => args.Count>0 && args.From is Space space && space[args.Removed] == 0;
}


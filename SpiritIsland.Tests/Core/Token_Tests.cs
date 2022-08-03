namespace SpiritIsland.Tests.Core;

public class Token_Tests {

	[Fact]
	public void SummariesAreUnique() {
		var tokens = new Token[] {
			StdTokens.Explorer,
			StdTokens.Town1,StdTokens.Town,
			StdTokens.City1,StdTokens.City2,StdTokens.City,
			StdTokens.Dahan1,StdTokens.Dahan,
			TokenType.Blight, // conflict with Beast
			TokenType.Defend,
			TokenType.Beast,
			TokenType.Disease,
			TokenType.Wilds
		};

		var conflicts = tokens
			.GroupBy(t=>t.ToString())
			.Where(grp=>grp.Count()>1)
			.Select(grp=>grp.Key+" is used for:"+grp.Select(t=>t.Class.Label+":"+(t is HealthToken ht ? ht.RemainingHealth: 0)).Join(", "))
			.Join("\r\n");

		conflicts.ShouldBe("");
	}

}
namespace SpiritIsland.Tests.Core;

public class Token_Tests {

	[Fact]
	public void SummariesAreUnique() {
		var tokens = new Token[] {
			Tokens.Explorer,
			Tokens.Town1,Tokens.Town,
			Tokens.City1,Tokens.City2,Tokens.City,
			Tokens.Dahan1,Tokens.Dahan,
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
using System.Linq;
using Shouldly;
using SpiritIsland.BranchAndClaw;
using Xunit;

namespace SpiritIsland.Tests.Core {

	public class Token_Tests {

		[Fact]
		public void SummariesAreUnique() {
			var tokens = new Token[] {
				Invader.Explorer[1],
				Invader.Town[1],Invader.Town[2],
				Invader.City[1],Invader.City[2],Invader.City[3],
				TokenType.Dahan[1],TokenType.Dahan[2],
				TokenType.Blight, // conflict with Beast
				TokenType.Defend,
				BacTokens.Beast,
				BacTokens.Disease,
				BacTokens.Wilds
			};

			var conflicts = tokens
				.GroupBy(t=>t.Summary)
				.Where(grp=>grp.Count()>1)
				.Select(grp=>grp.Key+" is used for:"+grp.Select(t=>t.Generic.Label+":"+t.Health).Join(", "))
				.Join("\r\n");

			conflicts.ShouldBe("");
		}

	}

}

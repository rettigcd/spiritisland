using Shouldly;
using System;
using System.Linq;

namespace SpiritIsland.Tests {

	internal static class TargetSpaceCtx_ExtensionsForTesting {

		public static void Init( this TokenCountDictionary currentTokens, string expectedInvaderSummary ) {

			CountDictionary<Token> dict = new();
			foreach(var part in expectedInvaderSummary.Split( ',' )) {
				var token = part[1..] switch {
					"E@1" => Invader.Explorer.Default,
					"T@2" => Invader.Town.Default,
					"C@3" => Invader.City.Default,
					"D@2" => TokenType.Dahan.Default,
					_ => throw new ArgumentException("invalide tokentype found in "+expectedInvaderSummary)
				};
				dict.Add(token, int.Parse(part.Substring(0,1)) );
			}

			var tokensToRemove = currentTokens.Keys.Except(dict.Keys).ToArray();
			foreach(var old in tokensToRemove)
				currentTokens[old] = 0;
			foreach(var p in dict)
				currentTokens[p.Key] = p.Value;

			currentTokens.Summary.ShouldBe( expectedInvaderSummary );
		}

		public static void ClearAllBlight( this SpiritGameStateCtx ctx ) {
			// So it doesn't cascade and require extra interactions...
			foreach(var space in ctx.AllSpaces) {
				var tmpCtx = ctx.TargetSpace(space);
				while(tmpCtx.HasBlight)
					tmpCtx.RemoveBlight();
			}
		}

	}

}

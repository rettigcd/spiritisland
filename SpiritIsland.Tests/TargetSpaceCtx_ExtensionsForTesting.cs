using Shouldly;
using System;
using System.Linq;

namespace SpiritIsland.Tests {

	internal static class TargetSpaceCtx_ExtensionsForTesting {

		public static void InitTokens( this TargetSpaceCtx spaceCtx, string expectedInvaderSummary ) {

			CountDictionary<Token> dict = new();
			foreach(var part in expectedInvaderSummary.Split( ',' )) {
				var token = part.Substring(1) switch {
					"E@1" => Invader.Explorer.Default,
					"T@2" => Invader.Town.Default,
					"C@3" => Invader.City.Default,
					"D@2" => TokenType.Dahan.Default,
					_ => throw new ArgumentException("invalide tokentype found in "+expectedInvaderSummary)
				};
				dict.Add(token, int.Parse(part.Substring(0,1)) );
			}
			var tokensToRemove = spaceCtx.Tokens.Keys.Except(dict.Keys).ToArray();
			foreach(var old in tokensToRemove)
				spaceCtx.Tokens[old] = 0;
			foreach(var p in dict)
				spaceCtx.Tokens[p.Key] = p.Value;

			spaceCtx.Tokens.Summary.ShouldBe( expectedInvaderSummary );
		}

	}

}

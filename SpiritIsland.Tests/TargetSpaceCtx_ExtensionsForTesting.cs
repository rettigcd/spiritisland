using Shouldly;
using SpiritIsland.BranchAndClaw;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Tests {

	internal static class TargetSpaceCtx_ExtensionsForTesting {

		public static void Init( this TokenCountDictionary currentTokens, string expectedInvaderSummary ) {

			CountDictionary<Token> desiredTokens = new();
			if(!string.IsNullOrEmpty( expectedInvaderSummary )) { 
				foreach(var part in expectedInvaderSummary.Split( ',' )) {
					var token = part[1..] switch {
						"E@1" => Invader.Explorer.Default,
						"T@2" => Invader.Town.Default,
						"C@3" => Invader.City.Default,
						"D@2" => TokenType.Dahan.Default,
						"Z@1" => BacTokens.Disease,
						_ => throw new ArgumentException("invalide tokentype found in "+expectedInvaderSummary)
					};
					desiredTokens.Add(token, int.Parse(part.Substring(0,1)) );
				}
			}

			var tokensToRemove = currentTokens.Keys.Except(desiredTokens.Keys).ToArray();
			foreach(var old in tokensToRemove)
				currentTokens[old] = 0;
			foreach(var p in desiredTokens)
				currentTokens[p.Key] = p.Value;

			currentTokens.Summary.ShouldBe( expectedInvaderSummary );
		}

		public static void ClearAllBlight( this SpiritGameStateCtx ctx ) {
			// So it doesn't cascade and require extra interactions...
			foreach(var space in ctx.AllSpaces) {
				var tmpCtx = ctx.Target(space);
				while(tmpCtx.HasBlight)
					tmpCtx.RemoveBlight();
			}
		}

		public static void ActivateFearCard( this SpiritGameStateCtx ctx, IFearOptions fearCard ) {
			ctx.GameState.Fear.Deck.Pop();
			ctx.GameState.Fear.ActivatedCards.Push( new PositionFearCard{ FearOptions=fearCard, Text="FearCard" } );
		}

		public static void ElevateTerrorLevelTo( this SpiritGameStateCtx ctx, int desiredFearLevel ) {
			while(ctx.GameState.Fear.TerrorLevel < desiredFearLevel)
				ctx.GameState.Fear.Deck.Pop();
		}

		#region Log Asserting

		public static void Assert_Ravaged( this Queue<string> log, params string[] spaces ) {

			log.Dequeue().ShouldStartWith( "Ravaging" );
			foreach(var s in spaces)
				log.Dequeue().ShouldStartWith( s );
		}

		public static void Assert_Built( this Queue<string> log, params string[] spaces ) {

			log.Dequeue().ShouldStartWith( "Building" );
			foreach(var s in spaces)
				log.Dequeue().ShouldStartWith( s );
		}

		public static void Assert_Explored( this Queue<string> log, params string[] spaces ) {
			if(spaces.Length>log.Count)
				throw new System.Exception("Not enough log entries.:" + log.Join(" -- "));

			log.Dequeue().ShouldStartWith( "Exploring" );
			foreach(var s in spaces)
				log.Dequeue().ShouldStartWith( s );
		}

		#endregion
	}

}

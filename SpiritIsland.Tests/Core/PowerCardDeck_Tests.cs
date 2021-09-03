using Shouldly;
using SpiritIsland.Basegame;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SpiritIsland.Tests.Core {

	public class PowerCardDeck_Tests {

		[Fact]
		public void Minor36Count() {
			var minorCards = PowerCard.GetMinors();
			// minorCards.Length.ShouldBe( 36 ); // Basegame
			minorCards.Length.ShouldBeGreaterThanOrEqualTo( 36 );
		}

		[Fact]
		public void Major22Count() {
			var majorCards = PowerCard.GetMajors();
			majorCards.Length.ShouldBe( 22 );
		}

		#region target-Space

		[Fact]
		public void PowerCard_Targets_Space_CorrectParameters() {
			// Since PowerCards and innates use Reflection to map to the Async task, we need to make sure method signature is correct.
			static bool MethodTargetsSpace( MethodBase m ) => m.GetCustomAttributes<TargetSpaceAttribute>().Any();
			var methods = typeof( PowerCard ).Assembly.GetTypes()
				.OrderBy( x => x.Namespace )
				.ThenBy( x => x.Name )
				.SelectMany( x => x.GetMethods() )
				.Where( MethodTargetsSpace )
				.ToArray();

			ShouldHave_TargetSpace_MethodSignature( methods );
		}

		[Fact]
		public void Innate_Targets_Space_CorrectParameters() {
			// Since PowerCards and innates use Reflection to map to the Async task, we need to make sure method signature is correct.

			static bool TypeTargetsSpace( Type m ) => m.GetCustomAttributes<TargetSpaceAttribute>().Any();
			static bool MethodIsOption( MethodBase m ) => m.GetCustomAttributes<InnateOptionAttribute>().Any();
			var methods = typeof( PowerCard ).Assembly.GetTypes()
				.Where( TypeTargetsSpace )
				.OrderBy( x => x.Namespace )
				.ThenBy( x => x.Name )
				.SelectMany( x => x.GetMethods() )
				.Where( MethodIsOption )
				.ToArray();

			ShouldHave_TargetSpace_MethodSignature( methods );

		}

		#endregion

		#region Target - Spirit

		[Fact]
		public void PowerCard_Targets_Spirit_CorrectParameters() {
			// Since PowerCards and innates use Reflection to map to the Async task, we need to make sure method signature is correct.
			static bool MethodTargetsSpace( MethodBase m ) => m.GetCustomAttributes<TargetSpiritAttribute>().Any();
			var methods = typeof( PowerCard ).Assembly.GetTypes()
				.OrderBy( x => x.Namespace )
				.ThenBy( x => x.Name )
				.SelectMany( x => x.GetMethods() )
				.Where( MethodTargetsSpace )
				.ToArray();

			ShouldHave_TargetSpirit_MethodSignature( methods );
		}

		[Fact]
		public void Innate_Targets_Spirit_CorrectParameters() {
			// Since PowerCards and innates use Reflection to map to the Async task, we need to make sure method signature is correct.

			static bool TypeTargetsSpace( Type m ) => m.GetCustomAttributes<TargetSpiritAttribute>().Any();
			static bool MethodIsOption( MethodBase m ) => m.GetCustomAttributes<InnateOptionAttribute>().Any();
			var methods = typeof( PowerCard ).Assembly.GetTypes()
				.Where( x=>x!=typeof( GiftOfStrength ) ) // ! this has a different signature - !! review it and see if it is still relavent
				.Where( TypeTargetsSpace )
				.OrderBy( x => x.Namespace )
				.ThenBy( x => x.Name )
				.SelectMany( x => x.GetMethods() )
				.Where( MethodIsOption )
				.ToArray();

			ShouldHave_TargetSpirit_MethodSignature( methods );

		}

		#endregion

		static void ShouldHave_TargetSpace_MethodSignature( MethodInfo[] methods ) {
			string[] problems = methods.Where( m => {
				var methodParams = m.GetParameters();
				return m.ReturnType.Name != "Task"
					|| methodParams.Length != 1
					|| methodParams[0].Name != "ctx"
					|| methodParams[0].ParameterType.Name != "TargetSpaceCtx";
			} )
				.Select( m => {
					var methodParams = m.GetParameters();
					var paramString = methodParams.Select( p => p.ParameterType.Name + " " + p.Name ).Join( "," );
					return $"{m.ReturnType.Name} {m.DeclaringType.Name}.{m.Name}({paramString})";
				} )
				.ToArray();

			problems.Length.ShouldBe( 0, problems.Take( 5 ).Join( "\r\n" ) );
		}

		static void ShouldHave_TargetSpirit_MethodSignature( MethodInfo[] methods ) {
			string[] problems = methods.Where( m => {
				var methodParams = m.GetParameters();
				return m.ReturnType.Name != "Task"
					|| methodParams.Length != 1
					|| methodParams[0].Name != "ctx"
					|| methodParams[0].ParameterType.Name != "TargetSpiritCtx";
			} )
				.Select( m => {
					var methodParams = m.GetParameters();
					var paramString = methodParams.Select( p => p.ParameterType.Name + " " + p.Name ).Join( "," );
					return $"{m.ReturnType.Name} {m.DeclaringType.Name}.{m.Name}({paramString})";
				} )
				.ToArray();

			problems.Length.ShouldBe( 0, problems.Take( 5 ).Join( "\r\n" ) );
		}



		[Theory]
		[InlineData(true)]
		[InlineData( false )]
		public void DrawingMajor_ForgetACard(bool drawDirect) {
			var spirit = new RiverSurges();
			var gs = new GameState( spirit, Board.BuildBoardC() ) {
				MajorCards = new PowerCardDeck( PowerCard.GetMajors() ),
				MinorCards = new PowerCardDeck( PowerCard.GetMinors() )
			};
			gs.Initialize();

			if(drawDirect)
				_= spirit.DrawMajor(gs);
			else { 
				_= spirit.Draw(gs,null);
				spirit.Action.AssertDecision( "Which type do you wish to draw", "minor,major","major");
			}

			spirit.Action.AssertPrompt_ChooseFirst("Select Major Power Card");
			spirit.Action.AssertPrompt_ChooseFirst( "Select power card to forget" );
		}

	}

}

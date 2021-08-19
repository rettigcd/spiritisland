using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SpiritIsland.Tests {

	public class PowerCardDeck_Tests {

		[Fact]
		public void Minor36Count() {
			var minorCards = PowerCard.GetMinors();
			minorCards.Length.ShouldBe( 36 );
		}

		[Fact]
		public void Major22Count() {
			var majorCards = PowerCard.GetMajors();
			majorCards.Length.ShouldBe( 22 );
		}

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

			ShouldHaveTargetSpaceMethodSignature( methods );

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

			ShouldHaveTargetSpaceMethodSignature( methods );

		}

		static void ShouldHaveTargetSpaceMethodSignature( MethodInfo[] methods ) {
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

	}
}

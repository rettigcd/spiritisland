﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class InnatePower : IFlexibleSpeedActionFactory {

		#region Constructors and factories

		static public InnatePower For<T>(){ 
			Type actionType = typeof(T);
			var contextAttr = actionType.GetCustomAttributes<GeneratesContextAttribute>().VerboseSingle(actionType.Name+" must have Single Target space or Target spirit attribute");
			return new InnatePower( actionType, contextAttr );
		}

		internal InnatePower(Type actionType,GeneratesContextAttribute targetAttr){

			innatePowerAttr = actionType.GetCustomAttribute<InnatePowerAttribute>();
			speedAttr = actionType.GetCustomAttribute<SpeedAttribute>(false) 
				?? throw new InvalidOperationException("Missing Speed attribute for "+actionType.Name);
			this.targetAttr = targetAttr;
			this.repeatAttr = actionType.GetCustomAttribute<RepeatIfAttribute>(); // !! What about more than 1 repeat?

			Name = innatePowerAttr.Name;

			// try static method (spirit / major / minor)
			this.elementListByMethod = actionType
				.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static )
				.Select( m => new MethodTuple(m) )
				.Where( x => x.Attr != null )
				.ToList();

			this._drawableOptions = elementListByMethod
				.Select(x=>x.Attr)
				.Where( o => o.Purpose != AttributePurpose.ExecuteOnly )
				.Cast<IDrawableInnateOption>()
				.ToList();
			if(this.repeatAttr!=null)
				_drawableOptions.AddRange( repeatAttr.Repeats );
		}

		#endregion

		#region Speed

		public Phase Speed => speedAttr.DisplaySpeed;
		public SpeedOverride OverrideSpeed { get; set; }

		public bool CouldActivateDuring( Phase phase, Spirit spirit ) {	// !!! Can we do this somehow without asking them every time if they want to use an element?
			return CouldBeTriggered( spirit )
				&& CouldMatchPhase( phase, spirit );
		}

		bool CouldBeTriggered( Spirit spirit ) {
			return elementListByMethod
				.Any(x=>spirit.CouldHaveElements(x.Elements));
		}

		bool CouldMatchPhase( Phase requestSpeed, Spirit spirit ) {
			return OverrideSpeed != null
				? OverrideSpeed.Speed.IsOneOf( requestSpeed, Phase.FastOrSlow )
				: speedAttr.CouldBeActiveFor( requestSpeed, spirit );
		}

		#endregion

		public string Name {get;}

		public string Text => Name;

		public string TargetFilter => this.targetAttr.TargetFilter;

		public string RangeText => this.targetAttr.RangeText;

		public LandOrSpirit LandOrSpirit => targetAttr.LandOrSpirit;

		public async Task ActivateAsync( SpiritGameStateCtx ctx ) {

			await ActivateInnerAsync( ctx );
			if( await ShouldRepeat(ctx.Self) )
				await ActivateInnerAsync( ctx );
		}

		public CountDictionary<Element>[] GetTriggerThresholds() => elementListByMethod.Select(a=>a.Attr.Elements).ToArray();

		public IEnumerable<InnateOptionAttribute> Options => elementListByMethod.Select(x=>x.Attr);

		public IEnumerable<IDrawableInnateOption> DrawableOptions => _drawableOptions;
		readonly List<IDrawableInnateOption> _drawableOptions;


		async Task ActivateInnerAsync( SpiritGameStateCtx spiritCtx ) {

			// !!! Targetting a space first, then declining all of the elemental upgrades
			// !!! we should resolve elemental upgrades first, then pick the target.

			// if we are using prepared, verify
			if(!await speedAttr.IsActiveFor( spiritCtx.GameState.Phase, spiritCtx.Self )) return;

			List<MethodInfo> lastMethods = await GetLastActivatedMethodsOfEachGroup( spiritCtx );
			if( lastMethods.Count == 0 ) return;

			object targetCtx = await targetAttr.GetTargetCtx( Name, spiritCtx );
			if(targetCtx == null) return;

			foreach(var method in lastMethods)
				await (Task)method.Invoke( null, new object[] { targetCtx } );

		}

		async Task<List<MethodInfo>> GetLastActivatedMethodsOfEachGroup( SpiritGameStateCtx spiritCtx ) {
			IEnumerable<MethodTuple[]> groups = elementListByMethod
				// filter first - so we only have groups that have matches
				.Where( pair => pair.Attr.Purpose != AttributePurpose.DisplayOnly )
				.GroupBy( x => x.Group )
				.Select( x => x.ToArray() );
			List<MethodInfo> lastMethods = new List<MethodInfo>();
			foreach(MethodTuple[] grp in groups) {
				MethodInfo method = await GetLastMethodThatHasElements( spiritCtx.Self, grp );
				if(method != null)
					lastMethods.Add( method );
			}
			return lastMethods;
		}

		private static async Task<MethodInfo> GetLastMethodThatHasElements( Spirit self, MethodTuple[] grp ) {
			MethodInfo method = null;
			foreach(MethodTuple x in grp.OrderBy( pair => pair.Elements.Total ))
				if(await self.HasElements( x.Elements ))
					method = x.Method;
			return method;
		}

		async Task<bool> ShouldRepeat( Spirit spirit ) => repeatAttr != null 
			&& await repeatAttr.GetRepeatCount( spirit ) > 0; // !!! change ShouldRepeat to RepeatCount

		public static string[] Tokenize( string s ) {

			var tokens = new Regex( "sacred site|presence|fast|slow"
				+ "|dahan|blight|fear|city|town|explorer"
				+ "|sun|moon|air|fire|water|plant|animal|earth"
				+ "|beast|disease|strife|wilds|badlands"
				+ "|\\+1range" 
			).Matches( s ).Cast<Match>().ToList();

			var results = new List<string>();

			int cur = 0;
			while(cur < s.Length) {
				// no more tokens, go to the end
				if(tokens.Count == 0) {
					results.Add( s[cur..] );
					break;
				}
				var nextToken = tokens[0];
				if(nextToken.Index == cur) {
					results.Add( "{"+nextToken.Value+"}" );
					cur = nextToken.Index + nextToken.Length;
					tokens.RemoveAt( 0 );
				} else {
					results.Add( s[cur..nextToken.Index] );
					cur = nextToken.Index;
				}
			}
			return results.ToArray();
		}

		readonly InnatePowerAttribute innatePowerAttr;
		readonly protected SpeedAttribute speedAttr;
		readonly GeneratesContextAttribute targetAttr;
		readonly RepeatIfAttribute repeatAttr;
		readonly List<MethodTuple> elementListByMethod;


		class MethodTuple {
			public MethodTuple(MethodInfo m ) {
				Method = m;
				Attr = m.GetCustomAttributes<InnateOptionAttribute>().FirstOrDefault();
			}
			public MethodInfo Method { get; }
			public InnateOptionAttribute Attr { get; }
			public CountDictionary<Element>  Elements => Attr.Elements;
			public int Group => Attr.Group;
		}

	}

	public enum LandOrSpirit { None, Land, Spirit }

}
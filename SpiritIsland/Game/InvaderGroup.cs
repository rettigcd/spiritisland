﻿using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland {

	public class InvaderGroup{

		#region constructor

		public InvaderGroup(Space space, int[] counts, Action<InvaderGroup> onCommit ) {

			this.Space = space;
			this.innerDict = Invader.Lookup.Values
				.ToDictionary( invader => invader, invader => counts[invader.Index] );

			countDict = new CountDictionary<Invader>(innerDict);
			this.onCommit = onCommit;
		}

		#endregion

		readonly Action<InvaderGroup> onCommit;
		public void Commit() {
			onCommit(this); // could this be pulled inside and justmake changes to the dictnionary?
		}

		public void ApplyDamage( Invader invader, int damage ) {
			if(damage > invader.Health) damage = invader.Health;
			var damagedInvader = invader.Damage(damage);
			changed.Add( invader ); --countDict[invader];
			changed.Add( damagedInvader ); ++countDict[damagedInvader];
		}

		// returns as much damage as possible to invader
		// returns actual damage done
		public int ApplyDamageMax( Invader invader, int availableDamage ) {
			int damageToInvader = Math.Min( invader.Health, availableDamage );

			var damagedInvader = invader.Damage( damageToInvader );
			changed.Add( invader );				--countDict[invader];
			changed.Add( damagedInvader  );		++countDict[damagedInvader];

			return damageToInvader;
		}

		public int Destroy( Invader healthy, int max=1 ) => (max == 0) ? 0 : DestroyInner( healthy, max );

		public int DestroyAll( Invader healthy ) => DestroyInner( healthy, int.MaxValue );

		int DestroyInner( Invader healthy, int maxCount ) {
			Invader[] invaderTypesToDestory = this.InvaderTypesPresent
				.Where(i=>i.Healthy==healthy && i.Health!=0)
				.OrderByDescending(x=>x.Health) // kill healthiest first
				.ToArray();

			int totalDestoyed = 0;
			foreach(var invaderType in invaderTypesToDestory) {
				int destroy = Math.Min(maxCount,countDict[invaderType]);
				// remove as many as we can of this type and use up the destroy slots
				countDict[invaderType] -= destroy;
				maxCount -= destroy;
				// mark them destroyed
				totalDestoyed += destroy;
				countDict[invaderType.Dead] += destroy;
			}
			return totalDestoyed;
		}

		#region public Read-Only

		public int this[Invader i] => countDict[i];

		public Invader[] FilterBy( params Invader[] allowedTypes ) => allowedTypes
			.SelectMany(x=>x.AliveVariations)
			.Intersect( InvaderTypesPresent )
			.ToArray();

		public Space Space { get; }

		public bool HasCity => countDict.HasAnyKey( Invader.City, Invader.City2, Invader.City1 );
		public bool HasTown => countDict.HasAnyKey( Invader.Town, Invader.Town1 );
		public bool HasExplorer => countDict.HasAnyKey( Invader.Explorer );
		public bool Has(params Invader[] healthyInvaders) => countDict.Keys.Select(k=>k.Healthy).Intersect( healthyInvaders ).Any();

		public int DestroyedCities => countDict[Invader.City0];
		public int DestroyedTowns => countDict[Invader.Town0];
		public int DestroyedExplorers => countDict[Invader.Explorer0];

		public IEnumerable<Invader> Changed => changed.Where( i => i.Health != 0 );

		/// <summary> Includes damaged invaders.</summary>
		public IEnumerable<Invader> InvaderTypesPresent => innerDict.Keys.Where( invader => invader.Health > 0 );

		public int TypeCount(params Invader[] healthyInvaders) => innerDict
			.Where(pair=>healthyInvaders.Contains(pair.Key.Healthy))
			.Sum(p=>p.Value);

		public int TotalCount => innerDict.Values.Sum();

		public int DamageInflictedByInvaders => innerDict.Select(p=>p.Value * p.Key.Healthy.Health).Sum();

		public override string ToString() {
			static int Order_CitiesTownsExplorers( Invader invader )
				=> -(invader.Healthy.Health * 10 + invader.Health);
			return innerDict
				.Where( pair => pair.Value > 0 && pair.Key.Health > 0 )
				.OrderBy( pair => Order_CitiesTownsExplorers( pair.Key ) )
				.Select( p => p.Value + p.Key.Summary )
				.Join( "," );
		}

		#endregion

		#region private
		readonly Dictionary<Invader, int> innerDict;
		readonly CountDictionary<Invader> countDict;
		readonly HashSet<Invader> changed = new HashSet<Invader>();
		#endregion

	}

}

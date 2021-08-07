﻿using System;
using System.Collections.Generic;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland {

	public class InvaderGroup{

		#region constructor

		public InvaderGroup(Space space, int[] counts, Action<int> addFear ) {

			this.counts = counts; // for committing

			this.Space = space;
			this.innerDict = Invader.Lookup.Values
				.ToDictionary( invader => invader, invader => counts[invader.Index] );

			countDict = new CountDictionary<Invader>(innerDict);
			this.addFear = addFear;
		}
		readonly int[] counts;


		#endregion

		Invader[] KillOrder => killOrder ??= "C@1 C@2 C@3 T@1 T@2 E@1".Split( ' ' ).Select( k => Invader.Lookup[k] ).ToArray();
		Invader[] killOrder;

		Invader[] LeftOverOrder => leftOverOrder ??= "C@2 T@2 C@3".Split( ' ' ).Select( k => Invader.Lookup[k] ).ToArray();
		Invader[] leftOverOrder;

		public void ApplyDamageToInvaders( int startingDamage, List<string> log = null ) {
			int damageToInvaders = startingDamage;

			// While damage remains    &&    we have invaders
			while(damageToInvaders > 0 && InvaderTypesPresent.Any()) {
				var invaderToDamage = KillOrder
					.FirstOrDefault( invader =>
						invader.Health <= damageToInvaders // prefer things we can kill
						&& this[invader] > 0
					)
					?? LeftOverOrder.First( invader => this[invader] > 0 ); // left-over damage

				damageToInvaders -= this.ApplyDamageMax( invaderToDamage, damageToInvaders );

			}
			if(log != null) log.Add( $"{startingDamage} damage to invaders leaving {this}." );
			this.Commit();
		}

		readonly Action<int> addFear;
		public void Commit() {

			addFear( DestroyedCities * 2 );
			addFear( DestroyedTowns );

			foreach(var invader in Changed)
				counts[invader.Index] = this[invader];
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
				changed.Add( invaderType );
				// mark them destroyed
				totalDestoyed += destroy;
				countDict[invaderType.Dead] += destroy;
				changed.Add(invaderType.Dead);
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

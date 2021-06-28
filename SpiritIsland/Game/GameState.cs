﻿using System;
using System.Linq;
using SpiritIsland.Core;

namespace SpiritIsland {

	public class GameState {

		public GameState(params Spirit[] spirits){
			if(spirits.Length==0) throw new ArgumentException("Game must include at least 1 spirit");
			this.Spirits = spirits;
		}

		public Island Island { get; set; }
		public Spirit[] Spirits { get; }

		public void InitBoards() {
			foreach(var board in Island.Boards){
				foreach(var space in board.Spaces){
					var counts = space.StartUpCounts;
					this.Adjust(space,Invader.City,counts.Cities);
					this.Adjust(space,Invader.Town,counts.Towns);
					this.Adjust(space,Invader.Explorer,counts.Explorers);
					this.AddDahan(space,counts.Dahan);
					if(counts.Blight>0) this.AddBlight(space); // add 1
				}
			}
		}

		internal void Defend( Space space, int delta ) {
			defendCount[space] += delta;
		}

		#region Beasts
		public void AddBeast( Space space ){ beastCount[space]++; }
		public bool HasBeasts( Space s ) => beastCount[s] > 0;
		#endregion

		#region Blight

		public void AddBlight( Space space, int delta=1 ){ blightCount[space]+=delta; }
		public bool HasBlight( Space s ) => blightCount[s] > 0;
		public int GetBlightOnSpace( Space space ){ return blightCount[space]; }

		#endregion

		#region Wilds
		public void AddWilds( Space space ){ wildsCount[space]++; }
		public bool HasWilds( Space s ) => wildsCount[s] > 0;
		#endregion

		#region Fear
		public void AddFear(int count) => fearCount += count;
		#endregion

		#region Dahan
		public void AddDahan( Space space, int delta=1 ){ 	dahanCount[space]+=delta;}
		public int GetDahanOnSpace( Space space ){ return dahanCount[space]; }
		public bool HasDahan( Space space ) => GetDahanOnSpace(space)>0;
		#endregion

		#region Invaders

		public bool HasInvaders( Space space ) 
			=> invaderCount.Keys.Any(k=>k.Space==space);

		public void Adjust(Space space, Invader invader, int count){
			invaderCount[Key(space,invader)] += count;
		}

		public InvaderGroup InvadersOn(Space targetSpace) {
			return 
				// invaders ?? 
				InitInvaderGroup(targetSpace);
		}

		public void ApplyDamage(DamagePlan damagePlan) {
			var invaders = InitInvaderGroup(damagePlan.Space);
			invaders.ApplyDamage(damagePlan);
			UpdateFromGroup(invaders);
		}
		void UpdateFromGroup(InvaderGroup invaders){
			foreach(var invader in invaders.Changed)
				this.invaderCount[Key(invaders.Space,invader)] = invaders[invader];
		}

		InvaderGroup InitInvaderGroup(Space targetSpace) {
			var dict1 = invaderCount.Keys
				.Where(k=>k.Space==targetSpace)
				.ToDictionary(k=>k.Invader,k=>invaderCount[k]);
			return new InvaderGroup( targetSpace, dict1 );
		}

		public void Explore( InvaderCard invaderCard ) {
			bool HasTownOrCity(Space space){
				var invaders = InvadersOn(space);
				return invaders.HasTown || invaders.HasCity;
			}
			var exploredSpaces = Island.Boards.SelectMany(board=>board.Spaces)
				.Where(invaderCard.Matches)
				.Where(space=> space.IsCostal
						|| space.SpacesWithin(1).Any(HasTownOrCity)
				);
			foreach(var space in exploredSpaces)
				Adjust( space, Invader.Explorer, 1 );
		}

		public void Build( InvaderCard invaderCard ) {
			var exploredSpaces = Island.Boards.SelectMany(board=>board.Spaces)
				.Where(invaderCard.Matches)
				.Select(InvadersOn)
				.Where(group => group.InvaderTypesPresent.Any());
			foreach(var group in exploredSpaces){
				int townCount = group[Invader.Town] + group[Invader.Town1];
				int cityCount = group[Invader.City] + group[Invader.City2] + group[Invader.City1];
				var invaderToAdd = townCount>cityCount ? Invader.City : Invader.Town;
				Adjust( group.Space, invaderToAdd, 1 );
			}
		}

		Invader[] killOrder;
		Invader[] leftOverOrder;

		public void Ravage( InvaderCard invaderCard ) {
			if(invaderCard == null) return;

			// 1 point of damage,  Prefer C@1  ---  ---  T@1  ---  E@1 >>> C@2, T@2, C@3
			// 2 points of damage, Prefer C@1  C@2  ---  T@1  T@2  E@1 >>> C@3
			// 3 points of damage, Prefer C@1  C@2  C@3  T@1  T@2  E@1
			if(killOrder==null){
				killOrder = "C@1 C@2 C@3 T@1 T@2 E@1".Split(' ').Select(k=>Invader.Lookup[k]).ToArray();
				leftOverOrder = "C@2 T@2 C@3".Split(' ').Select(k=>Invader.Lookup[k]).ToArray();
			}

			var ravageGroups = Island.Boards.SelectMany(board=>board.Spaces)
				.Where(invaderCard.Matches)
				.Select(InvadersOn)
				.Where(group => group.InvaderTypesPresent.Any());

			foreach(var ravageGroup in ravageGroups)
				RavageSpace( ravageGroup );

		}

		void RavageSpace( InvaderGroup ravageGroup ) {

			int defend = defendCount[ravageGroup.Space]; defendCount[ravageGroup.Space] = 0;
			int damageToDahan = Math.Max( ravageGroup.DamageInflicted - defend, 0);

			int dahan = GetDahanOnSpace( ravageGroup.Space );
			int dahanKilled = Math.Min( damageToDahan / 2, dahan ); // rounding down
			AddDahan( ravageGroup.Space, -dahanKilled ); dahan -= dahanKilled;

			ApplyDamageToInvaders( ravageGroup, dahan * 2 );
		}

		public void DamageInvaders(Space space,int damage){
			var group = InvadersOn(space);
			ApplyDamageToInvaders(group,damage);
		}

		void ApplyDamageToInvaders( InvaderGroup ravageGroup, int startingDamage ) {
			int damageToInvaders = startingDamage;
			while(damageToInvaders > 0 && ravageGroup.InvaderTypesPresent.Any()) {
				var invaderToDamage = killOrder.FirstOrDefault( invader =>
						invader.Health <= damageToInvaders // prefer things we can kill
						&& ravageGroup[invader] > 0
					)
					?? leftOverOrder.First( invader => ravageGroup[invader] > 0 ); // left-over damage
				ravageGroup[invaderToDamage]--;
				damageToInvaders -= invaderToDamage.Health;
			}
			UpdateFromGroup( ravageGroup );
		}

		static InvaderKey Key(Space space,Invader invader) => new InvaderKey{ Invader=invader, Space=space};

		struct InvaderKey : IEquatable<InvaderKey> {
			public Space Space;
			public Invader Invader;
			public bool Equals( InvaderKey other ) => Space==other.Space && Invader==other.Invader;
			public override bool Equals( object obj ) => Equals((InvaderKey)obj);
			public override int GetHashCode() => Space.GetHashCode() ^ Invader.GetHashCode();
		}


		#endregion

		readonly CountDictionary<Space> blightCount = new CountDictionary<Space>();
		readonly CountDictionary<Space> beastCount = new CountDictionary<Space>();
		readonly CountDictionary<Space> wildsCount = new CountDictionary<Space>();

		readonly CountDictionary<InvaderKey> invaderCount = new CountDictionary<InvaderKey>();
		readonly CountDictionary<Space> dahanCount = new CountDictionary<Space>();

		readonly CountDictionary<Space> defendCount = new CountDictionary<Space>();
		int fearCount = 0;
	}

}

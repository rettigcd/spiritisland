﻿namespace SpiritIsland {
	public class Invader : IOption {

		static readonly Invader[] Cities = new Invader[4]; // 0..3
		static readonly Invader[] Towns = new Invader[3];  // 0..2
		static readonly Invader[] Explorers = new Invader[2];  // 0..1

		// Healthy
		static readonly public Invader City      = new Invader("City",Cities,3); 
		static readonly public Invader City2     = new Invader("City",Cities,2); // damaged
		static readonly public Invader City1     = new Invader("City",Cities,1); // damaged
		static readonly public Invader City0     = new Invader("City",Cities,0); // DESTROYED
		static readonly public Invader Town      = new Invader("Town",Towns,2); 
		static readonly public Invader Town1     = new Invader("Town",Towns,1); // damaged
		static readonly public Invader Town0     = new Invader("Town",Towns,0); // DESTROYED
		static readonly public Invader Explorer  = new Invader("Explorer",Explorers,1);
		static readonly public Invader Explorer0 = new Invader("Explorer",Explorers,0);// DESTROYED

		#region private

		Invader(string label, Invader[] typeArr, int health){
			Label = label;
			this.typeArr = typeArr;
			Health = health;
			typeArr[Health] = this;
		}

		readonly Invader[] typeArr;

		#endregion

		public string Summary => Initial+"@"+Health; // C@3, T@2

		public Invader Damage(int level){
			return level > Health 
				? throw new System.ArgumentOutOfRangeException() 
				: typeArr[Health-level];
		}

		public int Health {get;}
		public Invader Healthy => typeArr[typeArr.Length-1];

		public char Initial => Label[0];

		public string Label { get; }

		string IOption.Text =>  Summary; // + health ?
	}

}

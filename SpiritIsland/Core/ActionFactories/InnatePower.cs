﻿using System;
using System.Linq;

namespace SpiritIsland.Core {

	public class InnatePower : IActionFactory {

		static public InnatePower For<T>(){ return new InnatePower(typeof(T));}

		public InnatePower(Type actionType){

			var pca = System.Attribute.GetCustomAttributes(actionType)
				.OfType<InnatePowerAttribute>()
				.ToArray();
			if(pca.Length==0) throw new ArgumentException(actionType.Name+" missing PowerCard attribute.");
			if(pca.Length!=1) throw new ArgumentException(actionType.Name+" has multiple PowerCard attributes.");
			var attr = pca[0];

			Speed = attr.Speed;
			Name = attr.Name;

			this.actionType = actionType;
		}

		public Speed Speed {get;}

		public string Name {get;}

		public string Text => Name;

		readonly Type actionType;

		public IAction Bind( Spirit spirit, GameState gameState ) {
			return (IAction)Activator.CreateInstance(actionType,spirit,gameState);
		}

	}

}
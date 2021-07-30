using System;

namespace SpiritIsland.Core {
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
    public class FearLevelAttribute : Attribute {
        public FearLevelAttribute(int level, string description ) { }
    }

}


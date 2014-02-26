using System;
using RunUO;
using RunUO.Mobiles;
using RunUO.Gumps;
using System.Collections.Generic;

namespace RunUO.Engines.MLQuests.Rewards
{
	public class DummyReward : BaseReward
	{
		public DummyReward( TextDefinition name )
			: base( name )
		{
		}

		protected override int LabelHeight { get { return 180; } }

		public override void AddRewardItems( PlayerMobile pm, List<Item> rewards )
		{
		}
	}
}

using System;
using System.Collections.Generic;
using RunUO.Items;

namespace RunUO.Mobiles
{
	public abstract class SBInfo
	{
		public static readonly List<SBInfo> Empty = new List<SBInfo>();

		public SBInfo()
		{
		}

		public abstract IShopSellInfo SellInfo { get; }
		public abstract List<GenericBuyInfo> BuyInfo { get; }
	}
}

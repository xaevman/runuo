using System;
using RunUO;
using RunUO.Items;
using RunUO.Mobiles;
using RunUO.Engines.Quests;
using RunUO.Engines.Quests.Necro;

namespace RunUO.Engines.Quests.Necro
{
	public class DarkTidesHorn : HornOfRetreat
	{
		public override bool ValidateUse( Mobile from )
		{
			PlayerMobile pm = from as PlayerMobile;

			return ( pm != null && pm.Quest is DarkTidesQuest );
		}

		[Constructable]
		public DarkTidesHorn()
		{
			DestLoc = new Point3D( 2103, 1319, -68 );
			DestMap = Map.Malas;
		}

		public DarkTidesHorn( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}
using System;
using RunUO;
using RunUO.Spells.First;
using RunUO.Targeting;

namespace RunUO.Items
{
	public class MagicArrowWand : BaseWand
	{
		[Constructable]
		public MagicArrowWand() : base( WandEffect.MagicArrow, 5, Core.ML ? 109 : 30 )
		{
		}

		public MagicArrowWand( Serial serial ) : base( serial )
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

		public override void OnWandUse( Mobile from )
		{
			Cast( new MagicArrowSpell( from, this ) );
		}
	}
}
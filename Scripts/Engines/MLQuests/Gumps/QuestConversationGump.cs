using System;
using RunUO;
using RunUO.Gumps;
using RunUO.Mobiles;
using RunUO.Network;

namespace RunUO.Engines.MLQuests.Gumps
{
	public class QuestConversationGump : BaseQuestGump
	{
		public QuestConversationGump( MLQuest quest, PlayerMobile pm, TextDefinition text )
			: base( 3006156 ) // Quest Conversation
		{
			CloseOtherGumps( pm );

			SetTitle( quest.Title );
			RegisterButton( ButtonPosition.Right, ButtonGraphic.Close, 3 );

			SetPageCount( 1 );

			BuildPage();
			AddConversation( text );
		}
	}
}

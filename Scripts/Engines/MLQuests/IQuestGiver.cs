using System;
using System.Collections.Generic;
using RunUO;

namespace RunUO.Engines.MLQuests
{
	public interface IQuestGiver
	{
		List<MLQuest> MLQuests { get; }

		Serial Serial { get; }
		bool Deleted { get; }

		Type GetType();
	}
}

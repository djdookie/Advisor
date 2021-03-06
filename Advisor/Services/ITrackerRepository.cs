﻿using System.Collections.Generic;
using HDT.Plugins.Advisor.Models;

namespace HDT.Plugins.Advisor.Services
{
	public interface ITrackerRepository
	{
		bool IsInMenu();

		Deck GetOpponentDeck();

		string GetGameNote();

		string GetGameMode();

		void UpdateGameNote(string text);

		List<ArchetypeDeck> GetAllArchetypeDecks();

		void AddDeck(Deck deck);

		void AddDeck(string name, string playerClass, string cards, bool archive, params string[] tags);

        void AddDeck(string name, Hearthstone_Deck_Tracker.Hearthstone.Deck deck, bool archive, params string[] tags);

        int DeleteAllDecksWithTag(string tag);
	}
}
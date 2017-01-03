using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace HDT.Plugins.Advisor
{
    public class AdvisorPlugin : IPlugin
    {
        private CardList cardList;

        public string Author
        {
            get { return "Dookie"; }
        }

        public string ButtonText
        {
            get { return "Settings"; }
        }

        public string Description
        {
            get { return "This plugin tries to guess the opponent's deck while playing and shows it's supposed cards."; }
        }

        public MenuItem MenuItem
        {
            get { return null; }
        }

        public string Name
        {
            get { return "Advisor"; }
        }

        public void OnButtonPress()
        {
        }

        public void OnLoad()
        {
			cardList = new CardList();
			Core.OverlayCanvas.Children.Add(cardList);
			Advisor advisor = new Advisor(cardList);

			GameEvents.OnGameStart.Add(advisor.GameStart);
			GameEvents.OnInMenu.Add(advisor.InMenu);
			GameEvents.OnTurnStart.Add(advisor.TurnStart);
        }

        public void OnUnload()
        {
            Core.OverlayCanvas.Children.Remove(cardList);
        }

        public void OnUpdate()
        {
        }

        public Version Version
        {
            get { return new Version(0, 0, 1); }
        }
    }
}

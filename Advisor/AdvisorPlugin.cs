using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Hearthstone;
using HDT.Plugins.Advisor.Layout;
using Advisor.Properties;

namespace HDT.Plugins.Advisor
{
    public class AdvisorPlugin : IPlugin
    {
        private MenuItem _menu;
        private AdvisorOverlay _advisorOverlay;
        Advisor advisor;
       // IEnumerable<Card> revealedCards;

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
            get
            {
                if (_menu == null)
                    _menu = new AdvisorMenu();
                return _menu;
            }
        }

        public string Name
        {
            get { return "Advisor"; }
        }

        public void OnButtonPress()
        {
            Advisor.ShowSettings();
        }

        public void OnLoad()
        {
			_advisorOverlay = new AdvisorOverlay();
			Core.OverlayCanvas.Children.Add(_advisorOverlay);
			advisor = new Advisor(_advisorOverlay);

            GameEvents.OnInMenu.Add(advisor.InMenu);
            GameEvents.OnGameStart.Add(advisor.GameStart);
            //GameEvents.OnTurnStart.Add(advisor.TurnStart);
            GameEvents.OnOpponentPlay.Add(advisor.OpponentPlay);
            GameEvents.OnOpponentSecretTriggered.Add(advisor.OpponentSecretTiggered);
            GameEvents.OnOpponentDeckDiscard.Add(advisor.OpponentDeckDiscard);
            GameEvents.OnOpponentDeckToPlay.Add(advisor.OpponentDeckToPlay);
            GameEvents.OnOpponentHandDiscard.Add(advisor.OpponentHandDiscard);
            GameEvents.OnOpponentJoustReveal.Add(advisor.OpponentJoustReveal);
        }

        public void OnUnload()
        {
            Core.OverlayCanvas.Children.Remove(_advisorOverlay);
            //Settings.Default.Save();
            
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

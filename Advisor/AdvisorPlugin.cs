using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using Advisor.Properties;
using HDT.Plugins.Advisor.Layout;
using HDT.Plugins.Advisor.Services;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace HDT.Plugins.Advisor
{
    public class AdvisorPlugin : IPlugin
    {
        private AdvisorOverlay _advisorOverlay;
        private MenuItem _menu;

        private Advisor advisor;

        public string Author => "Dookie";

        public string ButtonText => "Settings";

        public string Description => "This plugin tries to guess the opponent's deck while playing and shows it's supposed cards.";

        public MenuItem MenuItem
        {
            get
            {
                if (_menu == null)
                {
                    _menu = new AdvisorMenu();
                }

                return _menu;
            }
        }

        public string Name => "Advisor";

        public void OnButtonPress()
        {
            Advisor.ShowSettings();
        }

        public async void OnLoad()
        {
            // Small delay to guarantee all game variables are set correctly by now (especially CoreAPI.Game.IsInMenu)
            await Task.Delay(2000);

            _advisorOverlay = new AdvisorOverlay();
            Core.OverlayCanvas.Children.Add(_advisorOverlay);
            advisor = new Advisor(_advisorOverlay);

            // Check for updates
            if (Settings.Default.CheckForUpdates)
            {
                await CheckForUpdate();
            }

            GameEvents.OnInMenu.Add(advisor.InMenu);
            GameEvents.OnGameStart.Add(advisor.GameStart);
            GameEvents.OnOpponentPlay.Add(advisor.OpponentPlay);
            GameEvents.OnOpponentSecretTriggered.Add(advisor.OpponentSecretTiggered);
            GameEvents.OnOpponentDeckDiscard.Add(advisor.OpponentDeckDiscard);
            GameEvents.OnOpponentDeckToPlay.Add(advisor.OpponentDeckToPlay);
            GameEvents.OnOpponentHandDiscard.Add(advisor.OpponentHandDiscard);
            GameEvents.OnOpponentJoustReveal.Add(advisor.OpponentJoustReveal);
            // TODO: How to prevent from multiple GameEvent registrations we disabling and reenabling plugins? See: https://github.com/HearthSim/Hearthstone-Deck-Tracker/issues/3079
        }

        public void OnUnload()
        {
            Core.OverlayCanvas.Children.Remove(_advisorOverlay);
        }

        public void OnUpdate()
        {
        }

        public Version Version => new Version(1, 0, 12);

        public async Task CheckForUpdate()
        {
            var latest = await Github.CheckForUpdate("kimsey0", "Advisor", Version);
            if (latest != null)
            {
                Advisor.Notify("Plugin update available", $"[DOWNLOAD]({latest.html_url}) Advisor {latest.tag_name}", 0,
                    "download", () => Process.Start(latest.html_url));
                Log.Info("Update available: " + latest.tag_name, "Advisor");
            }
        }
    }
}
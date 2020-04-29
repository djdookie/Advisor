using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using HDT.Plugins.Advisor.Layout;
using HDT.Plugins.Advisor.Properties;
using HDT.Plugins.Advisor.Services;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace HDT.Plugins.Advisor
{
    public class AdvisorPlugin : IPlugin
    {
        private Advisor _advisor;
        private AdvisorOverlay _advisorOverlay;
        private MenuItem _menu;

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
            _advisor = new Advisor(_advisorOverlay);

            // Check for updates
            if (Settings.Default.CheckForUpdates)
            {
                await CheckForUpdate();
            }

            GameEvents.OnInMenu.Add(_advisor.InMenu);
            GameEvents.OnGameStart.Add(_advisor.GameStart);
            GameEvents.OnOpponentPlay.Add(_advisor.OpponentPlay);
            GameEvents.OnOpponentSecretTriggered.Add(_advisor.OpponentSecretTiggered);
            GameEvents.OnOpponentDeckDiscard.Add(_advisor.OpponentDeckDiscard);
            GameEvents.OnOpponentDeckToPlay.Add(_advisor.OpponentDeckToPlay);
            GameEvents.OnOpponentHandDiscard.Add(_advisor.OpponentHandDiscard);
            GameEvents.OnOpponentJoustReveal.Add(_advisor.OpponentJoustReveal);
            // TODO: How to prevent from multiple GameEvent registrations we disabling and reenabling plugins? See: https://github.com/HearthSim/Hearthstone-Deck-Tracker/issues/3079
        }

        public void OnUnload()
        {
            Core.OverlayCanvas.Children.Remove(_advisorOverlay);
        }

        public void OnUpdate()
        {
        }

        public Version Version => new Version(1, 0, 13);

        public async Task CheckForUpdate()
        {
            var latest = await Github.CheckForUpdate("kimsey0", "Advisor", Version);
            if (latest != null)
            {
                Advisor.Notify("Plugin update available", $"[DOWNLOAD]({latest.HtmlUrl}) Advisor {latest.TagName}", 0,
                    "download", () => Process.Start(latest.HtmlUrl));
                Log.Info("Update available: " + latest.TagName, "Advisor");
            }
        }
    }
}
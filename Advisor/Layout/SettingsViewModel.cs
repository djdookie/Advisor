using GalaSoft.MvvmLight;
using HDT.Plugins.Advisor.Properties;

namespace HDT.Plugins.Advisor.Layout
{
    public class SettingsViewModel : ViewModelBase
    {
        private Settings _settings;

        public SettingsViewModel()
        {
            Settings = Settings.Default;
        }

        public Settings Settings
        {
            get => _settings;
            set { Set(() => Settings, ref _settings, value); }
        }
    }
}
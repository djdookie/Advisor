using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using HDT.Plugins.Advisor.Properties;
using Hearthstone_Deck_Tracker;
using Newtonsoft.Json;

namespace HDT.Plugins.Advisor
{
    public class AdvisorSettingsProvider : SettingsProvider, IApplicationSettingsProvider
    {
        private static readonly string SettingsPath = Path.Combine(Config.Instance.ConfigDir, "AdvisorSettings.json");

        public override string ApplicationName
        {
            get => Assembly.GetExecutingAssembly().GetName().Name;
            set { }
        }

        public override string Name => nameof(AdvisorSettingsProvider);

        public SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property)
        {
            throw new NotImplementedException();
        }

        public void Reset(SettingsContext context)
        {
            try
            {
                File.Delete(SettingsPath);
            }
            catch (IOException)
            {
            }
        }

        public void Upgrade(SettingsContext context, SettingsPropertyCollection properties)
        {
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            var settingsPropertyValueCollection = new SettingsPropertyValueCollection();

            Dictionary<string, object> values = null;
            try
            {
                using (var file = File.OpenText(SettingsPath))
                {
                    var serializer = new JsonSerializer();
                    values = (Dictionary<string, object>) serializer.Deserialize(file, typeof(Dictionary<string, object>));
                }
            }
            catch (IOException)
            {
            }

            foreach (SettingsProperty settingsProperty in collection)
            {
                var value = new SettingsPropertyValue(settingsProperty);
                
                if (values != null && values.TryGetValue(settingsProperty.Name, out var serializedValue))
                {
                    value.SerializedValue = serializedValue;
                }

                settingsPropertyValueCollection.Add(value);
            }

            return settingsPropertyValueCollection;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            var values = collection.Cast<SettingsPropertyValue>().ToDictionary(v => v.Name, v => v.SerializedValue);
            using (var file = File.CreateText(SettingsPath))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, values);
            }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name ?? nameof(AdvisorSettingsProvider), config);
        }

        public static void Apply()
        {
            var provider = new AdvisorSettingsProvider();

            Settings.Default.Providers.Add(provider);
            foreach (SettingsProperty prop in Settings.Default.Properties)
            {
                prop.Provider = provider;
            }

            Settings.Default.Reload();
        }
    }
}
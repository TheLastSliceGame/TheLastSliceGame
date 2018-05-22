using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;

namespace TheLastSlice
{
    public class AppInsightsClient
    {
        private TelemetryClient TelemetryClient { get; set; }
        private Guid UserGuid { get; set; }
        private Guid SessionGuid { get; set; }
        private DateTime StartTime { get; set; }

        public AppInsightsClient()
        {
            TelemetryClient = new TelemetryClient();
            SessionGuid = System.Guid.NewGuid();
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Object value = localSettings.Values["UserGUID"];

            if (value == null)
            {
                UserGuid = System.Guid.NewGuid();
                localSettings.Values["UserGUID"] = UserGuid;
            }
            else
            {
                UserGuid = (Guid)value;
            }

            StartTime = DateTime.Now;
        }

        private Dictionary<string, string> GetPropertiesDictionary()
        {
            return new Dictionary<string, string>
            {
                { "UserGuid", UserGuid.ToString() },
                { "SessionGuid", SessionGuid.ToString() },
            };
        }

        public void GameOver(GameOverReason reason)
        {
            var properties = GetPropertiesDictionary();
            properties.Add("GameOverReason", reason.ToString());
           
            var metrics = new Dictionary<string, double>
            {
                { "Num Levels", TheLastSliceGame.LevelManager.Levels.Count },
                { "Seconds Played", (DateTime.Now - StartTime).TotalSeconds },
            };

            TelemetryClient.TrackEvent("GameOver", properties, metrics);
        }

        public void LevelComplete(int level, DateTime secondsToBeat)
        {
            var properties = GetPropertiesDictionary();
            properties.Add("Level", level.ToString());

            var metrics = new Dictionary<string, double>
            {
                { "Seconds To Beat Level", (DateTime.Now - secondsToBeat).TotalSeconds },
            };

            TelemetryClient.TrackEvent("LevelComplete", properties, metrics);
        }

        public void PostScoreSuccess()
        {
            var metrics = new Dictionary<string, double>
            {
            };

            TelemetryClient.TrackEvent("PostScoreSuccess", GetPropertiesDictionary(), metrics);
        }
    }
}

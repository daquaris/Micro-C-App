using Sentry;
using System;
using System.Collections.Generic;

namespace micro_c_app_maui
{
    public static class AnalyticsService
    {
        public static void Track(string name, string value)
        {
            Track(name, "value", value);
        }

        public static void Track(string name, params string[] values)
        {
            if (!SettingsPage.AnalyticsEnabled())
            {
                return;
            }

            if (values.Length % 2 != 0)
            {
                System.Diagnostics.Debug.WriteLine("Error: AnalyticsService.Track parameter 'values' must be an even total!");
            }

            var props = new Dictionary<string, string>();
            for (int i = 0; i < values.Length - 1; i += 2)
            {
                props[values[i]] = values[i + 1];
            }

            SentrySdk.AddBreadcrumb(message: name, category: "track", data: props);
        }

        public static void TrackError(Exception e, string value)
        {
            TrackError(e, "value", value);
        }

        public static void TrackError(Exception e, params string[] values)
        {
            if (!SettingsPage.AnalyticsEnabled())
            {
                return;
            }

            if (values.Length % 2 != 0)
            {
                System.Diagnostics.Debug.WriteLine("Error: AnalyticsService.TrackError parameter 'values' must be an even total!");
            }

            SentrySdk.CaptureException(e, scope =>
            {
                for (int i = 0; i < values.Length - 1; i += 2)
                {
                    scope.SetExtra(values[i], values[i + 1]);
                }
            });
        }
    }
}

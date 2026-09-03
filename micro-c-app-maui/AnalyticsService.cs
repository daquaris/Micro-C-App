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

            if (values == null || values.Length % 2 != 0)
            {
                // A caller passed a name with no matching value (or an odd trailing one), or an
                // explicit null for the params array. Silently truncating and sending the rest used
                // to hide the bug in production telemetry - drop the call instead so it's obviously
                // missing rather than quietly wrong (or, for null, a crash).
                System.Diagnostics.Debug.WriteLine("Error: AnalyticsService.Track parameter 'values' must be an even total!");
                return;
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

            if (values != null && values.Length % 2 != 0)
            {
                // Unlike Track (breadcrumb-only), the exception itself is the point here - drop the
                // trailing unmatched value but still capture the exception rather than losing the
                // whole report over a malformed extras call.
                System.Diagnostics.Debug.WriteLine("Error: AnalyticsService.TrackError parameter 'values' must be an even total!");
            }

            SentrySdk.CaptureException(e, scope =>
            {
                if (values == null)
                {
                    return;
                }

                for (int i = 0; i < values.Length - 1; i += 2)
                {
                    scope.SetExtra(values[i], values[i + 1]);
                }
            });
        }
    }
}

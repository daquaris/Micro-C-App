using MicroCLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace micro_c_app_maui.Models
{
    // Local, file-based save/load for PC builds (issue #19). Deliberately doesn't reuse the classic
    // app's ExportPage/ImportPage flow, which round-trips through the dataflare.bbarrett.me proxy -
    // that infra is exactly what's flaky (see issue #30), and there's no reason a save/load feature
    // needs a network round trip at all.
    public class SavedBuild
    {
        public string Name { get; set; } = "";
        public DateTime SavedAt { get; set; }
        public List<BuildComponent> Components { get; set; } = new();

        const string DIRECTORY_NAME = "SavedBuilds";
        static string Directory => Path.Combine(FileSystem.AppDataDirectory, DIRECTORY_NAME);

        static string PathFor(string name) => Path.Combine(Directory, $"{SanitizeFileName(name)}.json");

        static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
        }

        public static List<string> ListSavedNames()
        {
            try
            {
                if (!System.IO.Directory.Exists(Directory))
                {
                    return new List<string>();
                }

                return System.IO.Directory.GetFiles(Directory, "*.json")
                    .Select(f => Path.GetFileNameWithoutExtension(f))
                    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public static void Save(string name, IEnumerable<BuildComponent> components)
        {
            System.IO.Directory.CreateDirectory(Directory);
            var build = new SavedBuild
            {
                Name = name,
                SavedAt = DateTime.Now,
                Components = components.Where(c => c.Item != null).ToList(),
            };
            File.WriteAllText(PathFor(name), JsonSerializer.Serialize(build));
        }

        public static SavedBuild? Load(string name)
        {
            try
            {
                var path = PathFor(name);
                if (!File.Exists(path))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<SavedBuild>(File.ReadAllText(path));
            }
            catch
            {
                return null;
            }
        }

        public static void Delete(string name)
        {
            try
            {
                var path = PathFor(name);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Best-effort - nowhere useful to surface a delete failure from here.
            }
        }
    }
}

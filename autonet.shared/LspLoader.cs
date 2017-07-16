using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using autonet.Extensions;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Common;
using App = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

namespace autonet.lsp {
    public static class LspLoader {
        /// <summary>
        ///     Load a lsp file, any version is default.
        /// </summary>
        /// <param name="name">Part of the file name, with or without extension.</param>
        /// <param name="version">Specific version, otherwise latest.</param>
        /// <returns></returns>
        public static bool Load(string name, int version = -1) {
            var @base = Paths.ConfigDirectory.SubFolder("lsp").EnsureCreated();

            try {
                var res = GetResource(name, version);
                var file = @base.SubFile(res.FileName).FullName.Replace('\\', '/');
                File.WriteAllText(file, res.Content, Encoding.UTF8);
                LoadFile(file);
                return true;
            } catch (Exception e) {
                Debug.WriteLine(e);
                return false;
            }
        }

        public static List<string> AlreadyLoaded { get; } = new List<string>();

        internal static ResourceInfo GetResource(string name, int version = -1) {
            name = Path.ChangeExtension(name, null) + (version == -1 ? "" : $".{version}");
            var asm = Assembly.GetCallingAssembly();
            string target = null;
            var targets = asm.GetManifestResourceNames()
                              ?
                              .Where(mrn => CultureInfo.InvariantCulture.CompareInfo.IndexOf(mrn, name, CompareOptions.IgnoreCase) >= 0)
                              .Where(mrn => AlreadyLoaded.Contains(mrn) == false)
                              .ToArray() ?? new string[0];

            if (targets.Length == 0)
                throw new FileNotFoundException($"Could not find a resource that contains the name '{name}'");
            if (targets.Length > 1) {
                //resolve versions.
                if (targets.All(t => t.Replace(".lsp", "").Contains('.')) == false)
                    throw new FileNotFoundException($"Could not resolve resource: '{name}'\nVersus:\n{string.Join("\n", targets)}");
                target = targets.OrderByDescending(t => {
                        var _i = t.Replace(".lsp", "").Split('.').Last();
                        if (_i.All(char.IsDigit) == false)
                            throw new FileNotFoundException($"Could not resolve resource: '{name}'\nVersus:\n{string.Join("\n", targets)}");
                        return int.Parse(_i);
                    })
                    .FirstOrDefault();
            } else if (targets.Length == 1) {
                target = targets[0];
            }
            if (AlreadyLoaded.Contains(target))
                return null;
            AlreadyLoaded.Add(target);

            using (var sr = new StreamReader(asm.GetManifestResourceStream(target)))
                return new ResourceInfo() {FileName = target, Content = sr.ReadToEnd()};
        }

        /// <summary>
        /// sends a Load command into autocad!
        /// </summary>
        /// <param name="fullpath"></param>
        public static void LoadFile(string fullpath) {
            try {
                App.DocumentManager.MdiActiveDocument.SendStringToExecute($"(load \"{fullpath.Replace("\\", "/")}\") ", true, false, true);
            } catch (Exception e) {
                throw new Exception($"Failed loading file {fullpath} onto autocad", e);
            }
        }

        public class ResourceInfo {
            public string FileName { get; set; }
            public string Content { get; set; }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using autonet.Extensions;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Common;
using App = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;
using YourCAD.Utilities;

namespace autonet.lsp {
    public static class LspLoader {
        /// <summary>
        ///     Load a lsp file, any version is default from calling assembly
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
            }
            catch (Exception e) {
                Debug.WriteLine(e);
                return false;
            }
        }

        public static List<string> AlreadyLoaded { get; } = new List<string>();

        internal static ResourceInfo GetResource(string name, int version = -1) {
            string ReplaceCaseInsensitive(string input, string search, string replacement) {
                string result = Regex.Replace(
                    input,
                    Regex.Escape(search),
                    replacement.Replace("$", "$$"),
                    RegexOptions.IgnoreCase
                );
                return result;
            }
            name = Path.ChangeExtension(name, null) + (version == -1 ? "" : $".{version}");
            var asms = AppDomain.CurrentDomain.GetAssemblies().Union(new[] {Assembly.GetCallingAssembly()}).Distinct();
            Resource target = null;
            var targets = asms
                .SelectMany(asm => {
                    try {
                        return asm.GetManifestResourceNames().Select(rs => new Resource(asm, rs));
                    } catch (NotSupportedException) { }
                    return new Resource[0];
                })
                .Where(res => res.Contains(name))
                .ToArray();

            if (targets.Length == 0)
                throw new FileNotFoundException($"Could not find a resource that contains the name '{name}'");
            if (targets.Length > 1) {
                //resolve versions.
                if (targets.All(t => ReplaceCaseInsensitive(t.ResourceName, ".lsp", "").Contains('.')) == false)
                    throw new FileNotFoundException($"Could not resolve resource: '{name}'\nVersus:\n{string.Join("\n", targets.Select(t => t.ResourceName))}");
                target = targets.OrderByDescending(t => {
                        var _i = ReplaceCaseInsensitive(t.ResourceName,".lsp", "").Split('.').Last();
                        if (_i.All(char.IsDigit) == false)
                            throw new FileNotFoundException($"Could not resolve resource: '{name}'\nVersus:\n{string.Join("\n", targets.Select(tt => tt.ResourceName))}");
                        return int.Parse(_i);
                    })
                    .FirstOrDefault();
            }
            else if (targets.Length == 1) {
                target = targets[0];
            }
            if (AlreadyLoaded.Contains(target.ResourceName))
                throw new InvalidOperationException($"{target.ResourceName} is already loaded!");
            AlreadyLoaded.Add(target.ResourceName);
            return new ResourceInfo() {FileName = target.ResourceName, Content = target.ReadResource()};
        }

        private class Resource {
            public Resource(Assembly asm, string resname) {
                Asm = asm;
                ResourceName = resname;
            }

            public Assembly Asm { get; }
            public string ResourceName { get; }

            public string ReadResource() {
                using (var sr = new StreamReader(Asm.GetManifestResourceStream(ResourceName)))
                    return sr.ReadToEnd();
            }

            /// <summary>
            ///     Selects where resource contains <see cref="resname"/> in it's name.
            /// </summary>
            /// <param name="resname"></param>
            public bool Contains(string resname) {
                return CultureInfo.InvariantCulture.CompareInfo.IndexOf(ResourceName, resname, CompareOptions.IgnoreCase) >= 0;
            }
        }

        /// <summary>
        /// sends a Load command into autocad!
        /// </summary>
        /// <param name="fullpath"></param>
        public static void LoadFile(string fullpath) {
            try {
#if V2013
                CommandLineHelper.ExecuteStringOverInvoke($"(load \"{fullpath.Replace("\\", "/")}\") ");
#else
                App.DocumentManager.MdiActiveDocument.SendStringToExecute($"(load \"{fullpath.Replace("\\", "/")}\") ", true, false, true);
#endif
            }
            catch (Exception e) {
                throw new Exception($"Failed loading file {fullpath} onto autocad", e);
            }
        }

        public class ResourceInfo {
            public string FileName { get; set; }
            public string Content { get; set; }
        }
    }
}
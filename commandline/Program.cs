using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using autonet.Extensions;
using autonet.lsp;
using autonet.Settings;

namespace commandline
{
    class Program
    {
        static void Main(string[] args) {
            SystemSounds.Asterisk.Play();
            /*var n = new SettingsBag("./lol.json");
            n["what"] = 1.2d;
            n["wha2t"] = 1.2d;
            n["wha3t"] = new string[] {"inception nigga"};
            n.Save();*/
            var p = new[]{"A/B/lol.exe","C/B/lol.exe"}.Distinct().ToArray();
            var dups = (from x in p
                group x by Path.GetFileNameWithoutExtension(x) into grouped
                where grouped.Count() > 1
                select grouped).ToArray();
            foreach (IGrouping<string, string> grp in dups) {
                var paths = grp.ToArray();

                string[] toparts(string path) => path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)
                    .Reverse()
                    .ToArray();

                /*foreach (var path in paths) {
                    var parts1 = toparts(path);
                    foreach (var otherpath in paths.Except(new []{path})) {
                        otherpath
                    }
                }

                for (var pindex = 0; pindex < paths.Length; pindex++) { //testing this part.
                    var path = paths[pindex];
                    for (var partindex = 0; partindex < parts[pindex].Length; partindex++) {
                        var part = part[partindex];
                    }
                }*/
            }
        }


    }
}

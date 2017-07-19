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
            var n = new SettingsBag("./lol.json");
            n["what"] = 1.2d;
            n["wha2t"] = 1.2d;
            n["wha3t"] = new string[] {"inception nigga"};
            n.Save();
        }


    }
}

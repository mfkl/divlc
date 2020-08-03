using CommandLine;
using static System.Console;
using System.Threading.Tasks;
using System.Collections.Generic;
using LibGit2Sharp;
using System.IO;

namespace libvlc_diff
{
    class Program
    {
        const string libvlc4 = "vlc";
        const string libvlc3 = "vlc-3.0";
        const string LibVLC4URL = "https://github.com/videolan/vlc";
        const string LibVLC3URL = "https://github.com/videolan/vlc-3.0";
        public class Options
        {
            //[Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            //public bool Verbose { get; set; } = true;

            //[Option('t', "torrent", Required = false, HelpText = "The torrent link to download and play")]
            //public string Torrent { get; set; } = "http://www.publicdomaintorrents.com/bt/btdownload.php?type=torrent&file=Charlie_Chaplin_Mabels_Strange_Predicament.avi.torrent";

            //// TODO: If multiple chromecast on the network, allow selecting it interactively via the CLI
            //[Option('c', "cast", Required = false, HelpText = "Cast to the chromecast")]
            //public bool Chromecast { get; set; }

            //[Option('s', "save", Required = false, HelpText = "Whether to save the media file. Defaults to true.")]
            //public bool Save { get; set; } = true;

            //[Option('p', "path", Required = false, HelpText = "Set the path where to save the media file.")]
            //public string Path { get; set; } = Environment.CurrentDirectory;
        }

        static async Task Main(string[] args)
        {
            var result = await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(RunOptions);

            result.WithNotParsed(HandleParseError);
        }

        private static void HandleParseError(IEnumerable<Error> error)
        {
            WriteLine($"Error while parsing...");
        }

        private static async Task RunOptions(Options cliOptions)
        {
            //TODO: check if clones already exist.
            //TODO: check and log clone progress
            var vlc4Dir = Path.Combine(Directory.GetCurrentDirectory(), libvlc4);
            var vlc3 = Path.Combine(Directory.GetCurrentDirectory(), libvlc3);

            if(Directory.Exists(vlc4Dir))
                Directory.Delete(vlc4Dir, true);

            var cloneOptions = new CloneOptions
            {
                OnTransferProgress = progress => { Write($"{progress.ReceivedBytes}/{progress.TotalObjects}"); return true; }
            };

            var vlc4Repo = Repository.Clone("https://code.videolan.org/videolan/vlc.git", vlc4Dir, cloneOptions);
            using (var vlc4 = new Repository(vlc4Repo))
            {

            }
            //var repoPath = Repository.Clone(lvs, Path.Combine(Directory.GetCurrentDirectory(), libvlc4), new CloneOptions
            //{
            //    //OnTransferProgress = _ => { return true; },
            //    BranchName = "master",
            //    Checkout = true,
            //    //OnProgress = progress => { WriteLine(progress); return true; },
            //    //OnUpdateTips = (name, oldId, newId) => { updateTipsWasCalled = true; return true; },
            //    //OnCheckoutProgress = (a, b, c) => checkoutWasCalled = true
            //});

            //var repo = new Repository(repoPath);

            //WriteLine(repo);
            //Repository.Clone(LibVLC3URL, Path.Combine(Directory.GetCurrentDirectory(), libvlc3));

        }
    }
}

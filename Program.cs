using CommandLine;
using static System.Console;
using System.Threading.Tasks;
using System.Collections.Generic;
using LibGit2Sharp;
using System.IO;
using CppAst;

namespace divlc
{
    class Program
    {
        const string libvlcHeader = "libvlc.h";
        const string vlc = "vlc";
        const string include = "include";
        const string libvlc4 = "vlc";
        const string libvlc3 = "vlc-3.0";
        const string LibVLC4URL = "https://github.com/videolan/vlc";
        const string LibVLC3URL = "https://github.com/videolan/vlc-3.0";
        static string vlc4Dir = Path.Combine(Directory.GetCurrentDirectory(), libvlc4);
        static string vlc3Dir = Path.Combine(Directory.GetCurrentDirectory(), libvlc3);

        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; } = true;

            [Option("libvlc4", Required = false, HelpText = "The hash of the libvlc 4 version to compare.")]
            public string LibVLC4Hash { get; set; } = "6099ac613e9b99116d94371845f88808bfe8e626";

            [Option("libvlc3", Required = false
                , HelpText = "The hash of the libvlc 3 version to compare.")]
            public string LibVLC3Hash { get; set; } = "3f633b2777a1ce2e41e9869cb49992960294369b";

            [Option("noclone", Required = true, HelpText = "Do not clone, use the provided local git repositories.")]
            public bool NoClone { get; set; }
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
            SetupGitRepositories(cliOptions);

            var exampleFile = Path.Combine(vlc3Dir, include, vlc, libvlcHeader);
            var exampleFile2 = Path.Combine(vlc4Dir, include, vlc, libvlcHeader);
            var r = CppParser.ParseFile(exampleFile);
            var r2 = CppParser.ParseFile(exampleFile2);




        }

        static void SetupGitRepositories(Options cliOptions)
        {
            //TODO: check if clones already exist and if we want to use it.
            //TODO: check and log clone progress
            if (!cliOptions.NoClone)
            {
                if (Directory.Exists(vlc4Dir))
                    Directory.Delete(vlc4Dir, true);
                if (Directory.Exists(vlc3Dir))
                    Directory.Delete(vlc3Dir, true);

               /* var cloneOptions = new CloneOptions
                {
                    OnTransferProgress = progress => { WriteLine($"{progress.ReceivedBytes}/{progress.TotalObjects}"); return true; }
                };*/

                var vlc4Repo = Repository.Clone(LibVLC4URL, vlc4Dir);
                var vlc3Repo = Repository.Clone(LibVLC3URL, vlc3Dir);
                using var vlc4 = new Repository(vlc4Repo);
                // TODO: checkout specific commit
            }
        }


    }
}

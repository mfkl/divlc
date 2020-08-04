using CommandLine;
using static System.Console;
using System.Threading.Tasks;
using System.Collections.Generic;
using LibGit2Sharp;
using System.IO;
using CppAst;
using System;
using System.Linq;

namespace divlc
{
    class Program
    {
        const string libvlc = "libvlc.h";
        const string libvlcDialog = "libvlc_dialog.h";
        const string libvlcEvents = "libvlc_events.h";
        const string libvlcMedia = "libvlc_media.h";
        const string libvlcMediaDiscoverer = "libvlc_media_discoverer.h";
        const string libvlcMediaLibrary = "libvlc_media_library.h";
        const string libvlcMediaList = "libvlc_media_list.h";
        const string libvlcMediaListPlayer = "libvlc_media_list_player.h";
        const string libvlcMediaPlayer = "libvlc_media_player.h";
        const string libvlcRendererDiscoverer = "libvlc_renderer_discoverer.h";
        const string libvlcVlm = "libvlc_vlm.h";
        const string vlch = "vlc.h";

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

            [Option("no-clone", Required = false, HelpText = "Do not clone, use the provided local git repositories.")]
            public bool NoClone { get; set; }

            [Option("files", Required = false, HelpText = "Comma separated list of libvlc header files to include:" +
                " Defaults to all")]
            public string Files { get; set; }

            [Option("no-comment", Required = false, HelpText = "Do not include comments/documentation in the comparison. Defaults to false")]
            public bool NoComment { get; set; }
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

        private static string BuildPath(string vlcDir, string filename)
        {
            var path = Path.Combine(vlcDir, include, vlc, filename);

            if (!File.Exists(path))
            {
                WriteLine($"file {filename} not found at {path}");
                return string.Empty;
            }
            return path;
        }

        private static async Task RunOptions(Options cliOptions)
        {
            /*      SetupGitRepositories(cliOptions);*/
            // libvlc 3


            var vlc3files = GetFilesToParse(vlc3Dir, cliOptions);
            var vlc4files = GetFilesToParse(vlc4Dir, cliOptions);

            var parserOptions = new CppParserOptions { ParseComments = cliOptions.NoComment, ParseMacros = true };
            parserOptions.IncludeFolders.Add(Path.Combine(vlc3Dir, include));

            var r = CppParser.ParseFiles(vlc3files, parserOptions);

            var r2 = CppParser.ParseFiles(vlc4files, new CppParserOptions
            {

                ParseAsCpp = false,
                ParseComments = cliOptions.NoComment
            });



            // use mono.cecil to compare objects
            // diff: https://github.com/unoplatform/Uno.PackageDiff/blob/master/src/Uno.PackageDiff/AssemblyComparer.cs
            // report: https://github.com/unoplatform/Uno.PackageDiff/blob/master/src/Uno.PackageDiff/ReportAnalyzer.cs
        }

        static List<string> GetFilesToParse(string vlcDir, Options cliOptions)
        {
            var paths = new List<string>();

            if (!string.IsNullOrEmpty(cliOptions.Files))
            {
                var files = cliOptions.Files.Split(',');
                foreach(var file in files)
                {
                    paths.Add(BuildPath(vlcDir, file));
                }
                return paths;
            }

          /*  paths.Add(BuildPath(vlcDir, libvlc));
            paths.Add(BuildPath(vlcDir, libvlcDialog));
            paths.Add(BuildPath(vlcDir, libvlcEvents));
            paths.Add(BuildPath(vlcDir, libvlcMedia));
            paths.Add(BuildPath(vlcDir, libvlcMediaDiscoverer));
            paths.Add(BuildPath(vlcDir, libvlcMediaLibrary));
            paths.Add(BuildPath(vlcDir, libvlcMediaList));
            paths.Add(BuildPath(vlcDir, libvlcMediaListPlayer));
            paths.Add(BuildPath(vlcDir, libvlcMediaPlayer));
            paths.Add(BuildPath(vlcDir, libvlcRendererDiscoverer));
            paths.Add(BuildPath(vlcDir, libvlcVlm));*/
            paths.Add(BuildPath(vlcDir, vlch));

            return paths;
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

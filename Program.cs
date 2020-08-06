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

        static bool NoComment;

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
            public bool NoClone { get; set; } = true; // TODO: change default to false

            [Option("files", Required = false, HelpText = "Comma separated list of libvlc header files to include:" +
                " Defaults to all")]
            public string Files { get; set; }

            [Option("no-comment", Required = false, HelpText = "Do not include comments/documentation in the comparison. Defaults to false")]
            public bool NoComment { get; set; } = false;

            [Option("deprecated", Required = false, HelpText = "Whether to include deprecated functions in the comparison. Defaults to false")]
            public bool Deprecated { get; set; } = false;

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
            NoComment = cliOptions.NoComment;

            SetupGitRepositories(cliOptions);

            var parsedv3 = Parse(vlc3Dir);
            var func = parsedv3.Functions.Except(parsedv3.Functions.Where(f
                => f.Comment != null && f.Comment.ChildrenToString().Contains("deprecated"))).ToList();

            foreach(var fu in func)
            {
                WriteLine(fu.Name);
            }

            var parsedv4 = Parse(vlc4Dir);


            // use mono.cecil to compare objects
            // diff: https://github.com/unoplatform/Uno.PackageDiff/blob/master/src/Uno.PackageDiff/AssemblyComparer.cs
            // report: https://github.com/unoplatform/Uno.PackageDiff/blob/master/src/Uno.PackageDiff/ReportAnalyzer.cs
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

                // TODO: fix clone options display for both clones
                // TODO: clone in parallel
                var cloneOptions = new CloneOptions
                {
                    OnTransferProgress = progress => { WriteLine($"{progress.ReceivedBytes}/{progress.TotalObjects}"); return true; }
                };

                Repository.Clone(LibVLC4URL, vlc4Dir, cloneOptions);
                Repository.Clone(LibVLC3URL, vlc3Dir, cloneOptions);
            }

            CheckoutHash(vlc3Dir, cliOptions.LibVLC3Hash);
            CheckoutHash(vlc4Dir, cliOptions.LibVLC4Hash);

            PatchFilesIfNeeded(vlc3Dir);
            PatchFilesIfNeeded(vlc4Dir);
        }

        private static void CheckoutHash(string vlc, string hash)
        {
            using var vlcRepo = new Repository(vlc);
            var localCommit = vlcRepo.Lookup<Commit>(hash);
            Commands.Checkout(vlcRepo, localCommit);
        }

        private static void PatchFilesIfNeeded(string vlcDir)
        {
            var media = BuildPath(vlcDir, libvlcMedia);
            var lines = File.ReadAllText(media);

            if (lines[0] == '/') // first char of the non patched file.
            {
                const string patch = @"#include <limits.h>
#include <stddef.h>
#if SIZE_MAX == UINT_MAX
typedef int ssize_t;        /* common 32 bit case */
#elif SIZE_MAX == ULLONG_MAX
typedef long long ssize_t;  /* windows 64 bits */
#endif";
                File.WriteAllText(media, patch + lines);
            }
        }

        private static CppCompilation Parse(string vlcDir)
        {
            var parserOptions = new CppParserOptions
            {
                ParseComments = NoComment
            };

            parserOptions.IncludeFolders.Add(Path.Combine(vlcDir, include));
            return CppParser.ParseFile(BuildPath(vlcDir, vlch), parserOptions);
        }
    }
}

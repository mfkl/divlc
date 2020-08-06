using CommandLine;
using static System.Console;
using System.Threading.Tasks;
using System.Collections.Generic;
using LibGit2Sharp;
using System.IO;
using CppAst;
using System;
using System.Linq;
using System.Net.Http;

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

            var parsedv4 = Parse(vlc4Dir);

            Diff(parsedv3, parsedv4);

            //var func = parsedv3.Functions.Except(parsedv3.Functions.Where(f
            //    => f.Comment != null && f.Comment.ChildrenToString().Contains("deprecated"))).ToList();

            //foreach(var fu in func)
            //{
            //    WriteLine(fu.Name);
            //}


            var func = parsedv4.Functions.Except(parsedv4.Functions.Where(f
                => f.Comment != null && f.Comment.ChildrenToString().Contains("deprecated"))).ToList();

            string l = null;
            List<string> symbs = new List<string>();
            foreach (var fu in func)
            {
                l = l + fu.Name + Environment.NewLine;
                symbs.Add(fu.Name);
            }


            //using var httpClient = new HttpClient();
            //var libvlcSymbols = (await httpClient.GetStringAsync(LibVLCSymURL)).Split(new[] { '\r', '\n' }).Where(s => !string.IsNullOrEmpty(s)).ToList();

            //foreach(var s in libvlcSymbols)
            //{
            //    if(!symbs.Contains(s))
            //    {

            //    }
            //}
            //var libvlcdeprecatedSym = (await httpClient.GetStringAsync(LibVLCDeprecatedSymUrl)).Split(new[] { '\r', '\n' }).Where(s => !string.IsNullOrEmpty(s)).ToList();

            // use mono.cecil to compare objects
            // diff: https://github.com/unoplatform/Uno.PackageDiff/blob/master/src/Uno.PackageDiff/AssemblyComparer.cs
            // report: https://github.com/unoplatform/Uno.PackageDiff/blob/master/src/Uno.PackageDiff/ReportAnalyzer.cs
        }

        private static void Diff(CppCompilation parsedv3, CppCompilation parsedv4)
        {
            CompareStructs(parsedv3.Classes, parsedv4.Classes);

            CompareFuncs(parsedv3.Functions, parsedv4.Functions);
        }

        private static void CompareFuncs(CppContainerList<CppFunction> functions1, CppContainerList<CppFunction> functions2)
        {
            WriteLine(Environment.NewLine);

            ForegroundColor = ConsoleColor.Yellow;

            //TODO: Add deprecated option.
            var f1 = functions1.Except(functions1.Where(f => f.Span.Start.File.Contains("deprecated")));

            var f2 = functions2.Except(functions2.Where(f => f.Span.Start.File.Contains("deprecated")));

            if (f1.Count() != f2.Count())
                System.Diagnostics.Debug.WriteLine($"function count is different. v1 has {f1.Count()}, v2 has {f2.Count()}");

            foreach (var function in f1.Where(f1 => !f2.Any(f2 => f2.Name == f1.Name)))
            {
                // functions in v1, not in v2
                System.Diagnostics.Debug.WriteLine($"function {function.Name} was removed from libvlc 4");
            }

            ForegroundColor = ConsoleColor.Green;

            foreach (var function in f2.Where(f2 => !f1.Any(f1 => f1.Name == f2.Name)))
            {
                // functions in v2, not in v1
                System.Diagnostics.Debug.WriteLine($"function {function.Name} was added in libvlc 4");
            }
            // functions in v2, not in v1

            // for same functions, check return parameter, parameter count, parameter order, parameter type, parameter name, comment.
        }

        private static void CompareStructs(CppContainerList<CppClass> structsv3, CppContainerList<CppClass> structsv4)
        {
            WriteLine($"LibVLC 3 has {structsv3.Count} structs");
            WriteLine($"LibVLC 4 has {structsv4.Count} structs");

            List<CppClass> v3structMissingFromv4 = new List<CppClass>();
            List<CppClass> v4structMissingFromv3 = new List<CppClass>();
            List<StructDiff> structDiffs = new List<StructDiff>();

            foreach (var s in structsv3)
            {
                var match = structsv4.FirstOrDefault(ss => ss.Name == s.Name);
                if(match == null)
                {
                    v3structMissingFromv4.Add(s);
                    continue;
                }

                if (match.SizeOf != s.SizeOf)
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine($"{match.Name} size is {s.SizeOf} in libvlc 3 and {match.SizeOf} in libvlc 4");
                }

                CompareStructFields(s.Name, s.Fields, match.Fields);
                //var diff = !match.Comment?.ChildrenToString()?.Equals(s.Comment?.ChildrenToString());
                //if (diff.HasValue && !diff.Value && (match.Comment != null && s.Comment != null)) continue;

                structDiffs.Add(new StructDiff
                {
                    Name = s.Name,
                    Commentv3 = s.Comment?.ChildrenToString(),
                    Commentv4 = match.Comment?.ChildrenToString()
                });

            }

            foreach (var s in structsv4)
            {
                var match = structsv3.FirstOrDefault(ss => ss.Name == s.Name);
                if (match == null)
                {
                    ForegroundColor = ConsoleColor.Red;
                    v4structMissingFromv3.Add(s);
                    continue;
                }

                if (match.Comment == null && s.Comment == null) continue;
                if (match.Comment == null || s.Comment == null)
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine($"Comment changed for {s.Name}");
                    if (match.Comment == null)
                        WriteLine($"{match.Name} has no more documentation");
                    else WriteLine($"{s.Name} has no more documentation");
                    continue;
                }
                if (!match.Comment.ChildrenToString().Equals(s.Comment.ChildrenToString()))
                {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine($"Comment changed for {s.Name}");
                    WriteLine($"LibVLC 4: {s.Comment.ChildrenToString()}");
                    WriteLine($"LibVLC 3: {match.Comment.ChildrenToString()}");
                }
            }


        }

        private static void CompareStructFields(string structName, CppContainerList<CppField> fields1,
            CppContainerList<CppField> fields2)
        {
            if (fields1.Count == 0 && fields2.Count == 0) return;

            if (fields1.Count != fields2.Count)
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine($"{structName} {nameof(fields1)} count is {fields1.Count} in libvlc 3");
                WriteLine($"{structName} {nameof(fields1)} count is {fields2.Count} in libvlc 4");
            }

            foreach(var field in fields1.Where(f1 => !fields2.Any(f2 => f2.Name == f1.Name)))
            {
                WriteLine($"field {field.Name} is missing from v2");
                // item missing in v2
            }

            var union1 = fields1.FirstOrDefault(f1 => f1.Name == "u");
            var union2 = fields2.FirstOrDefault(f2 => f2.Name == "u");

            if(union1 != null && union2 != null)
            {
                CompareEventsUnion(union1, union2);
            }

            foreach (var field in fields2.Where(f2 => !fields1.Any(f1 => f1.Name == f2.Name)))
            {
                // item missing in v1
                WriteLine($"field {field.Name} is missing from v1");
            }

            //fields1.Where(s => s.Name)
            // type, name, visibility, comment
            //for (var i = 0; i < fields1.Count; i++)
            //{
            //    var f1 = fields1[i];
            //    var f2 = fields2[i];
            ////    if(f1.Type == )
            //    if (f1.Type.GetDisplayName() != f2.Type.GetDisplayName())
            //        WriteLine($"{nameof(f1.Type)} {f1.Type.GetDisplayName()} is different than {f2.Type.GetDisplayName()}");
            //    if(f1.Name != f2.Name)
            //        WriteLine($"{nameof(f1.Name)} {f1.Name} is different than {f2.Name}");
            //    if(f1.Visibility != f2.Visibility)
            //        WriteLine($"{nameof(f1.Visibility)} {f1.Visibility} is different than {f2.Visibility}");
            //    if(f1.Comment?.ChildrenToString() != f2.Comment?.ChildrenToString())
            //        WriteLine($"{nameof(f1.Comment)} {f1.Comment?.ChildrenToString()} is different than {f2.Comment?.ChildrenToString()}");
            //}

        }

        private static void CompareEventsUnion(CppField union1, CppField union2)
        {
            var c1 = (CppClass)union1.Type;
            var c2 = (CppClass)union2.Type;
            var r1 = c1.Fields.Where(t1 => !c2.Fields.Any(t2 => t1.Name == t2.Name)).ToList(); // in v1 but not in v2
            var r2 = c2.Fields.Where(t2 => !c1.Fields.Any(t1 => t2.Name == t1.Name)).ToList(); // in v2 but not in v1

            WriteLine("Events change");
            WriteLine(Environment.NewLine);
            WriteLine("fields in v1 but not v2:");
            foreach(var r in r1)
                WriteLine(r.Name);

            WriteLine(Environment.NewLine);
            WriteLine("fields in v2 but not in v1:");
            foreach (var r in r2)
                WriteLine(r.Name);
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

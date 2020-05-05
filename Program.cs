using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapsetVerifierFramework;
using MapsetVerifierFramework.objects;
using MapsetParser;
using MapsetParser.objects;
using MapsetParser.settings;
using VerifierCLI.objects;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;


namespace VerifierCLI
{
    [Command( Name = "VerifierCLI", Description = "Command line version of Mapset Verifier")]
    [HelpOption("-?|-h|--help")]
    class Program
    {
        [Argument(0, Description = "Path to mapset.")]
        public string MapsetPath { get; }

        [Option("-L|--level", Description = "Minimum issue level to show. Defaults to Info.")]
        public Issue.Level IssueLevel { get; } = Issue.Level.Info;

        [Option("--json", Description = "Whether to use JSON output or not. Defaults to false.")]
        public bool OutputAsJSON { get; }

        [Option("-v", Description = "Verbose output. Defaults to false.")]
        public bool Verbose { get; }

        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        public void OnExecute()
        {
            if (MapsetPath == null)
            {
                Console.WriteLine("Missing argument: MapsetPath. Use -h to show help.");
                return;
            }

            BeatmapSet beatmapSet = new BeatmapSet(MapsetPath);

            if (Verbose) {
                if (OutputAsJSON) {
                    Checker.OnLoadStart = LoadStartJS;
                    Checker.OnLoadComplete = LoadFinishJS;
                } else {
                    Checker.OnLoadStart = LoadStart;
                    Checker.OnLoadComplete = LoadFinish;
                }
            }

            Checker.LoadCheckDLLs();
            IEnumerable<Issue> issues =
                Checker.GetBeatmapSetIssues(beatmapSet)
                    .Where(anIssue => anIssue.level >= IssueLevel);

            List<JSONIssue> mapIssues = new List<JSONIssue>();
            foreach (var beatmapIssues in issues.GroupBy(anIssue => anIssue.beatmap))
            {
                JSONIssue diffIssue = new JSONIssue();
                
                Beatmap beatmap = beatmapIssues.Key;
                diffIssue.difficulty = beatmap?.ToString() ?? "[General]";
                diffIssue.problems = new List<CategoryIssue>();

                foreach (var category in beatmapIssues.GroupBy(anIssue => anIssue.CheckOrigin))
                {

                    if (beatmap == null || category.Any(anIssue => anIssue.AppliesToDifficulty(beatmap.GetDifficulty())))
                    {
                        CategoryIssue categoryIssue = new CategoryIssue();
                        categoryIssue.problems = new List<Problem>();
                        categoryIssue.message = category.Key.GetMetadata().Message;

                        foreach (Issue issue in category)
                        {
                            if (beatmap == null || issue.AppliesToDifficulty(beatmap.GetDifficulty()))
                            {
                                Problem problemIssue = new Problem();
                                problemIssue.level = issue.level;
                                problemIssue.message = issue.message;
                                categoryIssue.problems.Add(problemIssue);
                            }
                        }
                        diffIssue.problems.Add(categoryIssue);
                    }
                }
                mapIssues.Add(diffIssue);
            }

            Result mapResult = new Result();
            Metadata mapMetadata = new Metadata();
            MetadataSettings settings = beatmapSet.beatmaps.First().metadataSettings;
            mapMetadata.artist = settings.GetFileNameFiltered(settings.artist);
            mapMetadata.title = settings.GetFileNameFiltered(settings.title);
            mapMetadata.mapper = settings.GetFileNameFiltered(settings.creator);
            mapResult.metadata = mapMetadata;
            mapResult.results = mapIssues;
            if (OutputAsJSON) {
                Console.Write(JsonConvert.SerializeObject(mapResult));
            } else {
                foreach (JSONIssue diff in mapIssues) {
                    Console.WriteLine(diff.difficulty);
                    foreach (CategoryIssue category in diff.problems) {
                        Console.WriteLine(category.message);
                        foreach (Problem timestamp in category.problems) {
                            Console.WriteLine($"\t {timestamp.level} - {timestamp.message}");
                        }
                    }
                    Console.WriteLine();
                }
            }
        }

        private static async Task LoadStart(string aLoadMessage) {
            Console.WriteLine(aLoadMessage);
        }

        private static async Task LoadFinish(string aLoadMessage) {
            Console.WriteLine(aLoadMessage);
        }

        private static async Task LoadStartJS(string aLoadMessage) {
            Console.WriteLine("{'message': '" + aLoadMessage +"', 'status': 'start'}");
        }

        private static async Task LoadFinishJS(string aLoadMessage) {
            Console.WriteLine("{'message': '" + aLoadMessage +"', 'status': 'finish'}");
        }
    }
}

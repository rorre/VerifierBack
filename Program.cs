using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MapsetVerifierFramework;
using MapsetVerifierFramework.objects;
using MapsetParser;
using MapsetParser.objects;
using VerifierBack.objects;
using Newtonsoft.Json;

namespace VerifierBack
{

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() != 1)
            {
                Console.WriteLine("Requires one argument.");
                return;
            }

            BeatmapSet beatmapSet = new BeatmapSet(args[0]);

            Checker.LoadCheckDLLs();
            IEnumerable<Issue> issues = Checker.GetBeatmapSetIssues(beatmapSet);

            List<string> mapIssues = new List<string>();
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
                mapIssues.Add(JsonConvert.SerializeObject(diffIssue));
            }
            Console.Write("[" + mapIssues[0]);
            foreach (String json in mapIssues.Skip(1)) Console.Write($",{json}");
            Console.Write("]");
        }
    }
}

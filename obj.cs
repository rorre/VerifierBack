using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MapsetVerifierFramework.objects;

namespace VerifierCLI.objects
{
    class CategoryIssue
    {
        public string message {get; set;}
        public List<Problem> problems {get; set;}
    }
    class Problem
    {
        public Issue.Level level {get; set;}
        public string message {get; set;}
    }
    class JSONIssue
    {
        public string difficulty {get; set;}
        public IList<CategoryIssue> problems {get; set;}
    }
    class Metadata
    {
        public string artist {get; set;}
        public string title {get; set;}
        public string mapper {get; set;}
    }
    class Result
    {
        public Metadata metadata {get; set;}
        public List<JSONIssue> results {get; set;}
    }
}
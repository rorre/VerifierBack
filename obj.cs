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
}
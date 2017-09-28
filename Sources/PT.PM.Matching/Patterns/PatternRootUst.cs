﻿using Newtonsoft.Json;
using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternRootUst : RootUst, IPatternUst, ILoggable
    {
        private HashSet<Language> languages = new HashSet<Language>();
        private Regex pathWildcardRegex;

        [JsonIgnore]
        public ILogger Logger { get; set; }

        public string Key { get; set; } = "";

        public string FilenameWildcard { get; set; }

        public HashSet<Language> Languages
        {
            get
            {
                return languages;
            }
            set
            {
                if (languages.Contains(Language.Aspx))
                {
                    throw new ArgumentException($"Unable to create pattern for Aspx");
                }
                languages = value;
            }
        }

        public UstFormat DataFormat { get; set; } = UstFormat.Json;

        [JsonIgnore]
        public string DebugInfo { get; set; } = "";

        [JsonIgnore]
        public Regex FilenameWildcardRegex
        {
            get
            {
                if (!string.IsNullOrEmpty(FilenameWildcard) && pathWildcardRegex == null)
                {
                    pathWildcardRegex = new WildcardConverter().Convert(FilenameWildcard);
                }
                return pathWildcardRegex;
            }
        }

        public PatternRootUst(string key, string debugInfo, IEnumerable<Language> languages, string filenameWildcard)
            : this(null)
        {
            Key = key;
            DebugInfo = debugInfo;
            Languages = new HashSet<Language>(languages);
            FilenameWildcard = filenameWildcard;
        }

        public PatternRootUst()
            : this(null)
        {
        }

        public PatternRootUst(SourceCodeFile sourceCodeFile)
            : base(sourceCodeFile, Language.Universal)
        {
        }

        public override Ust[] GetChildren() => Nodes;

        public override string ToString()
        {
            return (!string.IsNullOrEmpty(DebugInfo) ? DebugInfo : Key) ?? "";
        }

        public List<MatchingResult> Match(Ust ust)
        {
            var context = new MatchingContext(this) { Logger = Logger };
            context = Match(ust, context);
            return context.Results;
        }

        public MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is RootUst rootUst)
            {
                var patternUst = (IPatternUst)Node;

                if (patternUst is PatternComment ||
                   (patternUst is PatternOr && ((PatternOr)patternUst).Alternatives.Any(v => v is PatternComment)))
                {
                    foreach (CommentLiteral commentNode in rootUst.Comments)
                    {
                        MatchAndAddResult(patternUst, commentNode, context);
                    }
                }
                else
                {
                    TraverseChildren(patternUst, rootUst, context);
                }

                newContext = context;
            }
            else
            {
                newContext = context.Fail();
            }

            return context;
        }

        private static void TraverseChildren(IPatternUst patternUst, Ust ust, MatchingContext context)
        {
            MatchAndAddResult(patternUst, ust, context);

            if (ust != null)
            {
                foreach (Ust child in ust.Children)
                {
                    TraverseChildren(patternUst, child, context);
                }
            }
        }

        private static void MatchAndAddResult(IPatternUst patternUst, Ust ust, MatchingContext context)
        {
            if (patternUst.Match(ust, context).Success)
            {
                if (context.Locations.Count == 0)
                {
                    context.Locations.Add(ust.TextSpan);
                }
                var matching = new MatchingResult(ust.Root, context.PatternUst, context.Locations);
                context.Logger.LogInfo(matching);
                context.Results.Add(matching);
            }
            context.Locations.Clear();
        }
    }
}

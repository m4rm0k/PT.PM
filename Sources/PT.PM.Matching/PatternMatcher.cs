﻿using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Matching
{
    public class PatternMatcher : IUstPatternMatcher<RootUst, PatternRoot, MatchResult>
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public List<PatternRoot> Patterns { get; set; }

        public bool IsIgnoreFilenameWildcards { get; set; }

        public PatternMatcher()
        {
        }

        public PatternMatcher(List<PatternRoot> patterns)
        {
            Patterns = patterns;
        }

        public List<MatchResult> Match(RootUst ust)
        {
            try
            {
                IEnumerable<PatternRoot> patterns = Patterns
                    .Where(pattern => pattern.Languages.Any(patternLang => ust.Sublanguages.Contains(patternLang)));
                if (!IsIgnoreFilenameWildcards)
                {
                    patterns = patterns.Where(pattern => pattern.FilenameWildcardRegex?.IsMatch(ust.SourceCodeFile.FullName) ?? true);
                }

                var result = new List<MatchResult>();
                foreach (PatternRoot pattern in patterns)
                {
                    pattern.Logger = Logger;
                    var results = pattern.Match(ust);
                    result.AddRange(results);
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(new MatchingException(ust.SourceCodeFile, ex));
                return new List<MatchResult>();
            }
        }
    }
}

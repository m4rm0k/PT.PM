﻿using PT.PM.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PT.PM
{
    public class ParserLanguageDetector : LanguageDetector
    {
        private readonly static Regex openTagRegex = new Regex("<\\w+>", RegexOptions.Compiled);
        private readonly static Regex closeTagRegex = new Regex("<\\/\\w+>", RegexOptions.Compiled);

        public override Language Detect(string sourceCode, IEnumerable<Language> languages = null)
        {
            List<Language> langs = (languages ?? LanguageUtils.Languages.Values).ToList();
            langs.Remove(Uncertain.Language);
            // Any PHP file contains start tag.
            if (!sourceCode.Contains("<?"))
            {
                langs.Remove(langs.FirstOrDefault(l => l.Key == "Php"));
            }
            // Aspx and html code contains at least one tag.
            if (!openTagRegex.IsMatch(sourceCode) || !closeTagRegex.IsMatch(sourceCode))
            {
                langs.Remove(langs.FirstOrDefault(l => l.Key == "Aspx"));
                langs.Remove(langs.FirstOrDefault(l => l.Key == "Html"));
            }
            var sourceCodeFile = new CodeFile(sourceCode);
            var parseUnits = new Queue<Tuple<Language, ParserUnit>>(langs.Count);

            langs = langs
                .GroupBy(l => l.CreateParser())
                .Select(l => l.First())
                .ToList();

            if (langs.Count == 1)
            {
                return langs[0];
            }

            foreach (Language language in langs)
            {
                var logger = new LoggerMessageCounter();
                ILanguageParser languageParser = language.CreateParser();

                var task = Task.Factory.StartNew(() =>
                {
                    languageParser.Logger = logger;
                    languageParser.Parse(sourceCodeFile);
                });

                parseUnits.Enqueue(Tuple.Create(language, new ParserUnit(languageParser, logger, task)));
            }

            int minErrorCount = int.MaxValue;
            Language resultWithMinErrors = null;

            // Check every parseUnit completion every 50 ms.
            while (parseUnits.Count > 0)
            {
                var pair = parseUnits.Dequeue();
                if (!pair.Item2.Task.IsCompleted)
                {
                    parseUnits.Enqueue(pair);
                    Thread.Sleep(50);
                    continue;
                }

                if (pair.Item2.Logger.ErrorCount == 0 && pair.Item1.Key != "Aspx")
                {
                    return pair.Item1;
                }

                var errorCount = pair.Item2.ParseErrorCount;
                if (errorCount < minErrorCount)
                {
                    minErrorCount = errorCount;
                    resultWithMinErrors = pair.Item1;
                }
            }

            return resultWithMinErrors;
        }
    }
}

﻿using System.Collections.Generic;
using PT.PM.Common;
using PT.PM.Patterns.Nodes;
using System;
using PT.PM.Common.Exceptions;
using System.Linq;
using PT.PM.Common.Nodes;

namespace PT.PM.Patterns
{
    public class PatternConverter : IPatternConverter<PatternRootNode>
    {
        private ILogger logger { get; set; } = DummyLogger.Instance;

        public ILogger Logger
        {
            get
            {
                return logger;
            }
            set
            {
                logger = value;
                foreach (var serializer in UstNodeSerializers)
                {
                    serializer.Value.Logger = logger;
                }
            }
        }

        public Dictionary<UstNodeSerializationFormat, IUstNodeSerializer> UstNodeSerializers { get; set; }

        public PatternConverter(IUstNodeSerializer serializer)
            : this(new[] { serializer })
        {
        }

        public PatternConverter(IEnumerable<IUstNodeSerializer> serializers)
        {
            UstNodeSerializers = new Dictionary<UstNodeSerializationFormat, IUstNodeSerializer>();
            foreach (var serializer in serializers)
            {
                UstNodeSerializers[serializer.DataFormat] = serializer;
            }
        }

        public PatternRootNode[] Convert(IEnumerable<PatternDto> patternsDto)
        {
            var result = new List<PatternRootNode>(patternsDto.Count());
            foreach (PatternDto patternDto in patternsDto)
            {
                var serializer = UstNodeSerializers[patternDto.DataFormat];
                try
                {
                    var node = serializer.Deserialize(patternDto.Value);
                    PatternRootNode pattern = new PatternRootNode
                    {
                        DataFormat = serializer.DataFormat,
                        Key = patternDto.Key,
                        Languages = patternDto.Languages,
                        Node = node is PatternRootNode patternRootNode ? patternRootNode.Node : node,
                        DebugInfo = patternDto.Description,
                        FilenameWildcard = patternDto.FilenameWildcard
                    };
                    pattern.FillAscendants();
                    result.Add(pattern);
                }
                catch (Exception ex)
                {
                    Logger.LogError(new ConversionException("", ex, $"Error while \"{patternDto.Key}\" pattern deserialising ({patternDto.Value}) ", true));
                }
            }
            return result.ToArray();
        }

        public PatternDto[] ConvertBack(IEnumerable<PatternRootNode> patterns)
        {
            var result = new List<PatternDto>();
            foreach (PatternRootNode pattern in patterns)
            {
                var serializer = UstNodeSerializers[pattern.DataFormat];
                try
                {
                    PatternDto patternDto = new PatternDto
                    {
                        DataFormat = pattern.DataFormat,
                        Key = pattern.Key,
                        Languages = pattern.Languages,
                        Value = serializer.Serialize(
                              pattern is PatternRootNode patternNode
                            ? patternNode.Node
                            : pattern),
                        Description = pattern.DebugInfo,
                        FilenameWildcard = pattern.FilenameWildcard
                    };
                    result.Add(patternDto);
                }
                catch (Exception ex)
                {
                    Logger.LogError(new ConversionException("", ex, $"Error while \"{pattern.Key}\" pattern serialising", true));
                }
            }
            return result.ToArray();
        }
    }
}

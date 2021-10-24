namespace RoboPhredDev.PotionCraft.Crucible.Yaml
{
    using System;
    using System.Collections.Generic;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;

    /// <summary>
    /// A parser meant to replay previously parsed events.
    /// </summary>
    public class ReplayParser : IParser
    {
        private readonly List<ParsingEvent> replayEvents = new();

        private IEnumerator<ParsingEvent> replayEnumerator = null;

        /// <inheritdoc/>
        public ParsingEvent Current => this.replayEnumerator != null ? this.replayEnumerator.Current : throw new InvalidOperationException("ReplayParser has not been activated.");

        /// <summary>
        /// Parses an object from the reader and returns a ReplayParser that can repeat the object nodes.
        /// </summary>
        /// <param name="reader">The parser to read from.</param>
        /// <returns>A ReplayParser containing the object.</returns>
        public static ReplayParser ParseObject(IParser reader)
        {
            var replayParser = new ReplayParser();

            var mappingStart = reader.Consume<MappingStart>();
            replayParser.Enqueue(mappingStart);

            var keys = new List<string>();
            MappingEnd mappingEnd;
            while (!reader.TryConsume(out mappingEnd))
            {
                var key = reader.Consume<Scalar>();
                keys.Add(key.Value);
                replayParser.Enqueue(key);

                // The value might be more complex than just a scaler
                //  This code is cribbed from the SkipThisAndNestedEvents IParser extension method
                var depth = 0;
                do
                {
                    var next = reader.Consume<ParsingEvent>();
                    depth += next.NestingIncrease;

                    // Make sure to save this node for the nestedObjectDeserializer pass
                    replayParser.Enqueue(next);
                }
                while (depth > 0);
            }

            // reader.Accept will have obtained a mapping end, queue it up.
            replayParser.Enqueue(mappingEnd);

            replayParser.Start();

            return replayParser;
        }

        /// <summary>
        /// Enqueue a parsing event to emit when the parser starts.
        /// </summary>
        /// <param name="parsingEvent">The event to enqueue.</param>
        public void Enqueue(ParsingEvent parsingEvent)
        {
            if (this.replayEnumerator != null)
            {
                throw new InvalidOperationException("Cannot enqueue to a ReplayParser that has already been started.");
            }

            this.replayEvents.Add(parsingEvent);
        }

        /// <summary>
        /// Start the parser for its parse operations.
        /// </summary>
        public void Start()
        {
            if (this.replayEnumerator != null)
            {
                throw new InvalidOperationException("This ReplayParser has already been started.");
            }

            this.replayEnumerator = this.replayEvents.GetEnumerator();
        }

        /// <summary>
        /// Resets this replay parser back to the start of its node collection.
        /// </summary>
        public void Reset()
        {
            this.replayEnumerator = this.replayEvents.GetEnumerator();
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            return this.replayEnumerator.MoveNext();
        }
    }
}

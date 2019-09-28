namespace Lwt.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Lwt.Interfaces;
    using Lwt.Models;

    /// <inheritdoc />
    public class TermCounter : ITermCounter
    {
        private readonly ITermRepository termRepository;
        private readonly ISkippedWordRemover skippedWordRemover;
        private readonly ITextNormalizer textNormalizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TermCounter"/> class.
        /// </summary>
        /// <param name="termRepository">the term repo.</param>
        /// <param name="skippedWordRemover">skipped words remover.</param>
        /// <param name="textNormalizer">text normalizer.</param>
        public TermCounter(
            ITermRepository termRepository,
            ISkippedWordRemover skippedWordRemover,
            ITextNormalizer textNormalizer)
        {
            this.termRepository = termRepository;
            this.skippedWordRemover = skippedWordRemover;
            this.textNormalizer = textNormalizer;
        }

        /// <inheritdoc/>
        public async Task<Dictionary<TermLearningLevel, long>> CountByLearningLevelAsync(
            IEnumerable<string> words,
            Language language,
            Guid userId)
        {
            IEnumerable<string> notSkippedTerms = this.skippedWordRemover.RemoveSkippedWords(words, language);
            IEnumerable<string> notSkippedTermsNormalized =
                this.textNormalizer.Normalize(notSkippedTerms, language);
            var termDict = new Dictionary<string, long>();

            foreach (string term in notSkippedTermsNormalized)
            {
                if (termDict.ContainsKey(term))
                {
                    termDict[term] += 1;
                }
                else
                {
                    termDict[term] = 1;
                }
            }

            Dictionary<string, TermLearningLevel> countDict =
                await this.termRepository.GetLearningLevelAsync(
                    userId,
                    language,
                    termDict.Keys.ToHashSet());

            var result = new Dictionary<TermLearningLevel, long>();
            foreach (TermLearningLevel termLearningLevel in Enum.GetValues(typeof(TermLearningLevel)))
            {
                result[termLearningLevel] = 0;
            }

            foreach (string word in termDict.Keys)
            {
                if (!countDict.ContainsKey(word))
                {
                    result[TermLearningLevel.UnKnow] += termDict[word];
                    continue;
                }

                result[countDict[word]] += termDict[word];
            }

            return result;
        }
    }
}
namespace Lwt.Models
{
    using System.Text.RegularExpressions;

    using Lwt.Interfaces;

    /// <summary>
    /// English language.
    /// </summary>
    public class English : ILanguage
    {
        /// <inheritdoc/>
        public string Name { get; set; } = "English";

        /// <inheritdoc/>
        public Language Id { get; set; } = Language.English;

        /// <inheritdoc/>
        public string[] SplitText(string text)
        {
            return Regex.Split(text, @"([^a-zA-Z0-9\'])");
        }
    }
}
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SEP.ValidationEngine
{
    public enum OverallStatus
    {
        ParseError,
        DataError,
        Valid
    }

    /// <summary>The validation status for some chunk of arbitrarily complex data.</summary>
    public class ValidationStatus
    {

        /// <summary>The status' state: Valid, ParseError (e.g. malformed JSON), or DataError (e.g. business rule violation)</summary>
        public OverallStatus Disposition { get; set; }

        /// <summary>True for Valid statuses, otherwise false</summary>
        public bool IsSuccess
        {
            get
            {
                return Disposition == OverallStatus.Valid;
            }
        }

        /// <summary>Error messages associated with each other by key. If no key is used, all messages will be keyed to the empty string.</summary>
        public IReadOnlyDictionary<string, IList<string>> ErrorsByKey
        {
            get
            {
                return new ReadOnlyDictionary<string, IList<string>>(Errors);
            }
        }

        private Dictionary<string, IList<string>> Errors { get; set; }
        private ValidationStatus(OverallStatus disposition)
        {
            Disposition = disposition;
            Errors = new Dictionary<string, IList<string>>();
        }

        /// <summary>Attaches the given message to this status keyed by the given key, or the empty string.</summary>
        /// <param name="message">The message to attach</param>
        /// <param name="key">(Optional) a string by which the message will be grouped</param>
        /// <returns>This status (for dot-chaining)</returns>
        public ValidationStatus WithMessage(string message, string key = "")
        {
            key = key ?? "";
            if (!ErrorsByKey.ContainsKey(key))
            {
                Errors[key] = new List<string>();
            }
            Errors[key].Add(message);
            return this;
        }

        /// <summary>Merges this status with the given status. Merged statuses contain the most severe Disposition of either input status, as well as collated messages from both.</summary>
        /// <param name="otherStatus">The message to attach</param>
        /// <param name="key">(Optional) A string by which the message will be grouped</param>
        /// <returns>The newly merged status</returns>
        public ValidationStatus Merge(ValidationStatus otherStatus)
        {
            return MergeAll(this, otherStatus);
        }

        /// <summary>Helper method to access error messages for the given key</summary>
        /// <param name="key">A string specifying the group of messages desired</param>
        /// <returns>A list of all messages associated with the given key</returns>
        public IEnumerable<string> MessagesFor(string key)
        {
            return Errors.ContainsKey(key) ? Errors[key] : new List<string>();
        }

        /// <summary>Merges all given statuses. Merged statuses contain the most severe Disposition of any input status, as well as collated messages from all.</summary>
        /// <param name="statuses">The statuses to merge</param>
        /// <returns>The newly merged status (or a valid status if no statuses are given)</returns>
        public static ValidationStatus MergeAll(params ValidationStatus[] statuses)
        {
            var disposition = statuses.Any(status => status.Disposition == OverallStatus.ParseError)
                ? OverallStatus.ParseError
                : statuses.Any(status => status.Disposition == OverallStatus.DataError)
                    ? OverallStatus.DataError
                    : OverallStatus.Valid;
            var errors = statuses
                .SelectMany(status => status.Errors)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key,
                    g => (IList<string>)g.SelectMany(kvp => kvp.Value).ToList());
            return new ValidationStatus(disposition)
            {
                Errors = errors
            };
        }

        /// <summary>Generates a new success status</summary>
        /// <returns>The new status</returns>
        public static ValidationStatus Success()
        {
            return new ValidationStatus(OverallStatus.Valid);
        }

        /// <summary>Generates a new failure status</summary>
        /// <param name="disposition">(Optional) The desired disposition of the new status (defaults to DataError)</param>
        /// <returns>The new status</returns>
        public static ValidationStatus Failure(OverallStatus disposition = OverallStatus.DataError)
        {
            return new ValidationStatus(disposition);
        }

        /// <summary>Generates a new status with success determined by the given boolean</summary>
        /// <param name="isSuccessful">True for Valid, false for DataError</param>
        /// <param name="message">A message to attach if `isSuccessful` is false</param>
        /// <param name="key">(Optional) The `message`'s key for if `isSuccessful` is false</param>
        /// <returns>Either a valid status or an invalid status with the given message</returns>
        public static ValidationStatus Evaluate(bool isSuccessful,
            string message,
            string key = "")
        {
            return isSuccessful
                ? Success()
                : Failure().WithMessage(message, key);
        }
    }
}

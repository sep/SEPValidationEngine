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

    public class ValidationStatus
    {
        public OverallStatus Disposition { get; set; }

        public bool IsSuccess
        {
            get
            {
                return Disposition == OverallStatus.Valid;
            }
        }

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

        public ValidationStatus Merge(ValidationStatus otherStatus)
        {
            return MergeAll(this, otherStatus);
        }

        public IEnumerable<string> MessagesFor(string key)
        {
            return Errors.ContainsKey(key) ? Errors[key] : new List<string>();
        }

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

        public static ValidationStatus Success()
        {
            return new ValidationStatus(OverallStatus.Valid);
        }

        public static ValidationStatus Failure(OverallStatus disposition = OverallStatus.DataError)
        {
            return new ValidationStatus(disposition);
        }

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

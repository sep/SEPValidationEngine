using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace SEP.ValidationEngine
{
    public abstract class BaseValidator
    {
        protected ValidationStatus ForEach(Func<IEnumerable<dynamic>> collectionMonad, Func<dynamic, int, ValidationStatus> validationMethod)
        {
            IEnumerable<dynamic> collection;
            try { collection = collectionMonad() ?? new dynamic[0]; }
            catch { collection = new dynamic[0]; }
            return ValidationStatus.MergeAll(collection.Select(validationMethod).ToArray());
        }

        #region object Rules

        protected Rule<T> IsNotNull<T>()
        {
            return new Rule<T>(value => null != value,
                " is a required field",
                false);
        }

        protected Rule<T> IsIn<T>(IEnumerable<T> validValues, string itemLabel)
        {
            return new Rule<T>(value => value != null && validValues.Contains(value),
                " must be a " + itemLabel,
                false);
        }

        #endregion

        #region string Rules

        protected Rule<string> HasNoFewerCharactersThan(int minimum)
        {
            return new Rule<string>(str => str.Length >= minimum,
                $" must be at least { minimum } character{ (minimum > 1 ? "s" : "") } long");
        }

        protected Rule<string> HasNoMoreCharactersThan(int maximum)
        {
            return new Rule<string>(str => str.Length <= maximum,
                $" must be no more than { maximum } characters long");
        }

        protected Rule<string> IsFormattedLike(string regex, string formLabel = null)
        {
            return new Rule<string>(str => Regex.Match(str, regex).Success,
                !string.IsNullOrWhiteSpace(formLabel)
                    ? " must be of the form " + formLabel
                    : " did not match required format");
        }

        protected Rule<string> IsFormattedLikeAGuid()
        {
            return new Rule<string>(str => Guid.TryParseExact(str, "D", out Guid unused),
                " must be a GUID");
        }

        #endregion

        #region array Rules

        protected Rule<IEnumerable<T>> HasNoFewerItemsThan<T>(int minimum, string itemLabel = "")
        {
            return new Rule<IEnumerable<T>>(collection => collection.Count() >= minimum,
                $" must contain at least { minimum } { itemLabel }");
        }

        protected Rule<IEnumerable<T>> HasNoMoreItemsThan<T>(int maximum, string itemLabel = "")
        {
            return new Rule<IEnumerable<T>>(collection => collection.Count() <= maximum,
                $" must contain no more than { maximum } { itemLabel }");
        }

        protected Rule<IEnumerable<T>> HasNoDuplicate<T>(string pluralPropertyLabel = "items", Func<T, dynamic> propertySelector = null)
        {
            propertySelector = propertySelector ?? (x => x);
            Func<T, dynamic> safePropertySelector = item => {
                dynamic propertyValue;
                try { propertyValue = propertySelector(item); }
                catch { propertyValue = null; }
                return propertyValue;
            };
            return new Rule<IEnumerable<T>>(collection => collection.Count() == collection
                .Select(safePropertySelector)
                .Distinct()
                .Count(),
                $" must have unique { pluralPropertyLabel }");
        }

        #endregion

        #region number Rules

        protected Rule<decimal?> IsGreaterThanOrEqualTo(decimal? minimum)
        {
            return new Rule<decimal?>((value) =>
                !value.HasValue || value >= minimum,
                $" must be greater than or equal to {minimum}",
                false);
        }

        protected Rule<decimal?> IsLessThanOrEqualTo(decimal? maximum)
        {
            return new Rule<decimal?>((value) =>
                !value.HasValue || value <= maximum,
                $" must be less than or equal to {maximum}",
                false);
        }

        protected Rule<long?> IsGreaterThanOrEqualTo(long minimum)
        {
            return new Rule<long?>((value) =>
                !value.HasValue || value >= minimum,
                $" must be greater than or equal to {minimum}",
                false);
        }

        protected Rule<long?> IsLessThanOrEqualTo(long maximum)
        {
            return new Rule<long?>((value) =>
                !value.HasValue || value <= maximum,
                $" must be less than or equal to {maximum}",
                false);
        }

        #endregion
    }
}
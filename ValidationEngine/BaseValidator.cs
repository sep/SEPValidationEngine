using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace SEP.ValidationEngine
{
    /// <summary>A parent class to provide helper methods for some common validator rules</summary>
    public abstract class BaseValidator
    {
        /// <summary>Applies a given set of validations to each element of a given collection.</summary>
        /// <param name="collectionLambda">A function that returns the collection; will default to the empty array in the event of a runtime error</param>
        /// <param name="validationMethod">A function that returns a validation status given an item in the collection and the item's index</param>
        /// <returns>The merged result of executing the validation method on each item in the collection</returns>
        protected ValidationStatus ForEach(Func<IEnumerable<dynamic>> collectionLambda, Func<dynamic, int, ValidationStatus> validationMethod)
        {
            IEnumerable<dynamic> collection;
            try { collection = collectionLambda() ?? new dynamic[0]; }
            catch { collection = new dynamic[0]; }
            return ValidationStatus.MergeAll(collection.Select(validationMethod).ToArray());
        }

        #region object Rules

        /// <summary>Constructs a test that a value is not null</summary>
        /// <returns>A new rule that fails if given a null value or on a runtime error</returns>
        protected Rule<T> IsNotNull<T>()
        {
            return new Rule<T>(value => null != value,
                " is a required field",
                false);
        }

        /// <summary>Constructs a test that a value is included in a given enumeration of valid values</summary>
        /// <param name="validValues">A collection of all values that are considered valid</param>
        /// <param name="itemLabel">What the user would call an idividual item in the given collection</param>
        /// <returns>A new rule that fails if given a value not in the given collection or on a runtime error</returns>
        protected Rule<T> IsIn<T>(IEnumerable<T> validValues, string itemLabel)
        {
            return new Rule<T>(value => value != null && validValues.Contains(value),
                " must be a " + itemLabel,
                false);
        }

        #endregion

        #region string Rules
        
        /// <summary>Constructs a test that a string has at least the given number of characters</summary>
        /// <param name="minimum">The minimum valid length</param>
        /// <returns>A new rule that fails if given a string that's too short</returns>
        protected Rule<string> HasNoFewerCharactersThan(int minimum)
        {
            return new Rule<string>(str => str.Length >= minimum,
                $" must be at least { minimum } character{ (minimum > 1 ? "s" : "") } long");
        }

        /// <summary>Constructs a test that a string has at most the given number of characters</summary>
        /// <param name="minimum">The maximum valid length</param>
        /// <returns>A new rule that fails if given a string that's too long</returns>
        protected Rule<string> HasNoMoreCharactersThan(int maximum)
        {
            return new Rule<string>(str => str.Length <= maximum,
                $" must be no more than { maximum } characters long");
        }

        /// <summary>Constructs a test that a string matches the given regular expression</summary>
        /// <param name="regex">The pattern that a valid string must have</param>
        /// <param name="formLabel">What the user would call the form of a valid string</param>
        /// <returns>A new rule that fails if given a string doesn't match the regex</returns>
        protected Rule<string> IsFormattedLike(string regex, string formLabel = null)
        {
            return new Rule<string>(str => Regex.Match(str, regex).Success,
                !string.IsNullOrWhiteSpace(formLabel)
                    ? " must be of the form " + formLabel
                    : " did not match required format");
        }

        /// <summary>Constructs a test that a string can be parsed as a Guid</summary>
        /// <returns>A new rule that fails if given a string doesn't parse as a Guid</returns>
        protected Rule<string> IsFormattedLikeAGuid()
        {
            return new Rule<string>(str => Guid.TryParseExact(str, "D", out Guid unused),
                " must be a GUID");
        }

        #endregion

        #region collection Rules

        /// <summary>Constructs a test that a collection has at least the given number of items</summary>
        /// <param name="minimum">The minimum number of items a collection must have to be valid</param>
        /// <param name="itemLabel">What the user would call an item or items (if minimum is more than 1) in the collection</param>
        /// <returns>A new rule that fails if given a collection that's too small</returns>
        protected Rule<IEnumerable<T>> HasNoFewerItemsThan<T>(int minimum, string itemLabel = "")
        {
            return new Rule<IEnumerable<T>>(collection => collection.Count() >= minimum,
                $" must contain at least { minimum } { itemLabel }");
        }

        /// <summary>Constructs a test that a collection has at most the given number of items</summary>
        /// <param name="minimum">The maximum number of items a collection may have to be valid</param>
        /// <param name="itemLabel">What the user would call an item or items (if maximum is more than 1) in the collection</param>
        /// <returns>A new rule that fails if given a collection that's too large</returns>
        protected Rule<IEnumerable<T>> HasNoMoreItemsThan<T>(int maximum, string itemLabel = "")
        {
            return new Rule<IEnumerable<T>>(collection => collection.Count() <= maximum,
                $" must contain no more than { maximum } { itemLabel }");
        }

        /// <summary>Constructs a test that a collection's items are unique with respect to their identity or a particular property</summary>
        /// <param name="pluralPropertyLabel">(Optional) What the user would call more than one of the items in the collection; defaults to "items"</param>
        /// <param name="propertySelector">(Optional) A function that takes an item in the collection and returns the property that must to be unique across the collection; defaults to the identify function</param>
        /// <returns>A new rule that fails if given a collection that has duplicate items</returns>
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

        /// <summary>Constructs a test that a number is greater than or equal to the given number</summary>
        /// <param name="minimum">The lowest valid number</param>
        /// <returns>A new rule that fails if given a number that is null or too large</returns>
        protected Rule<decimal?> IsGreaterThanOrEqualTo(decimal? minimum)
        {
            return new Rule<decimal?>((value) =>
                !value.HasValue || value >= minimum,
                $" must be greater than or equal to {minimum}",
                false);
        }

        /// <summary>Constructs a test that a number is less than or equal to the given number</summary>
        /// <param name="minimum">The greatest valid number</param>
        /// <returns>A new rule that fails if given a number that is null or too small</returns>
        protected Rule<decimal?> IsLessThanOrEqualTo(decimal? maximum)
        {
            return new Rule<decimal?>((value) =>
                !value.HasValue || value <= maximum,
                $" must be less than or equal to {maximum}",
                false);
        }

        /// <summary>Constructs a test that a number is greater than or equal to the given number</summary>
        /// <param name="minimum">The lowest valid number</param>
        /// <returns>A new rule that fails if given a number that is null or too large</returns>
        protected Rule<long?> IsGreaterThanOrEqualTo(long minimum)
        {
            return new Rule<long?>((value) =>
                !value.HasValue || value >= minimum,
                $" must be greater than or equal to {minimum}",
                false);
        }

        /// <summary>Constructs a test that a number is greater than or equal to the given number</summary>
        /// <param name="minimum">The lowest valid number</param>
        /// <returns>A new rule that fails if given a number that is null or too large</returns>
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
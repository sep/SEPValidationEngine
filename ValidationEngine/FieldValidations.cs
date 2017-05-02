using System;
using System.Collections.Generic;
using System.Linq;

namespace SEP.ValidationEngine
{
    /// <summary>This class encapsulates the necessary information to evaluate a set of business requirement related to a field of a particular type, `T`.</summary>
    public class FieldValidations<T>
    {
        private string Key;
        private IList<Rule<T>> Rules;

        /// <summary>Constructs (but does not run) a set of business rule for a particular field of type `T`.</summary>
        /// <param name="key">The string by which the field is identified</param>
        public FieldValidations(string key)
        {
            Key = key;
            Rules = new List<Rule<T>>();
        }

        /// <summary>Appends the given rule to the list of rules to execute when this field validations is run. The given rule will be run against the field's value.</summary>
        /// <param name="rule">Rule to add to this field validations</param>
        /// <returns>This field validations (for dot-chaining)</returns>
        public FieldValidations<T> ThatIt(Rule<T> rule)
        {
            Rules.Add(rule);
            return this;
        }

        /// <summary>Appends a new rule to the list of rules to execute when this field validations is run based on the given parameters.</summary>
        /// <param name="predicateLambda">Predicate to use to construct the new rule (NOTE: this means the rule will not use the field's value directly unless the predicate handles that internally.)</param>
        /// <param name="message">What the user should see if the rule is not satisfied</param>
        /// <param name="isSatisfiedOnError">Sepcifies the behavior of the rule in the event of a runtime error</param>
        /// <returns>This field validations (for dot-chaining)</returns>
        public FieldValidations<T> That(Func<bool> predicateLambda,
            string message,
            bool isSatisfiedOnError = true)
        {
            return ThatIt(new Rule<T>((unused) => predicateLambda(), message, isSatisfiedOnError));
        }

        /// <summary>Executes the configured rules against the value returned by the given lambda.</summary>
        /// <param name="valueLambda">A function that returns the field's value for validation (NOTE: runtime errors in the evaluation of this lambda will cause it to return the default for the type `T`)</param>
        /// <param name="messagePrefix">(Optional) A bit of text to prepend to any error messages</param>
        /// <returns>The merged result of executing all the configured rules</returns>
        public ValidationStatus RunAgainst(Func<T> valueLambda,
            string messagePrefix = null)
        {
            T value;
            try { value = valueLambda(); }
            catch { value = default(T); }
            return ValidationStatus.MergeAll(
                Rules.Select(rule => rule.Run(value, Key, messagePrefix)).ToArray());
        }
    }
}

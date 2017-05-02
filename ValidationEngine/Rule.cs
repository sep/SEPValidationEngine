using System;
using System.Collections.Generic;

namespace SEP.ValidationEngine
{
    /// <summary>This class encapsulates the necessary information to evaluate an individual business requirement related to a value of a particular type, `T`.</summary>
    public class Rule<T>
    {
        private Predicate<T> Predicate;
        private string MessageTemplate;
        private bool IsSatisfiedOnError;
        private List<T> ExplicitlyValidValues;

        /// <summary>Constructs (but does not run) a business rule for an individual value.</summary>
        /// <param name="predicate">A lambda that must evaluate to true iff the rule is satisfied</param>
        /// <param name="messageTemplate">What the user should see if the rule is not satisfied. By convention, assume the field name (or equivalent) will be prepended elsewhere.</param>
        /// <param name="isSatisfiedOnError">(Optional) False, to fail the rule if the given predicate throws a runtime error. Defaults to true to cut down on null checks.</param>
        public Rule(Predicate<T> predicate,
            string messageTemplate,
            bool isSatisfiedOnError = true)
        {
            Predicate = predicate;
            MessageTemplate = messageTemplate;
            IsSatisfiedOnError = isSatisfiedOnError;
            ExplicitlyValidValues = new List<T>();
        }

        /// <summary>Appends the given values to an internal list of values that explicitly satisfy the rule. Values added to this list will NOT be evaulated against the rule's predicate.</summary>
        /// <param name="allowedValues">Value(s) to be explicitly valid when the rule is run</param>
        /// <returns>This rule (for dot-chaining)</returns>
        public Rule<T> OrItIs(params T[] allowedValues)
        {
            ExplicitlyValidValues.AddRange(allowedValues);
            return this;
        }

        /// <summary>Executes the rule against the given value.</summary>
        /// <param name="value">Value to run the rule against</param>
        /// <param name="key">(Optional) Key the given value (to associate with the error messages in the status), defaults to the empty string</param>
        /// <param name="messagePrefix">(Optional) Prefix for the error message, defaults to the empty string</param>
        /// <returns>A success status iff the given value is explicitly allowed, satisfies the predicate, or throws an error (for rules that are satisfied on error). Otherwise, an invalid status with the appropriate message.</returns>
        public ValidationStatus Run(T value,
            string key = null,
            string messagePrefix = null)
        {
            key = key ?? string.Empty;
            messagePrefix = messagePrefix ?? string.Empty;
            bool ruleSatisfied = ExplicitlyValidValues.Contains(value);
            try { ruleSatisfied = ruleSatisfied || Predicate(value); }
            catch { ruleSatisfied = IsSatisfiedOnError; }
            return ValidationStatus.Evaluate(ruleSatisfied, messagePrefix + MessageTemplate, key);
        }
    }
}

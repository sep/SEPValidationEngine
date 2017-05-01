using System;
using System.Collections.Generic;

namespace SEP.ValidationEngine
{
    public class Rule<T>
    {
        private Predicate<T> Predicate;
        private string MessageTemplate;
        private bool IsSatisfiedOnError;
        private List<T> ExplicitlyValidValues;

        public Rule(Predicate<T> predicate,
            string messageTemplate,
            bool isSatisfiedOnError = true)
        {
            Predicate = predicate;
            MessageTemplate = messageTemplate;
            IsSatisfiedOnError = isSatisfiedOnError;
            ExplicitlyValidValues = new List<T>();
        }

        public Rule<T> OrItIs(params T[] allowedValues)
        {
            ExplicitlyValidValues.AddRange(allowedValues);
            return this;
        }

        public ValidationStatus Run(T value, string key = null, string messagePrefix = null)
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
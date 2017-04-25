using System;
using System.Collections.Generic;
using System.Linq;

namespace SEP.ValidationEngine
{
    public class FieldValidations<T>
    {
        private string Key;
        private IList<Rule<T>> Rules;

        public FieldValidations(string key)
        {
            Key = key;
            Rules = new List<Rule<T>>();
        }

        public FieldValidations<T> ThatIt(Rule<T> rule)
        {
            Rules.Add(rule);
            return this;
        }

        public FieldValidations<T> That(Func<bool> predicateLambda,
            string message,
            bool isSatisfiedOnError = true)
        {
            return ThatIt(new Rule<T>((unused) => predicateLambda(), message, isSatisfiedOnError));
        }

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
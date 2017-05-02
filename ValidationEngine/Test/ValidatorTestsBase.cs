using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Dynamic;
using System.Linq;

namespace SEP.ValidationEngine.Test
{
    [TestClass]
    /// <summary>A parent class to provide helper methods for some common validator test needs</summary>
    public abstract class ValidatorTestsBase<TValidator> where TValidator : BaseValidator, new()
    {
        /// <summary>The class under test, a subclass of BaseValidator</summary>
        protected TValidator Validator;

        /// <summary>The data that will be validated</summary>
        protected dynamic Data;

        [TestInitialize]
        /// <summary>The test initializer</summary>
        public virtual void Setup()
        {
            Validator = new TValidator();
            Data = new ExpandoObject();
        }

        /// <summary>Asserts that the given result contains only one error message grouped by the given key and containing the given substring. Pretty prints the actual errors on assertion failure.</summary>
        /// <param name="result">The result under test</param>
        /// <param name="expectedKey">The key which should reference the only group of error messages in the result</param>
        /// <param name="expectedSubString">A portion of the error message which should be the only error message in the result</param>
        protected void AssertSingleDataError(ValidationStatus result, string expectedKey, string expectedSubString)
        {
            Assert.AreEqual(OverallStatus.DataError, result.Disposition);
            try
            {
                var kvp = result.ErrorsByKey.SingleOrDefault();
                Assert.AreEqual(expectedKey, kvp.Key,
                    AllErrorsPrettyPrinted(result));
                StringAssert.Contains(kvp.Value.SingleOrDefault(), expectedSubString,
                    AllErrorsPrettyPrinted(result));
            }
            catch (InvalidOperationException)
            {
                Assert.Fail("There were too many errors."
                    + Environment.NewLine + AllErrorsPrettyPrinted(result));
            }
        }

        /// <summary>Asserts that the given result contains an error grouped by the given key which contains the given substring. Pretty prints the actual errors on assertion failure.</summary>
        /// <param name="result">The result under test</param>
        /// <param name="expectedKey">The key which should reference a group of error messages in the result</param>
        /// <param name="expectedSubString">A portion of an error message which should be keyed by the given key</param>
        protected void AssertSomeDataError(ValidationStatus result, string expectedKey, string expectedSubString)
        {
            Assert.AreEqual(OverallStatus.DataError, result.Disposition);
            Assert.IsTrue(result.MessagesFor(expectedKey)
                .Any(message => message.Contains(expectedSubString)),
                $"Expected error containing { expectedSubString } not found. "
                    + AllErrorsPrettyPrinted(result));
        }

        /// <summary>Asserts that the given result has a valid disposition and no error messages. Pretty prints the actual errors on assertion failure.</summary>
        /// <param name="result">The result under test</param>
        protected void AssertValidStatus(ValidationStatus result)
        {
            Assert.IsTrue(result.IsSuccess, "Expected success"
                + Environment.NewLine + AllErrorsPrettyPrinted(result));
            Assert.AreEqual(0, result.ErrorsByKey.Count,
                AllErrorsPrettyPrinted(result));
        }

        /// <summary>Joins the errors in the given result.</summary>
        /// <param name="result">The result under test</param>
        /// <returns>The errors for each key formatted across multiple lines with reasonable indentation</returns>
        protected string AllErrorsPrettyPrinted(ValidationStatus result)
        {
            return "Errors were: " + Environment.NewLine
                + string.Join(Environment.NewLine, result.ErrorsByKey
                    .Select(kvp => kvp.Key + Environment.NewLine
                         + string.Join(Environment.NewLine, kvp.Value.Select(message => 
                        "\t" + message))));
        }
    }

}

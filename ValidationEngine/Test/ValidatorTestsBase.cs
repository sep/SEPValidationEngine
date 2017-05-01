using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Dynamic;
using System.Linq;

namespace SEP.ValidationEngine.Test
{
    [TestClass]
    public abstract class ValidatorTestsBase<TValidator> where TValidator : BaseValidator, new()
    {
        protected TValidator Validator;
        protected dynamic Data;

        [TestInitialize]
        public virtual void Setup()
        {
            Validator = new TValidator();
            Data = new ExpandoObject();
        }

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

        protected void AssertSomeDataError(ValidationStatus result, string expectedKey, string expectedSubString)
        {
            Assert.AreEqual(OverallStatus.DataError, result.Disposition);
            Assert.IsTrue(result.MessagesFor(expectedKey)
                .Any(message => message.Contains(expectedSubString)),
                $"Expected error containing { expectedSubString } not found. "
                    + AllErrorsPrettyPrinted(result));
        }

        protected void AssertValidStatus(ValidationStatus result)
        {
            Assert.IsTrue(result.IsSuccess, "Expected success"
                + Environment.NewLine + AllErrorsPrettyPrinted(result));
            Assert.AreEqual(0, result.ErrorsByKey.Count,
                AllErrorsPrettyPrinted(result));
        }

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

using HelpDesk.BBL.ExtensionMethods;
using NUnit.Framework;

namespace HelpDesk.Tests
{
    [TestFixture]
    public class ExtensionMethodsTests
    {
        [Test]
        public void RemoveExcessSpaces_NullAsParam_ReturnsNull()
        {
            string input = null;

            string actual = input.RemoveExcessSpaces();

            Assert.IsNull(actual);
        }

        [Test]
        public void RemoveExcessSpaces_EmptyStringAsParam_ReturnsEmptyString()
        {
            string input = "";

            string actual = input.RemoveExcessSpaces();

            Assert.AreEqual("", actual);
        }

        [Test]
        public void RemoveExcessSpaces_StringWithoutTooManySpacesAsParam_ReturnsUnchangedString()
        {
            string input = "Not too many spaces";

            string actual = input.RemoveExcessSpaces();

            Assert.AreEqual(input, actual);
        }

        [TestCase(" Too many spaces ")]
        [TestCase("Too   many   spaces")]
        [TestCase("   Too   many   spaces   ")]
        public void RemoveExcessSpaces_StringWithTooManySpacesAsParam_ReturnsTrimmedString(string input)
        {
            string actual = input.RemoveExcessSpaces();

            Assert.AreEqual("Too many spaces", actual);
        }
    }
}

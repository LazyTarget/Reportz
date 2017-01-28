using System;
using NUnit.Framework;
using Reportz.Scripting.Classes;

namespace Reportz.Tests.ScriptingTests
{
    [TestFixture]
    public class ExpressionEvaluatorTests
    {
        [Test]
        public void SingleExpression_SystemKey_DateTime()
        {
            var sut = new ExpressionEvaluator();
            object expected = DateTime.Today;
            object actual = sut.EvaluateExpression("$$today");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SingleExpression_SystemKey_Null()
        {
            var sut = new ExpressionEvaluator();
            object expected = null;
            object actual = sut.EvaluateExpression("$$null");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SingleExpression_SystemKey_Empty()
        {
            var sut = new ExpressionEvaluator();
            object expected = string.Empty;
            object actual = sut.EvaluateExpression("$$empty");
            Assert.AreEqual(expected, actual);
        }


        [Test]
        public void SingleWrappedExpression_SystemKey_DateTime()
        {
            var sut = new ExpressionEvaluator();
            object expected = DateTime.Today;
            object actual = sut.EvaluateExpression("${$$today}");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SingleWrappedExpression_SystemKey_Null()
        {
            var sut = new ExpressionEvaluator();
            object expected = null;
            object actual = sut.EvaluateExpression("${$$null}");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SingleWrappedExpression_SystemKey_Empty()
        {
            var sut = new ExpressionEvaluator();
            object expected = string.Empty;
            object actual = sut.EvaluateExpression("${$$empty}");
            Assert.AreEqual(expected, actual);
        }


        [Test]
        public void SingleWrappedInterpolatedExpression_SystemKey_DateTime()
        {
            var sut = new ExpressionEvaluator();
            object expected = $"Current day is: {DateTime.Today}";
            object actual = sut.EvaluateExpression("Current day is: ${$$today}");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SingleWrappedInterpolatedExpression_SystemKey_Null()
        {
            var sut = new ExpressionEvaluator();
            object expected = $"Current day is: {null}";
            object actual = sut.EvaluateExpression("Current day is: ${$$null}");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SingleWrappedInterpolatedExpression_SystemKey_Empty()
        {
            var sut = new ExpressionEvaluator();
            object expected = $"Current day is: {string.Empty}";
            object actual = sut.EvaluateExpression("Current day is: ${$$empty}");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MultipleWrappedInterpolatedExpression_SystemKey_Empty()
        {
            var sut = new ExpressionEvaluator();
            object expected = $"Current day is: {string.Empty}";
            object actual = sut.EvaluateExpression("Current day is: ${$$empty}, this statement is: ${$$true}. Or is it ${$$false}?");
            Assert.AreEqual(expected, actual);
        }


    }
}

using System;
using System.Globalization;
using NUnit.Framework;
using Reportz.Scripting;
using Reportz.Scripting.Classes;

namespace Reportz.Tests.ScriptingTests
{
    [TestFixture]
    public partial class ScriptParserTests
    {
        [Test]
        public void SetTypedVariable_WithVarAlias_InScriptDocument()
        {
            var today = DateTime.Today;

            var scriptXml =
                $@"
<document>
	<execute>
		<var key=""date1"" type=""DateTime"">{today}</var>
        <var key=""date2"" type=""DateTime"">{today.ToString("u")}</var>
        <var key=""date3"" type=""DateTime"">{today.ToString("s")}</var>
        <var key=""date4"" type=""DateTime"">{today.ToString("yyyy-MM-dd")}</var>
        <var key=""date5"" type=""DateTime"" value=""{today.ToString("yyyy-MM-dd")}""></var>
        <var key=""date6"" type=""DateTime"" value=""{today.ToString("yyyy-MM-dd")}"" />
	</execute>
</document>
";
            var scriptParser = new ScriptParser();
            var scriptDoc = scriptParser.ParseDocument(scriptXml);

            var args = new ExecutableArgs();
            args.Scope = new VariableScope();

            var result = scriptDoc.Execute(args);

            // Assert
            var actualDate1 = args.Scope.GetVarValue<DateTime>("date1");
            var actualDate2 = args.Scope.GetVarValue<DateTime>("date2").ToUniversalTime();
            var actualDate3 = args.Scope.GetVarValue<DateTime>("date3");
            var actualDate4 = args.Scope.GetVarValue<DateTime>("date4");
            var actualDate5 = args.Scope.GetVarValue<DateTime>("date5");
            var actualDate6 = args.Scope.GetVarValue<DateTime>("date6");
            Assert.AreEqual(today, actualDate1);
            Assert.AreEqual(today, actualDate2);
            Assert.AreEqual(today, actualDate3);
            Assert.AreEqual(today, actualDate4);
            Assert.AreEqual(today, actualDate5);
            Assert.AreEqual(today, actualDate6);
        }


        [Test]
        public void SetTypedVariable_WithVariableAlias_InScriptDocument()
        {
            var today = DateTime.Today;

            var scriptXml =
                $@"
<document>
	<execute>
		<variable key=""date1"" type=""DateTime"">{today}</variable>
        <variable key=""date2"" type=""DateTime"">{today.ToString("u")}</variable>
        <variable key=""date3"" type=""DateTime"">{today.ToString("s")}</variable>
        <variable key=""date4"" type=""DateTime"">{today.ToString("yyyy-MM-dd")}</variable>
        <variable key=""date5"" type=""DateTime"" value=""{today.ToString("yyyy-MM-dd")}""></variable>
        <variable key=""date6"" type=""DateTime"" value=""{today.ToString("yyyy-MM-dd")}"" />
	</execute>
</document>
";
            var scriptParser = new ScriptParser();
            var scriptDoc = scriptParser.ParseDocument(scriptXml);

            var args = new ExecutableArgs();
            args.Scope = new VariableScope();

            var result = scriptDoc.Execute(args);

            // Assert
            var actualDate1 = args.Scope.GetVarValue<DateTime>("date1");
            var actualDate2 = args.Scope.GetVarValue<DateTime>("date2").ToUniversalTime();
            var actualDate3 = args.Scope.GetVarValue<DateTime>("date3");
            var actualDate4 = args.Scope.GetVarValue<DateTime>("date4");
            var actualDate5 = args.Scope.GetVarValue<DateTime>("date5");
            var actualDate6 = args.Scope.GetVarValue<DateTime>("date6");
            Assert.AreEqual(today, actualDate1);
            Assert.AreEqual(today, actualDate2);
            Assert.AreEqual(today, actualDate3);
            Assert.AreEqual(today, actualDate4);
            Assert.AreEqual(today, actualDate5);
            Assert.AreEqual(today, actualDate6);
        }


        [Test]
        public void SetTypedVariable_InScriptScope()
        {
            var today = DateTime.Today;

            var scriptXml =
                $@"
<document>
	<execute>
        <script>
		    <variable key=""date1"" type=""DateTime"">{today}</variable>
            <variable key=""date2"" type=""DateTime"">{today.ToString("u")}</variable>
            <variable key=""date3"" type=""DateTime"">{today.ToString("s")}</variable>
            <variable key=""date4"" type=""DateTime"">{today.ToString("yyyy-MM-dd")}</variable>
            <variable key=""date5"" type=""DateTime"" value=""{today.ToString("yyyy-MM-dd")}""></variable>
            <variable key=""date6"" type=""DateTime"" value=""{today.ToString("yyyy-MM-dd")}"" />
        </script>
	</execute>
</document>
";
            var scriptParser = new ScriptParser();
            var scriptDoc = scriptParser.ParseDocument(scriptXml);

            var rootArgs = new ExecutableArgs();
            rootArgs.Scope = new VariableScope();

            var result = scriptDoc.Execute(rootArgs);

            // Assert
            var args = result.Args;
            var actualDate1 = args.Scope.GetVarValue<DateTime>("date1");
            var actualDate2 = args.Scope.GetVarValue<DateTime>("date2").ToUniversalTime();
            var actualDate3 = args.Scope.GetVarValue<DateTime>("date3");
            var actualDate4 = args.Scope.GetVarValue<DateTime>("date4");
            var actualDate5 = args.Scope.GetVarValue<DateTime>("date5");
            var actualDate6 = args.Scope.GetVarValue<DateTime>("date6");
            Assert.AreEqual(today, actualDate1);
            Assert.AreEqual(today, actualDate2);
            Assert.AreEqual(today, actualDate3);
            Assert.AreEqual(today, actualDate4);
            Assert.AreEqual(today, actualDate5);
            Assert.AreEqual(today, actualDate6);
        }

    }
}

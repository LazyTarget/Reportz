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
        public void ExecuteScriptWithVariables()
        {
            var date1 = DateTime.Today;
            var dtfi = new DateTimeFormatInfo();
            dtfi.ShortDatePattern = "yyyy-MM-dd";
            dtfi.LongDatePattern = "dd MMMM yyyy";
            dtfi.FirstDayOfWeek = DayOfWeek.Monday;

            var scriptXml =
                $@"
<script>
	<execute>
		<variable key=""dtfi"">
			<instantiate>
				<type>{typeof(DateTimeFormatInfo).AssemblyQualifiedName}</type>
				<ctor>
					<arguments>
					</arguments>
				</ctor>
				<properties>
					<add key=""{nameof(DateTimeFormatInfo.ShortDatePattern)}"" type=""string"">{dtfi.ShortDatePattern}</add>
					<add key=""{nameof(DateTimeFormatInfo.LongDatePattern)}"" type=""string"">{dtfi.LongDatePattern}</add>
					<add key=""{nameof(DateTimeFormatInfo.FirstDayOfWeek)}"" type=""{typeof(DayOfWeek).AssemblyQualifiedName}"">{dtfi.FirstDayOfWeek}</add>
				</properties>
			</instantiate>
		</variable>
    
		<variable key=""date1"" type=""DateTime"">{date1.ToString("yyyy-MM-dd")}</variable>
	</execute>
</script>
";
            var scriptParser = new ScriptParser();
            var scriptDoc = scriptParser.ParseDocument(scriptXml);

            var args = new ExecutableArgs();
            args.Scope = new VariableScope();

            var result = scriptDoc.Execute(args);
            
            
            // Assert
            var actualDtfi = args.Scope.GetVarValue<DateTimeFormatInfo>("dtfi");
            Assert.AreEqual(dtfi.LongDatePattern, actualDtfi.LongDatePattern);
            Assert.AreEqual(dtfi.ShortDatePattern, actualDtfi.ShortDatePattern);
            Assert.AreEqual(dtfi.FirstDayOfWeek, actualDtfi.FirstDayOfWeek);

            var actualDate1 = args.Scope.GetVarValue<DateTime>("date1");
            Assert.AreEqual(date1, actualDate1);
        }

    }
}

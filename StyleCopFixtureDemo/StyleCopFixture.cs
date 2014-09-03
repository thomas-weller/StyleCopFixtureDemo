// ************************************************************************************************
// <copyright file="StyleCopFixture.cs" company="Thomas Weller Software-Entwicklung">
//   This sample code is provided AS IS without any warranty. There is no copyright 
//   or license whatsoever, so you may use it in any way you like...
// </copyright>
// <authors>
//   <author>Thomas Weller</author>
// </authors>
// <Subversion>
//   <LastChangedDate>$Date: 2009-11-27 12:29:27 +0100 (Fri, 27 Nov 2009) $</LastChangedDate>
//   <LastChangedBy>$Author: Thomas $</LastChangedBy>
//   <LastChangedInRev>$Rev: 1844 $</LastChangedInRev>
// </Subversion>
// <Summary/>
// ************************************************************************************************

using NUnit.Framework;

namespace StyleCopFixtureDemo
{
    [TestFixture, Description("SA1200: UsingDirectivesMustBePlacedWithinNamespace")]
    public class StyleCopFixture : StyleCopFixtureBase
    {
        #region Tests

        [Test, Description("SA1200: UsingDirectivesMustBePlacedWithinNamespace")]
        [TestCase("../../TestFiles/UsingDirectiveOnTop.Testfile.cs")]
        public void UsingDirectivesMustBePlacedWithinNamespace_ByRuleName(string file)
        {
            base.AddSourceFile(file);

            base.RunAnalysis();

            base.AssertViolated(3, "UsingDirectivesMustBePlacedWithinNamespace");
        }

        [Test, Description("SA1200: UsingDirectivesMustBePlacedWithinNamespace")]
        [TestCase("../../TestFiles/UsingDirectiveOnTop.Testfile.cs")]
        public void UsingDirectivesMustBePlacedWithinNamespace_ByRuleNameAndLineNumbers(string file)
        {
            base.AddSourceFile(file);

            base.RunAnalysis();

            base.AssertViolated("UsingDirectivesMustBePlacedWithinNamespace", 1, 2, 3);
        }

        [Test, Description("SA1200: UsingDirectivesMustBePlacedWithinNamespace")]
        [TestCase("../../TestFiles/UsingDirectiveOnTop.Testfile.cs")]
        public void UsingDirectivesMustBePlacedWithinNamespace_ByPredicate(string file)
        {
            base.AddSourceFile(file);

            base.RunAnalysis();

            base.AssertViolated(@violation => @violation.Rule.CheckId == "SA1200");
        }

        [Test, Description("SA1200: UsingDirectivesMustBePlacedWithinNamespace")]
        [TestCase("../../TestFiles/UsingDirectiveNotOnTop.Testfile.cs")]
        public void UsingDirectivesMustBePlacedWithinNamespaceNotViolated_ByRuleName(string file)
        {
            base.AddSourceFile(file);

            base.RunAnalysis();

            base.AssertNotViolated("UsingDirectivesMustBePlacedWithinNamespace");
        }

        [Test, Description("SA1200: UsingDirectivesMustBePlacedWithinNamespace")]
        public void TestUsingDirectivesMustBePlacedWithinNamespace()
        {
            base.AddSourceFile("../../TestFiles/UsingDirectiveOnTop.Testfile.cs");

            base.RunAnalysis();

            base.AssertViolated("UsingDirectivesMustBePlacedWithinNamespace");

            Assert.AreEqual(3, Violations.Count);
            Assert.AreEqual(2, Output.Count);
        }

        [Test, Description("SA1200: UsingDirectivesMustBePlacedWithinNamespace")]
        public void TestUsingDirectivesMustBePlacedWithinNamespace_Failing()
        {
            base.AddSourceFile("../../TestFiles/UsingDirectiveOnTop.Testfile.cs");

            base.RunAnalysis();

            base.AssertViolated(4, "UsingDirectivesMustBePlacedWithinNamespace");
        }

        #endregion // Tests

    } // class StyleCopFixture

} // namespace StyleCopFixtureDemo
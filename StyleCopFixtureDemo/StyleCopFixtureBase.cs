// ************************************************************************************************
// <copyright file="StyleCopFixtureBase.cs" company="Thomas Weller Software-Entwicklung">
//   This sample code is provided AS IS without any warranty. There is no copyright 
//   or license whatsoever, so you may use it in any way you like...
// </copyright>
// <authors>
//   <author>Thomas Weller</author>
// </authors>
// <Subversion>
//   <LastChangedDate>$Date: 2009-11-28 03:17:11 +0100 (Sat, 28 Nov 2009) $</LastChangedDate>
//   <LastChangedBy>$Author: Thomas $</LastChangedBy>
//   <LastChangedInRev>$Rev: 1845 $</LastChangedInRev>
// </Subversion>
// <Summary/>
// ************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using StyleCop;

namespace StyleCopFixtureDemo
{
    /// <summary>
    /// Base class for NUnit fixture that tests MS StyleCop rules.
    /// </summary>
    /// <remarks>
    /// This class runs MS StyleCop and allows each individual test to add the
    /// files it needs. A normal test will be as simple as in the example below.
    /// Basically you add the file you need, run the analysis and check the results.
    /// <para>
    /// This is inspired by code that is written by Sergey Shishkin (initially for MSTest). The blog post can be
    /// found <see href="http://sergeyshishkin.spaces.live.com/blog/cns!9F19E53BA9C1D63F!232.entry">here</see>.
    /// </para>
    /// <para>
    /// The class exposes three virtual properties (<see cref="StyleCopFolder"/>, <see cref="AddInsFolder"/>, 
    /// and <see cref="Settings"/>), that can be used to accomodate the actual test projects, if needed. However,
    /// their respective default values are such that they assume the 'normal' scenario (see below), and in
    /// practice, the need to override them will occur only in rare cases.<br/>
    /// The properties assume the following test setup:
    /// <list type="bullet">
    /// <item><description>
    /// The test project has references to the assembly that contain the custom <i>StyleCop</i> rules to test.
    /// </description></item>
    /// <item><description>
    /// This rules assembly has references to the <c>Microsoft.StyleCop</c> and <c>Microsoft.StyleCop.Csharp</c>
    /// assemblies (which contain <i>StyleCop</i>'s core engine).
    /// </description></item>
    /// <item><description>
    /// All references are set to <c>Copy local</c> (the default).
    /// </description></item>
    /// <item><description>
    /// The test project contains a <i>StyleCop</i> settings file (<c>Settings.StyleCop</c>) that
    /// has its <c><i>Copy to Output Directory</i></c> property set.
    /// </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The class provides also some assertion methods to verify the test outcomes.
    /// </para>
    /// </remarks>
    /// <example>
    /// This example shows the simplest possible usage of the <c>StyleCopFixtureBase</c> class:
    /// <code>
    /// [Test] 
    /// public void TestSomeRule() 
    /// { 
    ///     base.AddSourceFile("Somefile.cs"); 
    /// 
    ///     base.RunAnalysis(); 
    /// 
    ///     base.AssertNotViolated("SomeRule"); 
    /// } 
    /// </code>
    /// </example>
    public abstract class StyleCopFixtureBase
    {
        #region Constants

        /// <summary>
        /// Filename of StyleCop's settings file.
        /// </summary>
        private const string SettingsFilename = "Settings.StyleCop";

        #endregion // Constants

        #region Fields

        private readonly List<string> addinPaths = new List<string>();
        private readonly List<string> output = new List<string>();
        private readonly List<Violation> violations = new List<Violation>();
        private StyleCopConsole console;
        private CodeProject project;

        #endregion // Fields

        #region Properties

        #region StyleCop Results

        /// <summary>
        /// Gets the violations that the last <i>StyleCop</i> analysis run has found.
        /// </summary>
        /// <value>The violations.</value>
        protected List<Violation> Violations
        {
            get { return this.violations; }
        }

        /// <summary>
        /// Gets the output from the last <i>StyleCop</i> analysis run.
        /// </summary>
        /// <value>The output.</value>
        protected List<string> Output
        {
            get { return this.output; }
        }

        #endregion // StyleCop Results

        /// <summary>
        /// Gets the folder where <i>MS StyleCop</i> is installed.
        /// </summary>
        /// <remarks>
        /// The base class version of this property returns <see langword="null" />, which is
        /// OK for the normal test scenario.
        /// <para>
        ///  Override this property only if your test setup is different from the scenario described
        ///  in the class description, since normally <i>MS StyleCop</i> will be loaded from the
        /// <c>bin/Debug</c> directory.</para>
        /// </remarks>
        /// <value>The <i>MS StyleCop</i> installation folder.</value>
        /// <seealso cref="AddInsFolder"/>
        protected virtual string StyleCopFolder
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the folder from which <i>MS StyleCop</i> addins (aka. rules
        /// assemblies) are loaded.
        /// </summary>
        /// <remarks>
        /// This property defaults to the directory of the assembly that contains the currently 
        /// executed test (usually <c>&lt;projdir&gt;/bin/Debug</c>). The assumption is that - 
        /// since the test assembly must reference the assembly that contains the rules to be
        /// tested - the rules assembly can be found here as well. It is also assumed that the
        /// required <i>MS StyleCop</i> dll(s) are located here because they have to be
        /// referenced as well.<para>
        /// Override this property only if your test setup is different from the scenario
        /// described in the class description.</para>
        /// </remarks>
        /// <value>The addins folder.</value>
        /// <seealso cref="StyleCopFolder"/>
        protected virtual string AddInsFolder
        {
            get
            {
                // get the assembly folder - usually 'bin\Debug' (use
                // 'CodeBase' and not 'GetExecutingAssembly()' because
                //  MbUnit works with a shadow copy...)
                string directory =
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);

                // remove the 'file:\\' prefix
                return directory.Substring(6);
            }
        }

        /// <summary>
        /// Gets the full path to the settings file (<c>Settings.StyleCop</c>) used by the tests.
        /// </summary>
        /// <remarks>
        /// This property defaults to the directory of the assembly that contains the currently 
        /// executed test - usually it will return <c>&lt;projdir&gt;/bin/Debug/Settings.StyleCop</c>.
        /// It assumes that the test project has its own settings file that has its <i>Copy to 
        /// output directory</i> property set.
        /// </remarks>
        /// <value>The settings (full path).</value>
        protected virtual string Settings
        {
            get { return Path.GetFullPath(SettingsFilename); }
        }

        #endregion // Properties

        #region Fixture Setup/Teardown

        /// <summary>
        /// Fixture-wise initialization method.
        /// </summary>
        /// <remarks>
        /// Initializes <i>MS StyleCop</i> with the provided settings file and addin
        /// path. Prepares for capturing violations (in <see cref="Violations"/>)
        /// and the generated text output (in <see cref="Output"/>).
        /// </remarks>
        /// <seealso cref="StyleCopFolder"/>
        /// <seealso cref="AddInsFolder"/>
        /// <seealso cref="Settings"/>
        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            this.AddAddInsPath(this.StyleCopFolder);
            this.AddAddInsPath(this.AddInsFolder);

            string settings = this.Settings;
            if (!File.Exists(settings))
            {
                throw new FileNotFoundException(settings);
            }

            this.console = new StyleCopConsole(settings, false, null, this.addinPaths, false);

            this.console.ViolationEncountered += (sender, args) =>
            {
                this.violations.Add(args.Violation);
                Console.WriteLine("Rule '{0}' violated at line {1}: {2}",
                                  args.Violation.Rule.CheckId,
                                  args.LineNumber,
                                  args.Message);
            };

            this.console.OutputGenerated += (sender, args) =>
            {
                this.output.Add(args.Output);
                Console.WriteLine(args.Output);
            };
        }

        #endregion // Fixture Setup/Teardown

        #region Setup/Teardown

        /// <summary>
        /// Test-wise initialization method.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            var configuration = new Configuration(new string[0]);
            this.project = new CodeProject(Guid.NewGuid().GetHashCode(), null, configuration);
        }

        /// <summary>
        /// Test-wise cleanup method. Cleans up the current results
        /// (<see cref="Violations"/> and <see cref="Output"/>).
        /// </summary>
        [TearDown]
        public void Teardown()
        {
            this.violations.Clear();
            this.output.Clear();
        }

        #endregion // Setup/Teardown

        #region Implementation

        #region Assertions

        /// <summary>
        /// Special Assertion method for StyleCop results:
        /// Asserts that the specified rule was not violated.
        /// </summary>
        /// <param name="ruleName">Name of the rule.</param>
        protected void AssertNotViolated(string ruleName)
        {
            if (this.violations.Exists(v => v.Rule.Name == ruleName))
            {
                Assert.Fail(
                        "Rule '{0}' was unexpectedly violated.",
                        ruleName);
            }
        }

        /// <summary>
        /// Special Assertion method for StyleCop results: Asserts that
        /// the specified rule was violated using a custom delegate.
        /// </summary>
        /// <param name="match">
        /// The predicate method which defines the criteria
        /// that a <see cref="Violation"/> must meet.
        /// </param>
        /// <remarks>
        /// The <paramref name="match"/> predicate must return <see
        /// langword="false" /> to cause this method to fail.
        /// </remarks>
        /// <seealso cref="Predicate{T}"/>
        protected void AssertViolated(Predicate<Violation> match)
        {
            if (!this.violations.Exists(match))
            {
                Assert.Fail("Failed to violate a rule with the specified criteria.");
            }
        }

        /// <summary>
        /// Special Assertion method for StyleCop results: Asserts that
        /// the specified rule was not violated using a custom delegate.
        /// </summary>
        /// <param name="match">
        /// The predicate method which defines the criteria
        /// that a <see cref="Violation"/> must not meet.
        /// </param>
        /// <remarks>
        /// The <paramref name="match"/> predicate must return <see
        /// langword="true" /> to cause this method to fail.
        /// </remarks>
        /// <seealso cref="Predicate{T}"/>
        protected void AssertNotViolated(Predicate<Violation> match)
        {
            if (this.violations.Exists(match))
            {
                Assert.Fail("A rule with the specified criteria was violated (but none was expected to be).");
            }
        }

        /// <summary>
        /// Special Assertion method for StyleCop results:
        /// Asserts that the specified rule was violated.
        /// </summary>
        /// <param name="ruleName">Name of the rule.</param>
        protected void AssertViolated(string ruleName)
        {
            if (!this.violations.Exists(v => v.Rule.Name == ruleName))
            {
                Assert.Fail(
                        "Rule '{0}' was not violated (but was expected to be).",
                        ruleName);
            }
        }

        /// <summary>
        /// Special Assertion method for StyleCop results:
        /// Asserts that the specified rule was violated n times.
        /// </summary>
        /// <param name="ruleName">Name of the rule.</param>
        /// <param name="violationCount">The number of violations of this rule.</param>
        protected void AssertViolated(int violationCount, string ruleName)
        {
            int actualCount = this.Violations.FindAll(v => v.Rule.Name == ruleName).Count;
            if (actualCount != violationCount)
            {
                Assert.Fail(
                        "Violated rule '{0}' {1} times, but expected was {2} times.",
                        ruleName,
                        actualCount,
                        violationCount);
            }
        }

        /// <summary>
        /// Special Assertion method for StyleCop results: Asserts that
        /// the specified rule was violated at the specified line numbers.
        /// </summary>
        /// <param name="ruleName">Name of the rule.</param>
        /// <param name="lineNumbers">
        /// The line numbers where the violations are expected.
        /// </param>
        protected void AssertViolated(string ruleName, params int[] lineNumbers)
        {
            if (lineNumbers == null || lineNumbers.Length <= 0)
            {
                return;
            }

            foreach (int lineNumber in lineNumbers)
            {
                if (!this.violations.Exists(v => v.Rule.Name == ruleName &&
                                                 v.Line == lineNumber))
                {
                    Assert.Fail(
                            "Failed to violate rule '{0}' on line #{1}.",
                            ruleName,
                            lineNumber);
                }
            }
        }

        #endregion // Assertions

        /// <summary>
        /// Adds a source file to be analyzed by <i>StyleCop</i>
        /// during the next analysis run.
        /// </summary>
        /// <param name="fileName">
        /// The file (path must be relative to this assemblies'
        /// location (which usually is <c>bin/Debug</c>).
        /// </param>
        /// <exception cref="FileNotFoundException">
        /// The file <paramref name="fileName"/> could not be found.
        /// </exception>
        protected void AddSourceFile(string fileName)
        {
            fileName = Path.GetFullPath(fileName);

            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }

            this.console.Core.Environment.AddSourceCode(this.project, fileName, null);
        }

        /// <summary>
        /// Runs the actual <i>StyleCop</i> analysis.
        /// </summary>
        protected void RunAnalysis()
        {
            var projects = new[] { this.project };
            this.console.Start(projects, true);
        }

        /// <summary>
        /// Adds an addin path for the <i>StyleCop</i> engine.
        /// </summary>
        /// <param name="folder">
        /// The path to the additional addins (rule assemblies).
        /// </param>
        /// <exception cref="DirectoryNotFoundException">
        /// The directory '<paramref name="folder"/>' could not be found.
        /// </exception>
        /// <remarks>
        /// <para>
        /// </para>
        /// The application's <c>bin</c> folder is added automatically, so you
        /// don't have to care about that.
        /// </para><para>
        /// If you need to load the rule assemblies from <i>StyleCop</i>'s installation
        /// path, you should override the <see cref="StyleCopFolder"/> property.
        /// </para>
        /// </remarks>
        protected void AddAddInsPath(string folder)
        {
            if (folder != null)
            {
                if (Directory.Exists(folder))
                {
                    this.addinPaths.Add(folder);
                }
                else
                {
                    throw new DirectoryNotFoundException(
                                    string.Format("Directory not found: {0}.", 
                                                  folder));
                }
            }
        }

        #endregion // Implementation

    } // class StyleCopFixtureBase

} // namespace StyleCopFixtureDemo
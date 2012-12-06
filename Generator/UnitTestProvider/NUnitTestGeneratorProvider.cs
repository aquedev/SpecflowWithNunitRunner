using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TechTalk.SpecFlow.BindingSkeletons;
using TechTalk.SpecFlow.Parser.SyntaxElements;
using TechTalk.SpecFlow.Utils;

namespace TechTalk.SpecFlow.Generator.UnitTestProvider
{

    public class NUnitTestGeneratorProvider : IUnitTestGeneratorProvider
    {
        private readonly IStepTextAnalyzer m_stepTextAnalyzer;
        private const string TESTFIXTURE_ATTR = "NUnit.Framework.TestFixtureAttribute";
        private const string TEST_ATTR = "NUnit.Framework.TestAttribute";
        private const string ROW_ATTR = "NUnit.Framework.TestCaseAttribute";
        private const string CATEGORY_ATTR = "NUnit.Framework.CategoryAttribute";
        private const string TESTSETUP_ATTR = "NUnit.Framework.SetUpAttribute";
        private const string TESTFIXTURESETUP_ATTR = "NUnit.Framework.TestFixtureSetUpAttribute";
        private const string TESTFIXTURETEARDOWN_ATTR = "NUnit.Framework.TestFixtureTearDownAttribute";
        private const string TESTTEARDOWN_ATTR = "NUnit.Framework.TearDownAttribute";
        private const string IGNORE_ATTR = "NUnit.Framework.IgnoreAttribute";
        private const string DESCRIPTION_ATTR = "NUnit.Framework.DescriptionAttribute";


        protected CodeDomHelper CodeDomHelper { get; set; }

        public bool SupportsRowTests { get { return true; } }
        public bool SupportsAsyncTests { get { return false; } }

        public NUnitTestGeneratorProvider(CodeDomHelper codeDomHelper, IStepTextAnalyzer stepTextAnalyzer)
        {
            m_stepTextAnalyzer = stepTextAnalyzer;
            CodeDomHelper = codeDomHelper;
        }

        public void SetTestClass(TestClassGenerationContext generationContext, string featureTitle, string featureDescription)
        {
            CodeDomHelper.AddAttribute(generationContext.TestClass, TESTFIXTURE_ATTR);
            CodeDomHelper.AddAttribute(generationContext.TestClass, DESCRIPTION_ATTR, featureTitle);

        }

        public void SetTestClassCategories(TestClassGenerationContext generationContext, IEnumerable<string> featureCategories)
        {
            CodeDomHelper.AddAttributeForEachValue(generationContext.TestClass, CATEGORY_ATTR, featureCategories);
        }

        public void SetTestClassIgnore(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestClass, IGNORE_ATTR);
        }

        public virtual void FinalizeTestClass(TestClassGenerationContext generationContext)
        {
            // by default, doing nothing to the final generated code
        }


        public void SetTestClassInitializeMethod(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestClassInitializeMethod, TESTFIXTURESETUP_ATTR);
        }

        public void SetTestClassCleanupMethod(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestClassCleanupMethod, TESTFIXTURETEARDOWN_ATTR);
        }


        public void SetTestInitializeMethod(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestInitializeMethod, TESTSETUP_ATTR);
        }

        public void SetTestCleanupMethod(TestClassGenerationContext generationContext)
        {
            CodeDomHelper.AddAttribute(generationContext.TestCleanupMethod, TESTTEARDOWN_ATTR);
        }

        private List<string> m_parametersDeclaredInMethod = new List<string>();

        public void SetTestMethod(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
        {
            m_parametersDeclaredInMethod = new List<string>(); //reset it

            CodeDomHelper.AddAttribute(testMethod, TEST_ATTR);
            CodeDomHelper.AddAttribute(testMethod, DESCRIPTION_ATTR, scenarioTitle);

            var classesDeclared = new List<string>();
            var paramaeters = new List<KeyValuePair<string, System.Type>>();

            foreach (var scenario in generationContext.Feature.Scenarios.Where(x => x.Title == scenarioTitle))
            {

                foreach (ScenarioStep scenarioStep in scenario.Steps)
                {

                    var specsFolder = Directory.GetParent(generationContext.Feature.SourceFile);
                    var result = m_stepTextAnalyzer.Analyze(scenarioStep.Text, new CultureInfo("en-GB"));

                    if (scenarioStep.MultiLineTextArgument != null)
                        result.Parameters.Add(new AnalyzedStepParameter("String", "multilineText"));

                    if (scenarioStep.TableArg != null)
                        result.Parameters.Add(new AnalyzedStepParameter("Table", "table"));
                    var regex = GetSimpleRegex(result);



                    try
                    {
                        var originalFilePath = new FileInfo(generationContext.Feature.SourceFile).Directory.Parent.FullName + @"\bin\debug";
                        var newFolderPath = Path.GetTempPath() + @"\Specflowdlls\" + DateTime.Now.ToLongDateString() + DateTime.Now.Ticks;

                        DirectoryInfo newDir = new DirectoryInfo(newFolderPath);
                        newDir.Create();

                        string[] files = Directory.GetFiles(originalFilePath, "*.dll");

                        foreach (string file in files)
                        {
                            string otherFile = Path.Combine(newFolderPath, Path.GetFileName(file));
                            File.Copy(file, otherFile, true);
                        }

                        var dllname = new FileInfo(generationContext.Feature.SourceFile).Directory.Parent.Name + ".dll";

                        var assembly = Assembly.LoadFrom(String.Concat(newFolderPath, @"\", dllname));

                        var methods = assembly.GetTypes()
                            .SelectMany(t => t.GetMethods())
                            .Where(m => HasSpecflowAttribute(m.GetCustomAttributesData(), regex))
                            .ToArray();


                        foreach (var methodInfo in methods)
                        {
                            //foreach (var param in methodInfo.GetParameters())
                            //{
                            //    paramaeters.Add(new KeyValuePair<string, Type>(param.Name, param.ParameterType));
                            //}
                            classesDeclared = AddStatements(testMethod, methodInfo, classesDeclared, scenarioStep, result,regex);

                        }

                    }
                    catch (Exception exception)
                    {
                        throw new Exception("oops", exception);
                    }
                }
            }

            /*  This is to add missing parameters to the method */

            //foreach (var keyValuePair in paramaeters)
            //{
            //    bool alreadyHasParameter = false;

            //    foreach (var parameter in testMethod.Parameters)
            //    {
            //        if ((string) parameter == keyValuePair.Key)
            //        {
            //            alreadyHasParameter = true;
            //        }
            //    }

            //    if (!alreadyHasParameter)
            //    {
            //        testMethod.Parameters.Add(new CodeParameterDeclarationExpression(keyValuePair.Value,keyValuePair.Key));
            //    }

            //}
        }

        private bool HasSpecflowAttribute(IList<CustomAttributeData> getCustomAttributesData, string regex)
        {
            foreach (var customAttributeData in getCustomAttributesData)
            {
                foreach (var customAttributeTypedArgument in customAttributeData.ConstructorArguments)
                {
                    if (customAttributeTypedArgument.Value is String)
                    {
                        //   Console.WriteLine((string) customAttributeTypedArgument.Value);
                        if ((string)customAttributeTypedArgument.Value == regex)
                        {
                            return true;
                        }
                    }

                }

            }

            return false;
        }

        private List<string> AddStatements(CodeMemberMethod testMethod, MethodInfo methodInfo, List<string> classesDeclared, ScenarioStep step, AnalyzedStepText stepText, string regex)
        {
            if (!HasClassBeenDeclared(classesDeclared, methodInfo.DeclaringType.ToString()))
            {
                CodeVariableDeclarationStatement testRunnerField = new CodeVariableDeclarationStatement(
                    methodInfo.DeclaringType, methodInfo.DeclaringType.Name.ToLower(),
                    new CodeObjectCreateExpression(methodInfo.DeclaringType));
                testMethod.Statements.Add(testRunnerField);
                classesDeclared.Add(methodInfo.DeclaringType.ToString());
            }

            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (parameters.Count() >= 1)
            {
                //declare any variables needed here
                //string value = step.Text
                int i = 0;
                foreach (ParameterInfo parameterInfo in parameters) //these are the parameters needed
                {
                    string parameterValue = stepText.TextParts[i];


                    Match match = Regex.Match(step.Text,regex);
                    if (match.Success)
                    {
                         parameterValue = match.Groups[1].Value;
                    }

                    bool paramAlreadyDeclared = false;

                    //check not in test case
                    foreach (CodeParameterDeclarationExpression parameter in testMethod.Parameters) //these are the ones specified by the test case attributes
                    {
                        if (parameterInfo.Name == parameter.Name)
                        {
                            paramAlreadyDeclared = true;
                        }
                    }

                    //check not already declared in body of method
                    foreach (var preDecalredParam in m_parametersDeclaredInMethod)
                    {
                        if (preDecalredParam == parameterInfo.Name)
                        {
                            paramAlreadyDeclared = true;
                        }
                    }

                    if (!paramAlreadyDeclared)
                    {
                        m_parametersDeclaredInMethod.Add(parameterInfo.Name);//record that param declared
                        CodeVariableDeclarationStatement variable = new CodeVariableDeclarationStatement(typeof(string), parameterInfo.Name, new CodePrimitiveExpression(parameterValue));
                        testMethod.Statements.Add(variable);
                    }

                }



                testMethod.Statements.Add(new CodeMethodInvokeExpression(
                                        new CodeTypeReferenceExpression(
                                            methodInfo.DeclaringType.Name.ToLower()),
                                        methodInfo.Name,
                                        GetParameters(testMethod, methodInfo)));
            }
            else
            {
                testMethod.Statements.Add(new CodeMethodInvokeExpression(
                                              new CodeTypeReferenceExpression(
                                                  methodInfo.DeclaringType.Name.ToLower()),
                                              methodInfo.Name));
            }
            return classesDeclared;
        }

        private static bool HasClassBeenDeclared(List<string> classesDeclared, string typeBeingUsed)
        {
            foreach (var type in classesDeclared)
            {
                if (type == typeBeingUsed)
                {
                    return true;
                }
            }

            return false;
        }

        private static CodeExpression[] GetParameters(CodeMemberMethod testMethod, MethodInfo methodInfo)
        {
            var list = new CodeExpression[methodInfo.GetParameters().Count()];
            int i = 0;
            foreach (var parameter in methodInfo.GetParameters())
            {
                if (parameter.ParameterType == typeof(Int32))
                {
                    list[i] = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(
                            "System.Convert"),
                        "ToInt32", new CodeVariableReferenceExpression(parameter.Name));
                }
                else if (parameter.ParameterType == typeof(DateTime))
                {
                    list[i] = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(
                                                                 "System.DateTime"),
                                                             "Parse",
                                                             new CodeVariableReferenceExpression(parameter.Name));
                }
                else
                {

                    list[i] = new CodeVariableReferenceExpression(parameter.Name);
                }
                i++;
            }

            return list;
        }

        private string GetRegex(AnalyzedStepText stepText)
        {
            StringBuilder result = new StringBuilder();

            result.Append(EscapeRegex(stepText.TextParts[0]));
            for (int i = 1; i < stepText.TextParts.Count; i++)
            {
                result.AppendFormat("({0})", stepText.Parameters[i - 1].RegexPattern);
                result.Append(EscapeRegex(stepText.TextParts[i]));
            }

            return result.ToString();
        }

        private string GetSimpleRegex(AnalyzedStepText stepText)
        {
            StringBuilder result = new StringBuilder();

            result.Append(stepText.TextParts[0]);
            for (int i = 1; i < stepText.TextParts.Count; i++)
            {
                result.AppendFormat("({0})", stepText.Parameters[i - 1].RegexPattern);
                result.Append(stepText.TextParts[i]);
            }

            return result.ToString();
        }

        protected static string EscapeRegex(string text)
        {
            return Regex.Escape(text).Replace("\"", "\"\"").Replace("\\ ", " ");
        }

        public void SetTestMethodCategories(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> scenarioCategories)
        {
            CodeDomHelper.AddAttributeForEachValue(testMethod, CATEGORY_ATTR, scenarioCategories);
        }

        public void SetTestMethodIgnore(TestClassGenerationContext generationContext, CodeMemberMethod testMethod)
        {
            CodeDomHelper.AddAttribute(testMethod, IGNORE_ATTR);
        }


        public void SetRowTest(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle)
        {
            SetTestMethod(generationContext, testMethod, scenarioTitle);
        }

        public void SetRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> arguments, IEnumerable<string> tags, bool isIgnored)
        {
            var args = arguments.Select(
              arg => new CodeAttributeArgument(new CodePrimitiveExpression(arg))).ToList();

            // addressing ReSharper bug: TestCase attribute with empty string[] param causes inconclusive result - https://github.com/techtalk/SpecFlow/issues/116
            var exampleTagExpressionList = tags.Select(t => new CodePrimitiveExpression(t)).ToArray();
            CodeExpression exampleTagsExpression = exampleTagExpressionList.Length == 0 ?
                (CodeExpression)new CodePrimitiveExpression(null) :
                new CodeArrayCreateExpression(typeof(string[]), exampleTagExpressionList);
            args.Add(new CodeAttributeArgument(exampleTagsExpression));

            if (isIgnored)
                args.Add(new CodeAttributeArgument("Ignored", new CodePrimitiveExpression(true)));

            CodeDomHelper.AddAttribute(testMethod, ROW_ATTR, args.ToArray());
        }

        public void SetTestMethodAsRow(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, string scenarioTitle, string exampleSetName, string variantName, IEnumerable<KeyValuePair<string, string>> arguments)
        {
            // doing nothing since we support RowTest
        }
    }
}
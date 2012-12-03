﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.7.1.0
//      SpecFlow Generator Version:1.7.0.0
//      Runtime Version:4.0.30319.544
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
namespace TechTalk.SpecFlow.Specs.Features
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.7.1.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Basic scenario execution")]
    public partial class BasicScenarioExecutionFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "BasicExecution.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Basic scenario execution", "", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
            this.FeatureBackground();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 3
#line hidden
#line 4
 testRunner.Given("there is a feature file in the project as", "\tFeature: Simple Feature\n\tScenario: Simple Scenario\n\t\tGiven there is something\n\t\t" +
                    "When I do something\n\t\tThen something should happen", ((TechTalk.SpecFlow.Table)(null)));
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Should be able to execute a simple passing scenario")]
        public virtual void ShouldBeAbleToExecuteASimplePassingScenario()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Should be able to execute a simple passing scenario", ((string[])(null)));
#line 13
this.ScenarioSetup(scenarioInfo);
#line 14
 testRunner.Given("all steps are bound and pass");
#line 15
 testRunner.When("I execute the tests");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Total",
                        "Succeeded"});
            table1.AddRow(new string[] {
                        "1",
                        "1"});
#line 16
 testRunner.Then("the execution summary should contain", ((string)(null)), table1);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Should be able to execute a simple failing scenario")]
        public virtual void ShouldBeAbleToExecuteASimpleFailingScenario()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Should be able to execute a simple failing scenario", ((string[])(null)));
#line 21
this.ScenarioSetup(scenarioInfo);
#line 22
 testRunner.Given("all steps are bound and fail");
#line 23
 testRunner.When("I execute the tests");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Total",
                        "Failed"});
            table2.AddRow(new string[] {
                        "1",
                        "1"});
#line 24
 testRunner.Then("the execution summary should contain", ((string)(null)), table2);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Should be able to execute a simple pending scenario")]
        public virtual void ShouldBeAbleToExecuteASimplePendingScenario()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Should be able to execute a simple pending scenario", ((string[])(null)));
#line 28
this.ScenarioSetup(scenarioInfo);
#line 29
 testRunner.When("I execute the tests");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Total",
                        "Pending"});
            table3.AddRow(new string[] {
                        "1",
                        "1"});
#line 30
 testRunner.Then("the execution summary should contain", ((string)(null)), table3);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Should be aaa")]
        public virtual void ShouldBeAaa()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Should be aaa", ((string[])(null)));
#line 35
this.ScenarioSetup(scenarioInfo);
#line 36
 testRunner.Given("all steps are bound and passaaaa");
#line 37
 testRunner.When("I execute the tests");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Total",
                        "Succeeded"});
            table4.AddRow(new string[] {
                        "1",
                        "1"});
#line 38
 testRunner.Then("the execution summary should contain", ((string)(null)), table4);
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#endregion

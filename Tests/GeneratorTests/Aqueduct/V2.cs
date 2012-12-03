using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using NUnit.Framework;

namespace TechTalk.SpecFlow.GeneratorTests.Aqueduct
{
    [Serializable]
     public class AssemblyLoaderNew : MarshalByRefObject
    {
       private string ApplicationBase { get; set; }
      
         public AssemblyLoaderNew()
         {
           ApplicationBase = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
       }
    
       public Assembly Resolve(object sender, ResolveEventArgs args)
      {
           AssemblyName assemblyName = new AssemblyName(args.Name);
            string fileName = string.Format("{0}.dll", assemblyName.Name);
            if (fileName.Contains("TechTalk.SpecFlow.GeneratorTests"))
            {
                return  Assembly.LoadFile(Path.Combine(ApplicationBase, fileName));
            }
           return Assembly.LoadFile(Path.Combine(@"C:\\Projects\\oa-public\src\\OrbisAccess.PublicSite.Specs\\bin\\debug\\", fileName));
        }
    }

    [TestFixture]
    public class  Testing
    {
        [Test]
        public void Test()
        {
            string applicationBase = "C:/Projects/oa-public/src/OrbisAccess.PublicSite.Specs/bin/debug";
            AppDomainSetup setup = new AppDomainSetup
            {
                ApplicationName = "Lee",
                 ApplicationBase = applicationBase,
                PrivateBinPath = AppDomain.CurrentDomain.BaseDirectory,
                 PrivateBinPathProbe = AppDomain.CurrentDomain.BaseDirectory,
                 ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
             };
     
            Evidence evidence = new Evidence(AppDomain.CurrentDomain.Evidence);
            AppDomain domain = AppDomain.CreateDomain("Lee", evidence, setup);
             var loader =  domain.CreateInstanceFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TechTalk.SpecFlow.GeneratorTests.dll"), "TechTalk.SpecFlow.GeneratorTests.Aqueduct.AssemblyLoaderNew");

            var loadner = new AssemblyLoaderNew();
            domain.AssemblyResolve += new AssemblyLoaderNew().Resolve;
            //C:\\Projects\\oa-public\src\\OrbisAccess.PublicSite.Specs\\bin\\debug\\
            var assembly = domain.Load("OrbisAccess.PublicSite.Specs");

            Assert.IsNotNull(assembly);
        }
    }
}

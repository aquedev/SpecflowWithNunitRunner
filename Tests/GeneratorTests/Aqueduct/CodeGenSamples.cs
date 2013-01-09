using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using NUnit.Framework;

namespace TechTalk.SpecFlow.GeneratorTests.Aqueduct
{

    [Serializable]
    public class AssemblyLoader : MarshalByRefObject
    {
        private readonly Dictionary<string, DirectoryInfo> probes = new Dictionary<string, DirectoryInfo>();

        public AssemblyLoader()
        {
            AddProbePath(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
            AddProbePath(AppDomain.CurrentDomain.SetupInformation.PrivateBinPath);

            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        }

        public void AddProbePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                path = Directory.GetCurrentDirectory();

            DirectoryInfo dir = new DirectoryInfo(path);
            if (probes.ContainsKey(dir.FullName))
                return;

            probes.Add(dir.FullName, dir);
        }

        private Assembly Resolve(object sender, ResolveEventArgs args)
        {
            AssemblyName assemblyName = new AssemblyName(args.Name);
            foreach (DirectoryInfo probePath in probes.Values)
            {
                Assembly assembly = LoadAssemblyFromPath(probePath, assemblyName.Name);
                if (assembly != null)
                {
                    return assembly;
                }
            }
            return null;
        }

        private static Assembly LoadAssemblyFromPath(DirectoryInfo probePath, string assemblyName)
        {
            string dllFileName = Path.Combine(probePath.FullName, string.Format("{0}.dll", assemblyName));
            if (File.Exists(dllFileName))
                return Assembly.LoadFile(dllFileName);

            string exeFileName = Path.Combine(probePath.FullName, string.Format("{0}.exe", assemblyName));
            if (File.Exists(exeFileName))
                return Assembly.LoadFile(exeFileName);

            return null;
        }
    }
 

    [TestFixture]
   [Serializable]
    public class CodeGenSamples
    {
        private AppDomain domain;

        [Test]
        public void LoadDllViaNewAppDomain()
        {
            AppDomainSetup domainInfo = new AppDomainSetup();

            domainInfo.ApplicationBase = @"file:///c:/Projects/oa-public/src/OrbisAccess.PublicSite.Specs/bin";
            domainInfo.PrivateBinPath = "c:/Projects/oa-public/src/OrbisAccess.PublicSite.Specs/bin";
            domainInfo.ShadowCopyFiles = true.ToString();
            domainInfo.ShadowCopyDirectories = "Debug";
            domainInfo.PrivateBinPathProbe = "c:/Projects/oa-public/src/OrbisAccess.PublicSite.Specs/bin/Debug";
          //  domainInfo.ApplicationBase =  @"file:///" + System.Environment.SystemDirectory;

            Evidence evidence = AppDomain.CurrentDomain.Evidence;

             domain = AppDomain.CreateDomain("GetRefs", evidence, domainInfo);
            domain.AssemblyResolve +=new ResolveEventHandler(domain_AssemblyResolve);
            Console.WriteLine(domainInfo.ApplicationBase);

            var bytes = File.ReadAllBytes(@"C:\\Projects\\oa-public\src\\OrbisAccess.PublicSite.Specs\\bin\\debug\\OrbisAccess.PublicSite.Specs.dll");
            var assembly = domain.Load("OrbisAccess.PublicSite.Specs");

            Assert.IsNotNull(assembly);
        }

        private Assembly domain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return domain.Load(args.Name);
        }

        [Test]
        public void TestCustomDomainLoader()
        {
            
        }

        

        [Test]
        public void GetMethods()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);
           var assembly =  System.Reflection.Assembly.ReflectionOnlyLoadFrom(
                @"C:\\Projects\\oa-public\src\\OrbisAccess.PublicSite.Specs\\bin\\debug\\OrbisAccess.PublicSite.Specs.dll");

           var methods = assembly.GetTypes()
                            .SelectMany(t => t.GetMethods())
                            .Where(m => HasThenAttribute(m.GetCustomAttributesData()))
                            .ToArray();

            Assert.IsTrue(methods.Count() > 0);

            foreach (var methodInfo in methods)
            {
                Console.WriteLine(methodInfo.DeclaringType.Name);
                Assert.IsNotNull(methodInfo.DeclaringType);
            }
        }

        private bool HasThenAttribute(IList<CustomAttributeData> getCustomAttributesData)
        {
            foreach (var customAttributeData in getCustomAttributesData)
            {
                foreach (var customAttributeTypedArgument in customAttributeData.ConstructorArguments)
                {
                    if (customAttributeTypedArgument.Value is String)
                    {
                     //   Console.WriteLine((string) customAttributeTypedArgument.Value);
                        if ((string) customAttributeTypedArgument.Value == "I have navigated to Orbis Access home page")
                        {
                            return true;
                        }
                    }

                }
               
            }

            return false;
        }

        Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return System.Reflection.Assembly.ReflectionOnlyLoad(args.Name);
        }
      
        [Test]
        public void CallAMethod()
        {
            
          // // var lee = AppDomain.CreateDomain("lee");
            //  var bytes = File.ReadAllBytes( @"C:\\Projects\\oa-public\src\\OrbisAccess.PublicSite.Specs\\bin\\debug\\Castle.Core.dll");

            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = @"C:\Projects\oa-public\src\OrbisAccess.PublicSite.Specs";
            setup.ApplicationName = "Isolated Domain";
            setup.PrivateBinPath = @"C:\Projects\oa-public\src\OrbisAccess.PublicSite.Specs\bin\debug";
            // setup.PrivateBinPathProbe = "";//disable search in AppBase..

            var domain = AppDomain.CreateDomain(Guid.NewGuid().ToString(),
                                               AppDomain.CurrentDomain.Evidence,
                                               setup,
                                               AppDomain.CurrentDomain.PermissionSet);
            domain.AssemblyResolve += new ResolveEventHandler(MyResolveEventHandler);

            var reflectedAssembly = Assembly.ReflectionOnlyLoadFrom(@"C:\\Projects\\oa-public\src\\OrbisAccess.PublicSite.Specs\\bin\\debug\\OrbisAccess.PublicSite.Specs.dll");

            AssemblyName[] arrReferencedAssmbNames = reflectedAssembly.GetReferencedAssemblies();

            //Loop through the array of referenced assembly names.
            foreach (AssemblyName strAssmbName in arrReferencedAssmbNames)
            {
                try
                {
                    //Check for the assembly names that have raised the "AssemblyResolve" event.

                    //Build the path of the assembly from where it has to be loaded.				
                    var strTempAssmbPath = @"C:\\Projects\\oa-public\src\\OrbisAccess.PublicSite.Specs\\bin\\debug\\" +
                                           strAssmbName.Name + ".dll";
                    if (File.Exists(strTempAssmbPath))
                    {
                        var temp = domain.Load(AssemblyName.GetAssemblyName(strTempAssmbPath).Name);
                    }
                } catch(Exception)
                {
                   
                }
            }

            
          


            var assembly = domain.Load(AssemblyName.GetAssemblyName(@"C:\\Projects\\oa-public\src\\OrbisAccess.PublicSite.Specs\\bin\\debug\\OrbisAccess.PublicSite.Specs.dll").Name);


            //ProxyDomain pd = new ProxyDomain();
            //Assembly assembly = pd.GetAssembly(@"C:\\Projects\\oa-public\src\\OrbisAccess.PublicSite.Specs\\bin\\debug\\Castle.Core.dll");
        
        }


        private Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            //This handler is called only when the common language runtime tries to bind to the assembly and fails.

            //Retrieve the list of referenced assemblies in an array of AssemblyName.
            Assembly MyAssembly, objExecutingAssemblies;
            string strTempAssmbPath = "";

            objExecutingAssemblies = Assembly.ReflectionOnlyLoadFrom(String.Format(@"C:\\Projects\\oa-public\src\\OrbisAccess.PublicSite.Specs\\bin\\debug\\{0}.dll", args.Name));

            AssemblyName[] arrReferencedAssmbNames = objExecutingAssemblies.GetReferencedAssemblies();

            //Loop through the array of referenced assembly names.
            foreach (AssemblyName strAssmbName in arrReferencedAssmbNames)
            {
                //Check for the assembly names that have raised the "AssemblyResolve" event.
                if (strAssmbName.FullName.Substring(0, strAssmbName.FullName.IndexOf(",")) == args.Name.Substring(0, args.Name.IndexOf(",")))
                {
                    //Build the path of the assembly from where it has to be loaded.				
                    strTempAssmbPath = @"C:\\Projects\\oa-public\src\\OrbisAccess.PublicSite.Specs\\bin\\debug\\" + args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll";
                    break;
                }

            }
            //Load the assembly from the specified path. 					
            MyAssembly = Assembly.LoadFrom(strTempAssmbPath);

            //Return the loaded assembly.
            return MyAssembly;
        }
    }

    class ProxyDomain : MarshalByRefObject
    {
        public Assembly GetAssembly(string AssemblyPath)
        {
            try
            {
                AppDomainSetup setup = new AppDomainSetup();
                  setup.ApplicationBase = @"C:\Projects\oa-public\src\OrbisAccess.PublicSite.Specs";
                setup.ApplicationName = "Isolated Domain";
                setup.PrivateBinPath = @"C:\Projects\oa-public\src\OrbisAccess.PublicSite.Specs\bin\debug";
                // setup.PrivateBinPathProbe = "";//disable search in AppBase..
                var domain = AppDomain.CreateDomain(Guid.NewGuid().ToString(),
                                                    AppDomain.CurrentDomain.Evidence,
                                                    setup,
                                                    AppDomain.CurrentDomain.PermissionSet);
                domain.AssemblyResolve += new ResolveEventHandler(MyResolveEventHandler);
                //The following statement in theory should pick B.dll's dependency from DirA.
               return domain.Load(AssemblyName.GetAssemblyName(@"C:\\Projects\\oa-public\src\\OrbisAccess.PublicSite.Specs\\bin\\debug\\Castle.Core.dll").Name);

              //  return Assembly.LoadFrom(AssemblyPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        private Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            //This handler is called only when the common language runtime tries to bind to the assembly and fails.

            //Retrieve the list of referenced assemblies in an array of AssemblyName.
            Assembly MyAssembly, objExecutingAssemblies;
            string strTempAssmbPath = "";

            objExecutingAssemblies = Assembly.GetExecutingAssembly();
            AssemblyName[] arrReferencedAssmbNames = objExecutingAssemblies.GetReferencedAssemblies();

            //Loop through the array of referenced assembly names.
            foreach (AssemblyName strAssmbName in arrReferencedAssmbNames)
            {
                //Check for the assembly names that have raised the "AssemblyResolve" event.
                if (strAssmbName.FullName.Substring(0, strAssmbName.FullName.IndexOf(",")) == args.Name.Substring(0, args.Name.IndexOf(",")))
                {
                    //Build the path of the assembly from where it has to be loaded.				
                    strTempAssmbPath = @"C:\\Projects\\oa-public\src\\OrbisAccess.PublicSite.Specs\\bin\\debug\\" + args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll";
                    break;
                }

            }
            //Load the assembly from the specified path. 					
            MyAssembly = Assembly.LoadFrom(strTempAssmbPath);

            //Return the loaded assembly.
            return MyAssembly;
        }
    }
}

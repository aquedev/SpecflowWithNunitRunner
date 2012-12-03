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
    [TestFixture]
    public class V3
    {
        [Test]
        public void Test()
        {
            var originalFilePath = @"C:\Projects\oa-public\src\OrbisAccess.PublicSite.Specs\bin\debug\";
            var newFolderPath = @"C:\testtemp";

            DirectoryInfo newDir = new DirectoryInfo(newFolderPath);
            if (newDir.Exists)
            {
                Console.WriteLine("The folder already exists");
                foreach (var source in newDir.GetFiles().Where( x => x.Exists))
                {
                    source.Delete();
                }
            }
            else
                newDir.Create();

            string[] files = Directory.GetFiles(originalFilePath, "*.dll");

            foreach (string file in files)
            {
                string otherFile = Path.Combine(newFolderPath, Path.GetFileName(file));
                File.Copy(file, otherFile);
            }

          
            var assembly =
                Assembly.LoadFrom(
                    @"C:\Projects\oa-public\src\OrbisAccess.PublicSite.Specs\bin\debug\OrbisAccess.PublicSite.Specs.dll");

            Assert.IsNotNull(assembly);
        }

        [Test]
        public void GenerateFolderPath()
        {
            var newFolderPath = System.IO.Path.GetTempPath() + @"\OrbisTests\" + DateTime.Now.ToLongDateString() + DateTime.Now.Ticks;
            Assert.IsNotNullOrEmpty(newFolderPath);
        }

        [Test]
        public void FolderPath()
        {
            var originalFilePath = new FileInfo(@"C:\Projects\oa-public\src\OrbisAccess.PublicSite.Specs\Features\HOM1.feature").Directory.Parent.FullName + @"\bin\debug";
            Assert.IsTrue(@"C:\Projects\oa-public\src\OrbisAccess.PublicSite.Specs\bin\debug" == originalFilePath);
        }

        [Test]
        public void GetDllName()
        {
            var dllname = new FileInfo(@"C:\Projects\oa-public\src\OrbisAccess.PublicSite.Specs\Features\HOM1.feature").Directory.Parent.Name + ".dll";
            Assert.IsTrue(@"OrbisAccess.PublicSite.Specs.dll" == dllname);
        }
    }
}

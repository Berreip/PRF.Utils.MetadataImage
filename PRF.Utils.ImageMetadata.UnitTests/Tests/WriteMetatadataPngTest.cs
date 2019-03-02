using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PRF.Utils.CoreComponents.Extensions;
using PRF.Utils.ImageMetadata.Helpers;
using PRF.Utils.ImageMetadata.Managers;
using PRF.Utils.ImageMetadata.UnitTests.Contents;

namespace PRF.Utils.ImageMetadata.UnitTests.Tests
{
    [TestClass]
    public class WriteMetatadataPngTest
    {
        /// <summary>
        ///  On récupère l'image de façon dégueulasse car on veut explicitement le fichier posé qq part et non un flux (et puis c'est des TU)
        /// </summary>
        /// <returns></returns>
        private static FileInfo GetTargetImage()
        {
            var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().Location);
            if (assemblyFile.Directory == null) return null;

            try
            {
                return new FileInfo(Path.Combine(assemblyFile.Directory.FullName, "Contents", "img.png"));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// test que l'image cible est présente
        /// </summary>
        [TestMethod]
        public void CheckContentPresence()
        {
            //Configuration
            //Test
            var targetImage = GetTargetImage();

            //Verify
            Assert.IsNotNull(targetImage);
            Assert.IsTrue(targetImage.Exists);
        }

        
        [TestMethod]
        public void ReadMetadataV1()
        {
            //Configuration
            var targetImage = GetTargetImage();
            var copyImg = Path.Combine(targetImage.Directory.FullName, $@"tempImage_{Guid.NewGuid()}.png");
            var fileCopy = new FileInfo(copyImg);

            try
            {
                File.Copy(targetImage.FullName, copyImg);

                //Test
                var result = fileCopy.ToMetadataContainer<UnitTestMetadata>();

                //Verify
                Assert.AreEqual(0, result.Count);
            }
            finally
            {
                fileCopy.DeleteIfExist();
            }
        }

        [TestMethod]
        public void WriteMetadataV1()
        {
            //Configuration
            var targetImage = GetTargetImage();
            var copyImg = Path.Combine(targetImage.Directory.FullName, $@"tempImage_{Guid.NewGuid()}.png");
            var fileCopy = new FileInfo(copyImg);
            const string text = "valeur à écrire";

            try
            {
                File.Copy(targetImage.FullName, copyImg);

                //Test
                var container = fileCopy.ToMetadataContainer<UnitTestMetadata>();
                container.Add(UnitTestMetadata.Metadata4, text);
                container.UpdateFile(fileCopy);
                
                // relis le fichier et vérifie que les données sont présente
                var result = fileCopy.ToMetadataContainer<UnitTestMetadata>();

                //Verify
                Assert.AreEqual(1, result.Count);
                Assert.IsTrue(result.TryGetValue(UnitTestMetadata.Metadata4, out var val));
                Assert.AreEqual(text, val);
            }
            finally
            {
                fileCopy.DeleteIfExist();
            }
        }

        [TestMethod]
        public void WriteMetadataV2()
        {
            //Configuration
            var targetImage = GetTargetImage();
            var copyImg = Path.Combine(targetImage.Directory.FullName, $@"tempImage_{Guid.NewGuid()}.png");
            var fileCopy = new FileInfo(copyImg);
            const string text = "valeur à écrire";

            try
            {
                //Test
                // créer un container vide:
                var container = new MetadataContainer<UnitTestMetadata>
                {
                    {UnitTestMetadata.Metadata4, text}
                };
                container.SaveNewFile(fileCopy, new Bitmap(10, 10));

                // relis le fichier et vérifie que les données sont présente
                var result = fileCopy.ToMetadataContainer<UnitTestMetadata>();

                //Verify
                Assert.AreEqual(1, result.Count);
                Assert.IsTrue(result.TryGetValue(UnitTestMetadata.Metadata4, out var val));
                Assert.AreEqual(text, val);
            }
            finally
            {
                fileCopy.DeleteIfExist();
            }
        }


        /// <summary>
        ///  cas avec du Json dans la valeur
        /// </summary>
        [TestMethod]
        public void WriteMetadataV3()
        {
            //Configuration
            var targetImage = GetTargetImage();
            var copyImg = Path.Combine(targetImage.Directory.FullName, $@"tempImage_{Guid.NewGuid()}.png");
            var fileCopy = new FileInfo(copyImg);
            const string text = @"{""Id"":75,""Name"":""Robert""}";

            try
            {
                File.Copy(targetImage.FullName, copyImg);

                //Test
                var container = fileCopy.ToMetadataContainer<UnitTestMetadata>();
                container.Add(UnitTestMetadata.Metadata4, text);
                container.UpdateFile(fileCopy);

                // relis le fichier et vérifie que les données sont présente
                var result = fileCopy.ToMetadataContainer<UnitTestMetadata>();

                //Verify
                Assert.AreEqual(1, result.Count);
                Assert.IsTrue(result.TryGetValue(UnitTestMetadata.Metadata4, out var val));
                Assert.AreEqual(text, val);
            }
            finally
            {
                fileCopy.DeleteIfExist();
            }
        }
    }
}

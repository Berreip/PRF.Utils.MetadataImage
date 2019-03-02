using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
        /// <param name="imgName"></param>
        /// <returns></returns>
        private static FileInfo GetTargetImage(string imgName)
        {
            var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().Location);
            if (assemblyFile.Directory == null) return null;

            try
            {
                return new FileInfo(Path.Combine(assemblyFile.Directory.FullName, "Contents", imgName));
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
            var targetImage = GetTargetImage("img.png");

            //Verify
            Assert.IsNotNull(targetImage);
            Assert.IsTrue(targetImage.Exists);
        }


        [TestMethod]
        public void ReadMetadataV1()
        {
            //Configuration
            var targetImage = GetTargetImage("img.png");
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
            var targetImage = GetTargetImage("img.png");
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
            var targetImage = GetTargetImage("img.png");
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
            var targetImage = GetTargetImage("img.png");
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

        /// <summary>
        ///  cas avec du Json dans la valeur et plusieurs clés
        /// </summary>
        [TestMethod]
        public void WriteMetadataV4()
        {
            //Configuration
            var targetImage = GetTargetImage("img.png");
            var copyImg = Path.Combine(targetImage.Directory.FullName, $@"tempImage_{Guid.NewGuid()}.png");
            var fileCopy = new FileInfo(copyImg);
            var dicoVerif = new Dictionary<UnitTestMetadata, string>
            {
                {UnitTestMetadata.Metadata1 , @"{""Id"":75,""Name"":""Robert1""}"},
                {UnitTestMetadata.Metadata2 , @"{""Id"":75,""Name"":""Robert2""}"},
                {UnitTestMetadata.Metadata3 , @"{""Id"":75,""Name"":""Robert3""}"},
                {UnitTestMetadata.Metadata4 , @"{""Id"":75,""Name"":""Robert4""}"},
                {UnitTestMetadata.Metadata5 , @"{""Id"":75,""Name"":""Robert5""}"}
            };

            try
            {
                File.Copy(targetImage.FullName, copyImg);

                //Test
                var container = fileCopy.ToMetadataContainer<UnitTestMetadata>();
                foreach (var keyValue in dicoVerif)
                {
                    container.Add(keyValue.Key, keyValue.Value);
                }
                container.UpdateFile(fileCopy);

                // relis le fichier et vérifie que les données sont présente
                var result = fileCopy.ToMetadataContainer<UnitTestMetadata>();

                //Verify
                Assert.AreEqual(dicoVerif.Count, result.Count);
                foreach (var keyValue in dicoVerif)
                {
                    Assert.IsTrue(result.TryGetValue(keyValue.Key, out var val));
                    Assert.AreEqual(keyValue.Value, val);
                }
            }
            finally
            {
                fileCopy.DeleteIfExist();
            }
        }

        /// <summary>
        ///  cas lecture asynchrone
        /// </summary>
        [TestMethod]
        public async Task WriteMetadataV5()
        {
            //Configuration
            var targetImage = GetTargetImage("tempImage_avec_data.png");
            var copyImg = Path.Combine(targetImage.Directory.FullName, $@"tempImage_{Guid.NewGuid()}.png");
            var fileCopy = new FileInfo(copyImg);

            try
            {
                File.Copy(targetImage.FullName, copyImg);

                //Test
                using (var cts = new CancellationTokenSource())
                {
                    var container = await fileCopy.ToMetadataContainerAsync<UnitTestMetadata>(cts.Token).ConfigureAwait(false);

                    //Verify
                    Assert.AreEqual(1, container.Count);
                    Assert.IsTrue(container.TryGetValue(UnitTestMetadata.Metadata4, out var val));
                    Assert.AreEqual(@"{""Id"":75,""Name"":""Robert""}", val);
                }
            }
            finally
            {
                fileCopy.DeleteIfExist();
            }
        }
    }
}

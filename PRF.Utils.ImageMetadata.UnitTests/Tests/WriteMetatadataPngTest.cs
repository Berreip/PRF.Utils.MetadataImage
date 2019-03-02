using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PRF.Utils.CoreComponents.Extensions;
using PRF.Utils.ImageMetadata.PNG;

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
                var result = fileCopy.GetMetadata();

                //Verify
                Assert.AreEqual(0, result.Count);
            }
            finally
            {
                fileCopy.DeleteIfExist();
            }
           

        }

    }
}

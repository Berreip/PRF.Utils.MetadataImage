using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using PRF.Utils.ImageMetadata.Configuration;
using PRF.Utils.ImageMetadata.Helpers;

namespace PRF.Utils.ImageMetadata.PNG
{
    /// <summary>
    /// Ecrit les métadonnées des images
    /// </summary>
    public static class PngMetadataWriter
    {
        /// <summary>
        /// Enregistre une image PNG avec les métadonnées d'entrée sur le disque dur.
        /// </summary>
        /// <param name="bmpEntree">Bitmap d'entrée</param>
        /// <param name="file">le chemin de l'image à écrire </param>
        /// <param name="metadatas">métadonnées à stocker</param>
        public static void SaveImageWithMetadata(this Bitmap bmpEntree, FileInfo file, RawMetadata[] metadatas)
        {
            if (!Directory.Exists(file.DirectoryName))
            {
                if (file.DirectoryName == null)
                    throw new InvalidOperationException($"SaveImageWithMetadata le dossier parent du fichier {file.FullName} était null");

                Directory.CreateDirectory(file.DirectoryName);
            }
            SaveMetadata(bmpEntree, metadatas, file.FullName);
            // refresh du fichier pour les .Exist (qui ne sont pas mis à jour sinon)
            file.Refresh();
        }


        /// <summary>
        /// Enregistre une image PNG avec les métadonnées d'entrée sur le disque dur.
        /// </summary>
        /// <param name="bmpEntree">Bitmap d'entrée</param>
        /// <param name="metadatas">Querys à stocker</param>
        /// <param name="fileFullName">le chemin de l'image à écrire </param>
        public static void SaveImageWithMetadata(this Bitmap bmpEntree, string fileFullName, RawMetadata[] metadatas)
        {
            bmpEntree.SaveImageWithMetadata(new FileInfo(fileFullName), metadatas );
        }

        /// <summary>
        /// Met à jour une image PNG avec les métadonnées d'entrée sur le disque dur.
        /// </summary>
        /// <param name="fileToUpdate">fichier à mettre à jour</param>
        /// <param name="querys">métadonnées à stocker</param>
        public static void UpdateImageWithMetadata(FileInfo fileToUpdate, RawMetadata[] querys)
        {
            // génère un nom de fichier temporaire pour faire l'update
            var fileRenamed = Path.Combine(fileToUpdate.DirectoryName ?? throw new InvalidOperationException(), Guid.NewGuid().ToString());
            using (var bitmap = new Bitmap(fileToUpdate.FullName))
            {
                SaveMetadata(bitmap, querys, fileRenamed);
            }

            File.Copy(fileRenamed, fileToUpdate.FullName, true);
            File.Delete(fileRenamed);
        }

        private static void SaveMetadata(Bitmap bmpEntree, RawMetadata[] querys, string outputFile)
        {
            // FileMode.CreateNew = le fichier ne DOIT pas exister sinon erreur.
            using (var fs = new FileStream(outputFile, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            {
                var meta = new BitmapMetadata(@"png");
                foreach (var pair in querys)
                {
                    meta.SetQuery(pair.Key, pair.Value);
                }

                var encoder = new PngBitmapEncoder();
                var source = bmpEntree.ToBitmapSource();
                encoder.Frames.Add(BitmapFrame.Create(source, source, meta, null));
                encoder.Save(fs);
            }
        }
    }
}

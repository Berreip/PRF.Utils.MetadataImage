using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using PRF.Utils.CoreComponents.JSON;
using PRF.Utils.ImageMetadata.Configuration;
using PRF.Utils.ImageMetadata.Helpers;

namespace PRF.Utils.ImageMetadata.PNG
{
    internal static class PngMetadataReader
    {
        private const int OFFSET = 0; // par du début du fichier = offset inutile mais plus clair que mettre '0' en dur
        private const int BUFFER_SIZE = 4096; // on commence par lire les premiers bytes du fichier (2048 en général, ça suffit dans notre cas mais ne suffit souvent pas)

        /// <summary>
        /// Récupère en asynchrone les métadonnées d'une image
        /// </summary>
        /// <param name="imagePath">le chemin du fichier</param>
        /// <param name="ctsToken">le token d'annulation</param>
        public static async Task<List<MetadataKeyValue>> GetMetadataAsync(string imagePath, CancellationToken ctsToken)
        {
            try
            {
                try
                {
                    return await ExtractHeaderMetadataAsync(imagePath, BUFFER_SIZE, ctsToken).ConfigureAwait(false);
                }
                catch (FileFormatException)
                {
                    // si l'on arrive pas à lire le format de l'image, on retente avec un buffer plus grand (mais une seule fois)
                    return await ExtractHeaderMetadataAsync(imagePath, BUFFER_SIZE * 2, ctsToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // relance une interruption pour la gérer plus loin (permet d'interrompre un chargement plus précocément)
                throw;
            }
            catch (Exception e)
            {
                // relance une exception avec un message plus spécifique et l'excetion source en innerException
                throw new Exception($"Erreur lors de l'extraction des métadonnées de l'image: {imagePath}. {Environment.NewLine} => Exception: {e}", e);
            }
        }
        
        public static List<MetadataKeyValue> GetMetadata(this FileInfo image)
        {
            try
            {
                try
                {
                    return ExtractHeaderMetadata(image.FullName, BUFFER_SIZE);
                }
                catch (FileFormatException)
                {
                    // si l'on arrive pas à lire le format de l'image, on retente avec un buffer plus grand (mais une seule fois)
                    return ExtractHeaderMetadata(image.FullName, BUFFER_SIZE * 2);
                }
            }
            catch (Exception e)
            {
                // relance une exception avec un message plus spécifique et l'excetion source en innerException
                throw new Exception($"Erreur lors de l'extraction des métadonnées de l'image: {image.FullName}. {Environment.NewLine} => Exception: {e}", e);
            }
        }

        private static async Task<List<MetadataKeyValue>> ExtractHeaderMetadataAsync(string imagePath, int bufferSize, CancellationToken ctsToken)
        {
            ctsToken.ThrowIfCancellationRequested();
            using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
            {
                var buffer = new byte[bufferSize];
                //lit seulement le début du fichier
                using (var ms = new MemoryStream(buffer, OFFSET, await fs.ReadAsync(buffer, OFFSET, bufferSize, ctsToken).ConfigureAwait(false)))
                {
                    ctsToken.ThrowIfCancellationRequested();

                    return BitmapDecoder
                        .Create(ms, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None)
                        .Frames[0].Metadata is BitmapMetadata bitmapMetaData ? ExtractMetadataFiltered(bitmapMetaData) : new List<MetadataKeyValue>();
                }
            }
        }

        private static List<MetadataKeyValue> ExtractHeaderMetadata(string imagePath, int bufferSize)
        {
            using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
            {
                var buffer = new byte[bufferSize];
                //lit seulement le début du fichier
                using (var ms = new MemoryStream(buffer, OFFSET, fs.Read(buffer, OFFSET, bufferSize)))
                {
                    return BitmapDecoder.Create(ms, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None).Frames[0].Metadata is BitmapMetadata bitmapMetaData 
                        ? ExtractMetadataFiltered(bitmapMetaData) 
                        : new List<MetadataKeyValue>();
                }
            }
        }

        private static List<MetadataKeyValue> ExtractMetadataFiltered(BitmapMetadata bitmapMetadata)
        {
            var currentQuery = bitmapMetadata.GetQuery(ConstantsMetadata.PNG_NATIVE_IMAGE_FORMAT_METADATA_TEXT);
            if (currentQuery is string str)
            {
                return str.DeserializeFromJson<List<MetadataKeyValue>>();
            }
            return new List<MetadataKeyValue>();
        }
    }
}

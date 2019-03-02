using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using PRF.Utils.ImageMetadata.Configuration;

namespace PRF.Utils.ImageMetadata.PNG
{
    public static class PngMetadataReader
    {
        private const int OFFSET = 0; // par du début du fichier = offset inutile mais plus clair que mettre '0' en dur
        private const int BUFFER_SIZE = 6144; // on commence par lire les premiers bytes du fichier (en général, ça suffit dans notre cas mais 2048 ne suffit pas)

        /// <summary>
        /// Récupère en asynchrone les métadonnées d'une image
        /// </summary>
        /// <param name="imagePath">le chemin du fichier</param>
        /// <param name="ctsToken">le token d'annulation</param>
        /// <param name="filters">les filtres identifiants les balises ("tEXt" "Parameters" pour IH500)</param>
        public static async Task<List<RawMetadata>> GetMetadataAsync(string imagePath, CancellationToken ctsToken, params string[] filters)
        {
            try
            {
                try
                {
                    return await ExtractHeaderMetadataAsync(imagePath, BUFFER_SIZE, ctsToken, filters).ConfigureAwait(false);
                }
                catch (FileFormatException)
                {
                    // si l'on arrive pas à lire le format de l'image, on retente avec un buffer plus grand (mais une seule fois)
                    return await ExtractHeaderMetadataAsync(imagePath, BUFFER_SIZE * 2, ctsToken, filters).ConfigureAwait(false);
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

        public static List<RawMetadata> GetMetadata(this FileInfo imagePath, params string[] filters)
        {
            return GetMetadata(imagePath.FullName, filters);
        }

        public static List<RawMetadata> GetMetadata(this string imagePath, params string[] filters)
        {
            try
            {
                try
                {
                    return ExtractHeaderMetadata(imagePath, BUFFER_SIZE, filters);
                }
                catch (FileFormatException)
                {
                    // si l'on arrive pas à lire le format de l'image, on retente avec un buffer plus grand (mais une seule fois)
                    return ExtractHeaderMetadata(imagePath, BUFFER_SIZE * 2, filters);
                }
            }
            catch (Exception e)
            {
                // relance une exception avec un message plus spécifique et l'excetion source en innerException
                throw new Exception($"Erreur lors de l'extraction des métadonnées de l'image: {imagePath}. {Environment.NewLine} => Exception: {e}", e);
            }
        }

        private static async Task<List<RawMetadata>> ExtractHeaderMetadataAsync(string imagePath, int bufferSize, CancellationToken ctsToken, params string[] filters)
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
                        .Frames[0].Metadata is BitmapMetadata bitmapMetaData ? ExtractMetadataFiltered(bitmapMetaData, filters) : new List<RawMetadata>();
                }
            }
        }

        private static List<RawMetadata> ExtractHeaderMetadata(string imagePath, int bufferSize, params string[] filters)
        {
            using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
            {
                var buffer = new byte[bufferSize];
                //lit seulement le début du fichier
                using (var ms = new MemoryStream(buffer, OFFSET, fs.Read(buffer, OFFSET, bufferSize)))
                {
                    return BitmapDecoder
                        .Create(ms, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None)
                        .Frames[0].Metadata is BitmapMetadata bitmapMetaData ? ExtractMetadataFiltered(bitmapMetaData, filters) : new List<RawMetadata>();
                }
            }
        }

        private static List<RawMetadata> ExtractMetadataFiltered(BitmapMetadata bitmapMetadata, params string[] filters)
        {
            var rawMetadataItems = new List<RawMetadata>();
            foreach (var query in bitmapMetadata.Where(o => filters.Any(o.Contains)))
            {
                var queryReader = bitmapMetadata.GetQuery(query);
                if (queryReader == null) continue;
                if (!(queryReader is BitmapMetadata innerBitmapMetadata))
                {
                    rawMetadataItems.Add(new RawMetadata(query, queryReader));
                }
                else
                {
                    Extract(rawMetadataItems, innerBitmapMetadata, query);
                }
            }
            return rawMetadataItems;
        }

        private static void Extract(List<RawMetadata> rawMetadataItems, BitmapMetadata bitmapMetadata, string query)
        {
            foreach (var relativeQuery in bitmapMetadata)
            {
                var queryReader = bitmapMetadata.GetQuery(relativeQuery);
                if (queryReader == null) continue;
                if (!(queryReader is BitmapMetadata innerBitmapMetadata))
                {
                    rawMetadataItems.Add(new RawMetadata(query + relativeQuery, queryReader));
                }
                else
                {
                    Extract(rawMetadataItems, innerBitmapMetadata, query + relativeQuery);
                }
            }
        }
    }
}

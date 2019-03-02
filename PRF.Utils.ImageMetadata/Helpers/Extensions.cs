using System;
using System.IO;
using PRF.Utils.ImageMetadata.Managers;

namespace PRF.Utils.ImageMetadata.Helpers
{
    /// <summary>
    /// Méthodes d'extensions pour l'extraction de métadonnées
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extrait du fichier les métadonnées correspondant au type de la clé et les encapsules dans un container
        /// </summary>
        /// <typeparam name="TKey">le type de clé</typeparam>
        /// <param name="file">le fichier d'où l'on cherche à extraire les métadonnées</param>
        /// <returns>le conteneur de métadonnées</returns>
        public static IMetadataContainer<TKey> ToMetadataContainer<TKey>(this FileInfo file) where TKey : Enum
        {
            return new MetadataContainer<TKey>(file);
        }
    }
}

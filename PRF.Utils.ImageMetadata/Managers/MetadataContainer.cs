using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using PRF.Utils.CoreComponents.Extensions;
using PRF.Utils.ImageMetadata.Configuration;
using PRF.Utils.ImageMetadata.Helpers;
using PRF.Utils.ImageMetadata.PNG;

namespace PRF.Utils.ImageMetadata.Managers
{
    /// <summary>
    /// classe qui s'occupe de fournir un stockage pour les métadonnées en cours de manipulation.
    /// Il permet de générer le code permettant l'insertion dans une image
    /// </summary>
    public interface IMetadataContainer<TKey> : IEnumerable<KeyValuePair<TKey, object>> where TKey : Enum
    {
        /// <summary>
        /// Ajoute une métadonnée au conteneur. remplace la données présente si elle existait déjà
        /// </summary>
        void Add(TKey key, object metadata);

        /// <summary>
        /// Renvoie le nombre de métadonnées dans ce conteneur
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Sauvegarde l'image dans un nouveau fichier en y insérant toutes les métadonnées présentent dans le conteneur
        /// </summary>
        /// <param name="file">le fichier cible (Il ne DOIT PAS EXISTER)</param>
        /// <param name="bmp">l'image à enregistré (elle n'est PAS disposée et doit être gérée par le client)</param>
        void SaveNewFile(FileInfo file, Bitmap bmp);

        /// <summary>
        /// Met à jour le fichier en y insérant toutes les métadonnées présentent dans le conteneur
        /// </summary>
        void UpdateFile(FileInfo file);

        /// <summary>
        ///Récupère la valeur correspondant à la clé si elle existe
        /// </summary>
        bool TryGetValue(TKey key, out object value);
    }

    /// <inheritdoc />
    public class MetadataContainer<TKey> : IMetadataContainer<TKey> where TKey : Enum
    {
        private readonly ConcurrentDictionary<TKey, object> _reference = new ConcurrentDictionary<TKey, object>();

        /// <summary>
        /// Dictionnaire de conversion pour ne pas retenter de convertir N fois des enum non convertibles
        /// </summary>
        private static readonly Dictionary<string, TKey> _converter = Enum.GetValues(typeof(TKey)).Cast<TKey>().ToDictionary(o => o.ToString(), o => o);

        /// <summary>
        /// Constructeur qui charge les métadonnées depuis un fichier
        /// </summary>
        /// <param name="file">le fichier servant de source à ce container</param>
        public MetadataContainer(FileInfo file) : this(ExtractRawMetadata(file)) { }
        
        /// <summary>
        /// Constructeur qui encapsule les métadonnées données
        /// </summary>
        /// <param name="metadata">la liste des métadonnées préalablement extraite</param>
        internal MetadataContainer(IEnumerable<MetadataKeyValue> metadata) : this()
        {
            foreach (var rawMetadata in metadata)
            {
                if (_converter.TryGetValue(rawMetadata.Key, out var convertedKey))
                {
                    // si une conversion existe entre la clé string et la clé enum on ajout l'élément
                    Add(convertedKey, rawMetadata.Value);
                }
            }
        }

        /// <summary>
        /// Constructeur par défaut. il ne charge aucune données
        /// </summary>
        public MetadataContainer()
        {
        }


        /// <inheritdoc />
        public void Add(TKey key, object metadata)
        {
            // n'ajoute pas de métadonnées si null
            if (metadata == null) return;

            _reference.AddOrUpdate(key, metadata, (key2, valueUpdate) => valueUpdate);
        }

        /// <inheritdoc />
        public int Count => _reference.Count;

        /// <inheritdoc />
        public void SaveNewFile(FileInfo file, Bitmap bmp)
        {
            PngMetadataWriter.SaveImageWithMetadata(bmp, file, GetQueries());
        }

        /// <inheritdoc />
        public void UpdateFile(FileInfo file)
        {
            PngMetadataWriter.UpdateImageWithMetadata(file, GetQueries());
        }

        public bool TryGetValue(TKey key, out object value)
        {
            return _reference.TryGetValue(key, out value);
        }

        /// <summary>
        /// Récupère une query prête à être écrite dans un fichier en utilisant le native image format des PNG (/tEXt or /[*]tEXt where * = 0 to N)
        /// </summary>
        private List<MetadataKeyValue> GetQueries()
        {
            return _reference.Select((o, i) => new MetadataKeyValue(o.Key.ToString(), o.Value)).ToList();
        }

        private static List<MetadataKeyValue> ExtractRawMetadata(FileInfo file)
        {
            if (file != null && file.ExistsExplicit())
            {
                return file.GetMetadata();
            }
            return new List<MetadataKeyValue>();
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, object>> GetEnumerator()
        {
            return _reference.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

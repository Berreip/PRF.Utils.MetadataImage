namespace PRF.Utils.ImageMetadata.Configuration
{
    internal class MetadataKeyValue
    {
        public MetadataKeyValue(string key, object value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// la clé identifiant la métadonnée (issue d'une enum normalement)
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// la valeur de la métadonnée sour forme de string
        /// </summary>
        public object Value { get; }
    }
}

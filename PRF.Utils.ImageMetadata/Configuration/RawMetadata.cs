namespace PRF.Utils.ImageMetadata.Configuration
{
    public class RawMetadata
    {
        public RawMetadata(string key, object value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Query id
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Query value
        /// </summary>
        public object Value { get; }

        public string WriteValue()
        {
            return Value.ToString();
        }
    }
}

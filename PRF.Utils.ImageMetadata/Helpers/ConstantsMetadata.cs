namespace PRF.Utils.ImageMetadata.Helpers
{
    internal static class ConstantsMetadata
    {
        // native image format des PNG
        // => /tEXt or /[*]tEXt where * = 0 to N
        // => tEXt/{str=*} where * = identifying keyword for text
        public const string PNG_NATIVE_IMAGE_FORMAT_METADATA_TEXT = @"/tEXt/PRF_Metadata";
    }
}

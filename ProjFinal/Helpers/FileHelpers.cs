namespace ProjFinal.Helpers
{
    public static class FileHelpers
    {
        /// <summary>
        /// Sanitizes a folder name by replacing invalid characters with underscores.
        /// </summary>
        public static string SanitizeFolderName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name.Trim();
        }
    }
}

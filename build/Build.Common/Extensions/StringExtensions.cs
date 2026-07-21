using System;

using Cake.Core.IO;

namespace Build.Common.Extensions
{
    public static class StringExtensions
    {
        public static DirectoryPath AsDirectoryPath(this string pathAsString) => new DirectoryPath(pathAsString);

        public static FilePath AsFilePath(this string pathAsString) => new FilePath(pathAsString);

        public static int AsInt(this string intValue) => int.Parse(intValue);

        public static Uri AsUri(this string uriValue) => new Uri(uriValue);
    }
}

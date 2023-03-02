using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HolyQuran.Models;
using Ionic.Zip;

//using Ionic.Zip;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HolyQuran.Services
{
    public class UploadService
    {
        private readonly QuranOptions _options;

        public UploadService(IOptions<QuranOptions> options)
        {
            _options = options?.Value;
        }

        public async Task<(string fileType, byte[] archiveData, string archiveName)> FetchFiles(
            int surahOrder,
            Rawy rawy = Rawy.Qalon,
            Readers readers = Readers.AbdAlRashedSofi)
        {
            var reader = ((int)readers).ToString();
            var path = Path.Combine(_options.Mp3Location, surahOrder.ToString(), rawy.ToString(), reader);

            await using var memoryStream = new MemoryStream();

            using var zip = new ZipFile();
            zip.AddDirectory(path);
            zip.Save(memoryStream);

            return ("application/zip", memoryStream.ToArray(), $"{surahOrder}-{rawy}-{reader}.zip");
        }

        public void SaveFile(List<IFormFile> files, string surahFolderName, Rawy rawy = Rawy.Qalon)
        {
            surahFolderName ??= string.Empty;

            var target = Path.Combine(_options.Mp3Location, surahFolderName, ((int)rawy).ToString());

            Directory.CreateDirectory(target);

            files.ForEach(async file =>
            {
                if (file.Length <= 0) return;
                var filePath = Path.Combine(target, file.FileName);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
            });
        }

        public static string SizeConverter(long bytes)
        {
            var fileSize = new decimal(bytes);
            var kilobyte = new decimal(1024);
            var megabyte = new decimal(1024 * 1024);
            var gigabyte = new decimal(1024 * 1024 * 1024);

            return fileSize switch
            {
                _ when fileSize < kilobyte => "Less then 1KB",
                _ when fileSize < megabyte =>
                    $"{Math.Round(fileSize / kilobyte, 0, MidpointRounding.AwayFromZero):##,###.##}KB",
                _ when fileSize < gigabyte =>
                    $"{Math.Round(fileSize / megabyte, 2, MidpointRounding.AwayFromZero):##,###.##}MB",
                _ when fileSize >= gigabyte =>
                    $"{Math.Round(fileSize / gigabyte, 2, MidpointRounding.AwayFromZero):##,###.##}GB",
                _ => "n/a",
            };
        }
    }
}
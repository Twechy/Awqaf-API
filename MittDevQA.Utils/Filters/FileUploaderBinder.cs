using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Utils.Others;

namespace Utils.Filters
{
    public class FileUploaderBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var instanceType = bindingContext.ModelType;
            var instance = RetriveObj(bindingContext);
            if (instance != null)
            {
                var filepropertyInfo = instanceType.GetProperty("FormFile");
                var fileNamePropertyInfo = instanceType.GetProperty("FileName");
                var exstsNamePropertyInfo = instanceType.GetProperty("Exsts");

                if (filepropertyInfo != null && fileNamePropertyInfo != null)
                {
                    var hostingEnv = getHostingEnv(bindingContext);
                    var formFile = (IFormFile)filepropertyInfo.GetValue(instance);
                    if (formFile != null)
                    {
                        var fileName = await SaveFile(formFile, hostingEnv,
                            (string[])exstsNamePropertyInfo?.GetValue(instance));
                        fileNamePropertyInfo.SetValue(instance, fileName);
                    }
                }

                bindingContext.Result = ModelBindingResult.Success(instance);
            }
        }

        private object RetriveObj(ModelBindingContext bindingContext)
        {
            try
            {
                var model = Activator.CreateInstance(bindingContext.ModelType);
                var modelType = model.GetType();
                var form = bindingContext.HttpContext.Request.Form;
                foreach (var propertyInfo in modelType.GetProperties())
                {
                    var myKey = propertyInfo.Name;
                    if (!propertyInfo.CanRead || !form.Keys.Contains(myKey)) continue;
                    try
                    {
                        var value = form[myKey].ToString();
                        propertyInfo.SetValue(model, Convert.ChangeType(value,
                            propertyInfo.PropertyType), null);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                var filePropertyInfo = modelType.GetProperty("FormFile");
                filePropertyInfo.SetValue(model, form.Files.First());

                return model;
            }
            catch
            {
                // ignored
            }

            return null;
        }

        private FilesPathOptions getHostingEnv(ModelBindingContext bindingContext)
        {
            var serviceProvider = bindingContext.HttpContext.RequestServices;
            return serviceProvider.GetService<IOptions<FilesPathOptions>>()?.Value;
        }

        private static async Task<string> SaveFile(IFormFile formFile, FilesPathOptions filesPathOptions,
            params string[] allowedExsts)
        {
            var fileName = Path.GetFileName(formFile.FileName);
            if (allowedExsts.Length > 0 && allowedExsts.All(x => x.ToLower() != Path.GetExtension(fileName).ToLower()
                                                                     .Replace(".", "")))
                throw new AppException("غير مسموح بهذا الامتداد");

            var uploads = filesPathOptions?.FilesPath;
            var filePath = Path.Combine(uploads, fileName);

            if (allowedExsts.Contains("jpg") || allowedExsts.Contains(".jpg"))
            {
                compressSave(formFile.OpenReadStream(), fileName, filePath, formFile.Length / 1024);
            }
            else
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(fileStream);
                }
            }

            return fileName;
        }

        private static void compressSave(Stream stream, string orginalName, string fullName, long fileLen)
        {
            using (var bmp1 = new Bitmap(stream))
            {
                var jpgEncoder = GetEncoder(Path.GetExtension(orginalName) == ".jpg" ? ImageFormat.Jpeg : Path.GetExtension(orginalName) == ".png" ? ImageFormat.Png : null);

                var QualityEncoder = Encoder.Quality;

                var myEncoderParameters = new EncoderParameters(1);

                var myEncoderParameter = new EncoderParameter(QualityEncoder, fileLen < 1000 ? 50L : 30L);

                myEncoderParameters.Param[0] = myEncoderParameter;
                bmp1.Save(fullName, jpgEncoder, myEncoderParameters);
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private static double getImageSize(string fileName)
        => (new FileInfo(fileName)).Length / 1024;

        public class FileUploaderBinderProvider : IModelBinderProvider
        {
            private readonly List<Type> _modelsTypes;

            public FileUploaderBinderProvider(params Type[] modelsToImplementFileUploaderProviders)
            {
                _modelsTypes = modelsToImplementFileUploaderProviders.ToList();
            }

            public IModelBinder GetBinder(ModelBinderProviderContext context)
            {
                if (context == null)
                    throw new AppException(nameof(context));

                return _modelsTypes.Any(x => x == context.Metadata.ModelType)
                    ? new BinderTypeModelBinder(typeof(FileUploaderBinder))
                    : null;
            }
        }
    }

    public class FilesPathOptions
    {
        public string FilesPath { get; set; }
    }
}
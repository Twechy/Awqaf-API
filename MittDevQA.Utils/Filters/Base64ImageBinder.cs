using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Utils.Filters
{
    public class Base64ImageBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var instanceType = bindingContext.ModelType;
            var instance = retriveObj(bindingContext);
            if (instance != null)
            {
                var propertyInfo = instanceType.GetProperty("Image");

                if (propertyInfo == null)
                    propertyInfo = instanceType.GetProperty("Logo");

                if (propertyInfo != null)
                {
                    var imageService = getImageService(bindingContext);
                    var image = propertyInfo.GetValue(instance).ToString();
                    var result = imageService.Save(image);
                    propertyInfo.SetValue(instance, result);

                    bindingContext.Result = ModelBindingResult.Success(instance);
                }
            }

            return Task.CompletedTask;
        }

        private ImageSaverService getImageService(ModelBindingContext bindingContext)
        {
            var serviceProvider = bindingContext.HttpContext.RequestServices;
            return new ImageSaverService(serviceProvider.GetService<IWebHostEnvironment>());
        }

        private object retriveObj(ModelBindingContext bindingContext)
        {
            try
            {
                var stream = bindingContext.HttpContext.Request.Body;
                var reader = new StreamReader(stream);
                var instanceAsJson = reader.ReadToEnd();

                return JsonConvert.DeserializeObject(instanceAsJson,
                    bindingContext.ModelType);
            }
            catch
            {
                return null;
            }
        }
    }

    public class Base64ImageBinderProvider : IModelBinderProvider
    {
        private List<Type> _modelsTypes;

        public Base64ImageBinderProvider(params Type[] modelsToImplementBase64ProvidersOnIt)
        {
            _modelsTypes = modelsToImplementBase64ProvidersOnIt.ToList();
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (_modelsTypes.Any(x => x.Equals(context.Metadata.ModelType)))
            {
                return new BinderTypeModelBinder(typeof(Base64ImageBinder));
            }

            return null;
        }
    }

    public class ImageSaverService
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ImageSaverService(IWebHostEnvironment environment)
        {
            _hostingEnvironment = environment;
        }

        public string Save(string imageData)
        {
            try
            {
                if ((imageData.Contains(".png") &&
                     File.Exists(Path.Combine(_hostingEnvironment.WebRootPath,
                         "imgs", imageData))) || string.IsNullOrEmpty(imageData)
                                              || string.IsNullOrWhiteSpace(imageData))
                    return imageData;

                var fileName = Guid.NewGuid().ToString() + ".png";

                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "imgs", fileName);

                saveFile(filePath, imageData);
                return fileName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private void saveFile(string fullPath, string imageData)
        {
            using (var imageFile = new FileStream(fullPath, FileMode.Create))
            {
                var bytess = Convert.FromBase64String(imageData);
                imageFile.Write(bytess, 0, bytess.Length);
                imageFile.Flush();
            }
        }
    }
}
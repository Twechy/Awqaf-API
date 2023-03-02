using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Utils.Others
{
    public static class JsConverter
    {
        public static string ToJson(this object value)
        {
            if (value == null) throw new AppException("json value is null..");

            var serializeObject = JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            });
            return serializeObject;
        }

        public static T FromJson<T>(this string value) where T : class
        {
            if (value == null) throw new AppException($"value is null.. from the type => {typeof(T).Name}");
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}
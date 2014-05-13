using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using DD4T.ContentModel;
using System.IO;
using DD4T.ContentModel.Contracts.Serializing;

namespace DD4T.Serialization
{
    public class JSONSerializerService : ISerializerService
    {
        private JsonSerializer _serializer = null;
        public JsonSerializer Serializer
        {
            get
            {
                if (_serializer == null)
                {
                    _serializer = new JsonSerializer();
                    _serializer.NullValueHandling = NullValueHandling.Ignore;
                    _serializer.Converters.Add(new FieldConverter());

                }
                return _serializer;
            }
        }
        public string Serialize<T>(T input) where T : ContentModel.IRepositoryLocal
        {
            // serialize into JSON
            JsonWriter jsonWriter;
            

            string jsonText;
            using (StringWriter sw = new StringWriter())
            {
                jsonWriter = new JsonTextWriter(sw);
                Serializer.Serialize(jsonWriter, input);
                jsonText = sw.ToString();
            }
            return jsonText;
        }

        public T Deserialize<T>(string input) where T : ContentModel.IRepositoryLocal
        {
            using (var inputValueReader = new StringReader(input))
            {
                JsonTextReader reader = new JsonTextReader(inputValueReader);
                return (T)Serializer.Deserialize<T>(reader);
            }
        }

        public bool IsAvailable()
        {
            return Type.GetType("Newtonsoft.Json.JsonSerializer") != null;
        }
    }
    public class FieldConverter : CustomCreationConverter<IField>
    {
        public override IField Create(Type objectType)
        {
            return new Field();
        }
    }
}

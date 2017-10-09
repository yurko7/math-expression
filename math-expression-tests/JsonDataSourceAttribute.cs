using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YuKu.MathExpression.Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    internal sealed class JsonDataSourceAttribute : Attribute, ITestDataSource
    {
        public JsonDataSourceAttribute(String fileName)
        {
            FileName = fileName;
        }

        public String FileName { get; }

        public IEnumerable<Object[]> GetData(MethodInfo methodInfo)
        {
            using (TextReader streamReader = File.OpenText(FileName))
            {
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    JObject jDataInfo = JObject.Load(jsonReader);
                    _displayNameFormat = jDataInfo.Value<String>("displayName");
                    JArray jData = (JArray) jDataInfo.GetValue("data");
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    IEnumerable<Object[]> data = jData
                        .Cast<JObject>()
                        .Select(jObject => parameters
                            .Select(parameter => jObject.TryGetValue(parameter.Name, out JToken value)
                                ? value.ToObject(parameter.ParameterType)
                                : parameter.ParameterType.GetDefaultValue())
                            .ToArray());
                    return data;
                }
            }
        }

        public String GetDisplayName(MethodInfo methodInfo, Object[] data)
        {
            return String.Format(_displayNameFormat, data);
        }

        private String _displayNameFormat;
    }
}

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace sourcecodefiles
{
    public class JSONManipulator
    {
        public string SerializeDictionary(Dictionary<string, object> dict)
        {
            return JsonSerializer.Serialize(dict);
        }

        public Dictionary<string, object> DeserializeDictionary(string json)
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        }
    }
}

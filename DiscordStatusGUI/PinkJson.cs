using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace PinkJson
{
    public class Json : IList<JsonObject>
    {
        const string EOL = "\r\n";

        private List<JsonObject> jsonObjects = new List<JsonObject>();

        /// <summary>
        /// Сreates a JSON-object from an AnonymousType
        /// </summary>
        /// <param name="json"></param>
        public Json(dynamic json)
        {
            if (!CheckIfAnonymousType(json.GetType()))
                if (!CheckIfArray(json.GetType()))
                    throw new Exception("Unknown data type");

            if (CheckIfArray(json.GetType()))
            {
                //Method
                JsonObjectArray ToArray(dynamic jsonn)
                {
                    var obj = new JsonObjectArray();
                    foreach (var elem in jsonn)
                    {
                        object p;
                        if (CheckIfAnonymousType(elem.GetType()))
                            p = new Json(elem);
                        else
                            p = new JsonObject(null, elem);
                        (obj as JsonObjectArray).Add(p);
                    }
                    return obj as JsonObjectArray;
                }

                jsonObjects.Add(new JsonObject(null, ToArray(json)));
            }
            else
                jsonObjects = ConvertAnonymousType(json);
        }

        /// <summary>
        /// Creates a JSON-object from a string view
        /// </summary>
        /// <param name="json"></param>
        public Json(string json)
        {
            if (!CheckIfCorrectJson(json))
                throw new Exception("Json syntax error");

            var IsArray = json[0] == '[';
            json = Trim(json.Trim('\r', '\n', ' '), 1).Replace("\t", "  ");

            if (IsArray)
            {
                //json = json.Trim('\r', '\n', ' ');
                //var arr = SplitJson(json);
                var obj = JsonArrayFromString(json);
                /*foreach (var arrObj in arr)
                {
                    object p;
                    if (arrObj[0] == '{' || arrObj[0] == '[')
                        p = new Json(arrObj);
                    else
                        p = new JsonObject(null, (arrObj[0] == '\"' ? Trim(arrObj, 1) : DetectType(arrObj)));
                    (obj as JsonObjectArray).Add(p);
                }*/
                jsonObjects.Add(new JsonObject(null, obj));
            }
            else
            {
                var jsonObjectsString = SplitJson(json);

                foreach (var elem in jsonObjectsString)
                {
                    if (string.IsNullOrEmpty(elem))
                        break;
                    var splitter = -1;
                    var locked = false;
                    var prew = ' ';
                    //Find splitter :
                    for (var i = 0; i < elem.Length; i++)
                    {
                        var ch = elem[i];
                        if (ch == '"' && prew != '\\')
                            locked = !locked;
                        if (locked == false && ch == ':')
                        {
                            splitter = i;
                            break;
                        }
                        prew = ch;
                    }
                    var key = Trim(elem.Substring(0, splitter).Trim('\r', '\n', ' '), 1);
                    var value = elem.Substring(splitter + 1).Trim('\r', '\n', ' ');

                    object obj = DetectType(value);
                    if (value[0] == '\"')
                        obj = Trim(value, 1);
                    switch (value[0])
                    {
                        case '[':
                            /*value = Trim(value.Trim('\r', '\n', ' '), 1).Trim('\r', '\n', ' ');
                            var arr = SplitJson(value);
                            obj = new JsonObjectArray();
                            foreach (var arrObj in arr)
                            {
                                object p;
                                if (arrObj[0] == '{')
                                    p = new Json(arrObj);
                                else
                                    p = new JsonObject(null, (arrObj[0] == '\"' ? Trim(arrObj, 1) : DetectType(arrObj)));
                                (obj as JsonObjectArray).Add(p);
                            }*/
                            obj = JsonArrayFromString(value, true);
                            break;
                        case '{':
                            obj = new Json(value);
                            break;
                    }

                    jsonObjects.Add(new JsonObject(key, obj));
                }
            }
        }

        private JsonObjectArray JsonArrayFromString(string json, bool trim = false)
        {
            json = json.Trim('\r', '\n', ' ');
            if (trim == true)
                json = Trim(json, 1).Trim('\r', '\n', ' ');
            var arr = SplitJson(json);
            var obj = new JsonObjectArray();
            foreach (var arrObj in arr)
            {
                object p;
                if (arrObj[0] == '{' || arrObj[0] == '[')
                    p = new Json(arrObj);
                else
                    p = new JsonObject(null, (arrObj[0] == '\"' ? Trim(arrObj, 1) : DetectType(arrObj)));
                (obj as JsonObjectArray).Add(p);
            }
            return obj;
        }

        private dynamic DetectType(string val)
        {
            try { return Convert.ToInt64(val); } catch { }
            try { return Convert.ToDouble(val.Replace(".", ",")); } catch { }
            try { return Convert.ToBoolean(val); } catch { }
            if (val == "null") return "";
            return val;
        }

        private List<string> SplitJson(string json)
        {
            var jsonObjectsString = new List<string>();
            int brk = 0;
            var tmp = "";
            var locked = false;
            var prew = ' ';
            foreach (var ch in json)
            {
                if (ch == '}' || ch == ']')
                    brk--;
                if ((ch == '"') && prew != '\\')
                    locked = !locked;
                if (brk == 0 && ch == ',' && locked == false)
                {
                    jsonObjectsString.Add(tmp.Trim(',', '\r', '\n', ' '));
                    tmp = "";
                }
                if (ch == '{' || ch == '[')
                    brk++;
                tmp += ch;
                prew = ch;
            }
            if (!string.IsNullOrEmpty(tmp))
                jsonObjectsString.Add(tmp.Trim(',', '\r', '\n', ' '));

            return jsonObjectsString;
        }

        private static string Trim(string str, int count)
        {
            return str.Substring(count, str.Length - (count * 2));
        }

        private static List<JsonObject> ConvertAnonymousType(object value)
        {
            Type type = value.GetType();
            var genericType = type.GetGenericTypeDefinition();
            var parameterTypes = genericType.GetConstructors()[0]
                                            .GetParameters()
                                            .Select(p => p.ParameterType)
                                            .ToList();
            var propertyNames = genericType.GetProperties()
                                           .OrderBy(p => parameterTypes.IndexOf(p.PropertyType))
                                           .Select(p => p.Name);

            return propertyNames.Select<string, JsonObject>(name =>
            {
                var val = !CheckIfAnonymousType(type.GetProperty(name).GetValue(value, null).GetType()) ? 
                    type.GetProperty(name).GetValue(value, null) : 
                    new Json(type.GetProperty(name).GetValue(value, null));
                if (CheckIfArray(type.GetProperty(name).GetValue(value, null).GetType()))
                {
                    val = new JsonObjectArray();
                    var obj = type.GetProperty(name).GetValue(value, null) as Array;
                    foreach (var elem in obj)
                    {
                        if (CheckIfAnonymousType(elem.GetType()))
                            (val as JsonObjectArray).Add(new Json(elem));
                        else
                            (val as JsonObjectArray).Add(new JsonObject(null, elem));
                    }
                }
                return new JsonObject(type.GetProperty(name).Name, val);//type.GetProperty(name).GetValue(value, null)
            }).OfType<JsonObject>().ToList();
        }

        /// <summary>
        /// Checks whether the json string is correct
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public bool CheckIfCorrectJson(string json)
        {
            var i = 0;
            try
            {
                if ((json.Trim()[0] != '{' || json.Trim()[json.Trim().Length - 1] != '}') && (json.Trim()[0] != '[' || json.Trim()[json.Trim().Length - 1] != ']'))
                    return false;
            }
            catch { return false; }
            foreach (var ch in json)
            {
                if (ch == '[' || ch == '{')
                    i++;
                if (ch == ']' || ch == '}')
                    i--;
            }
            return i == 0;
        }

        private static bool CheckIfAnonymousType(Type type)
        {
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        private static bool CheckIfArray(Type type)
        {
            return //Attribute.IsDefined(type, typeof(Array), false)
                /*&& type.IsGenericType && type.Name.Contains("Array")*/
                /*&& (*/type.Name.EndsWith("]") /*|| type.Name.StartsWith("VB$"))*/
                /*&& (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic*/;
        }

        /// <summary>
        /// Convenient representation of a JSON-object
        /// </summary>
        /// <param name="space">Number of spaces before each nested object</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public string Stringify(int space = 4, int count = 1)
        {
            var result = "";
            var IsMainArray = (CheckIfArray(jsonObjects[0].Value.GetType()) || jsonObjects[0].Value.GetType() == typeof(JsonObjectArray)) && jsonObjects[0].Key == null;
            if (IsMainArray)
                count -= 1;
            else
                result = "{" + EOL;

            for (var i = 0; i < jsonObjects.Count; i++)
            {
                var elem = jsonObjects[i];
                result += new string(' ', space * count) + elem.Stringify(space, count) + (i != jsonObjects.Count - 1 ? "," : "") + EOL;
            }

            return (!IsMainArray ? result + new string(' ', space * (count - 1)) + "}" : result);
        }

        //List methods
        /// <summary>
        /// Gets the element at the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public JsonObject ElementByKey(string key)
        {
            foreach (var jsonObject in jsonObjects)
                if (jsonObject.Key == key)
                    return jsonObject;
            return null;
        }

        /// <summary>
        /// Gets the index of element at the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int IndexByKey(string key)
        {
            for (var i = 0; i < jsonObjects.Count; i++)
            {
                var jsonObject = jsonObjects[i];
                if (jsonObject.Key == key)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Deletes an element by the specified key
        /// </summary>
        /// <param name="key"></param>
        public void RemoveByKey(string key)
        {
            jsonObjects.RemoveAt(IndexByKey(key));
        }

        /// <summary>
        /// Gets or sets the element at the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public JsonObject this[string key]
        {
            get
            {
                return ElementByKey(key);
            }
            set
            {
                jsonObjects[IndexByKey(key)] = value;
            }
        }

        /// <summary>
        /// Returns a string that represents the current JSON-object
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var IsMainArray = (CheckIfArray(jsonObjects[0].Value.GetType()) || jsonObjects[0].Value.GetType() == typeof(JsonObjectArray)) && jsonObjects[0].Key == null;
            if (IsMainArray)
                return string.Join(", ", jsonObjects);
            else
                return "{ " + string.Join(", ", jsonObjects) + " }";
        }

        #region Default
        //Default ICollection methods
        public void Add(JsonObject jsonObject)
        {
            jsonObjects.Add(jsonObject);
        }

        public void Clear()
        {
            jsonObjects.Clear();
        }

        public bool Contains(JsonObject jsonObject)
        {
            return jsonObjects.Contains(jsonObject);
        }

        public void CopyTo(JsonObject[] array, int arrayIndex)
        {
            jsonObjects.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return jsonObjects.Count();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(JsonObject jsonObject)
        {
            return jsonObjects.Remove(jsonObject);
        }

        //Default List methods
        public int IndexOf(JsonObject jsonObject)
        {
            return jsonObjects.IndexOf(jsonObject);
        }

        public void Insert(int index, JsonObject jsonObject)
        {
            jsonObjects.Insert(index, jsonObject);
        }

        public void RemoveAt(int index)
        {
            jsonObjects.RemoveAt(index);
        }

        public JsonObject this[int index]
        {
            get
            {
                return jsonObjects[index];
            }
            set
            {
                jsonObjects[index] = value;
            }
        }

        public IEnumerator<JsonObject> GetEnumerator()
        {
            return jsonObjects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion Default
    }

    public class JsonObject
    {
        /// <summary>
        /// Represents the element key
        /// </summary>
        public string Key;
        private dynamic _Value;

        /// <summary>
        /// Gets or sets the element value
        /// </summary>
        public dynamic Value
        {
            get
            {
                if (_Value.GetType() == typeof(bool))
                    return new FixBoolean(_Value);
                return _Value;
            }
            set
            {
                _Value = value;
            }
        }

        /// <summary>
        /// Сreates a new JSON-object
        /// </summary>
        /// <param name="key">Element key</param>
        /// <param name="value">Element value</param>
        public JsonObject(string key, object value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the JSON-object from current object value
        /// </summary>
        /// <param name="key">Element key</param>
        /// <returns>Can represent any JSON-object</returns>
        public dynamic this[string key]
        {
            get
            {
                return Value[key];
            }
            set
            {
                Value[key].Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the JSON-object from current object value
        /// </summary>
        /// <param name="index">Element index</param>
        /// <returns>Can represent any JSON-object</returns>
        public dynamic this[int index]
        {
            get
            {
                return Value[index];
            }
            set
            {
                Value[index].Value = value;
            }
        }

        /// <summary>
        /// Convenient representation of a JSON-object
        /// </summary>
        /// <param name="space">Number of spaces before each nested object</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public string Stringify(int space = 4, int count = 0)
        {
            var key = string.IsNullOrEmpty(Key) ? "" : "\"{0}\": ";
            var val = Value.GetType() == typeof(string) ? "\"{1}\"" : "{1}";
			if (Value.GetType() == typeof(string) && string.IsNullOrEmpty(Value))
                val = "null";
            if (Value.GetType() == typeof(double))
                val = Value.ToString().Replace(",", ".");
            object ext;
            try
            {
                ext = string.Format(key + val, Key, Value.Stringify(space, count + 1));
            } catch
            {
                ext = string.Format(key + val, Key, Value);
            }

            return ext.ToString();
        }

        /// <summary>
        /// Returns a string that represents the current JSON-object
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var key = string.IsNullOrEmpty(Key) ? "" : "\"{0}\": ";
            var val = Value.GetType() == typeof(string) ? "\"{1}\"" : "{1}";
			if (Value.GetType() == typeof(string) && string.IsNullOrEmpty(Value))
				val = "null";
            if (Value.GetType() == typeof(double))
                val = Value.ToString().Replace(",", ".");
            return string.Format(key + val, Key, Value);
        }
    }

    public class JsonObjectArray : List<object>
    {
        const string EOL = "\r\n";

        /// <summary>
        /// Convenient representation of a JSON-object
        /// </summary>
        /// <param name="space">Number of spaces before each nested object</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public string Stringify(int space = 4, int count = 1)
        {
            var result = "[" + EOL;
            for (var i = 0; i < this.Count; i++)
            {
                dynamic elem = this[i];
                try
                {
                    result += new string(' ', space * count) + elem.Stringify(space, count + 1);
                }
                catch
                {
                    result += new string(' ', space * count) + elem;
                }
                finally
                {
                    if (i != this.Count - 1)
                        result += ",";
                    result += EOL;
                }
            }
            return result + new string(' ', space * (count - 1)) + "]";
        }

        /// <summary>
        /// Returns a string that represents the current JSON-object
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return "[ " + string.Join(", ", this) + " ]";
        }
    }

    class FixBoolean
    {
        /// <summary>
        /// Original boolean object
        /// </summary>
        public bool Value;

        /// <summary>
        /// Creates a boolean object with the corrected ToString() method
        /// </summary>
        /// <param name="bl"></param>
        public FixBoolean(bool bl)
        {
            Value = bl;
        }

        /// <summary>
        /// boolean with a small letter
        /// </summary>
        /// <returns>A string that represents the current boolean</returns>
        public override string ToString()
        {
            return Value.ToString().ToLower();
        }
    }
}

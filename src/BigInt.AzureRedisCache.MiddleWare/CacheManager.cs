using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace BigInt.AzureRedisCache.MiddleWare
{
    public static class CacheManager
    {
        public static IDatabase AzureredisDb = RedisCache.GetDatabase();
        public static T Get<T>(string key)
        {
            var r = AzureredisDb.StringGet(key);
            return Deserialize<T>(r);
        }
        public static List<T> GetList<T>(string key)
        {
            return (List<T>)Get(key);
        }

        public static void SetList<T>(string key, List<T> list, TimeSpan expiry)
        {
            Set(key, list, expiry);
        }

        public static object Get(string key)
        {
            return Deserialize<object>(AzureredisDb.StringGet(key));
        }

        public static void Set(string key, object value, TimeSpan expiry)
        {
            AzureredisDb.StringSet(key, Serialize(value), expiry);
        }

        static byte[] Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, o);
                byte[] objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        static T Deserialize<T>(byte[] stream)
        {
            if (stream == null)
            {
                return default(T);
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(stream))
            {
                T result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FitnessTracker.Infrastructure
{
    public class JsonDataStore<T>
    {
        private readonly string _filePath;
        private readonly DataContractJsonSerializer _serializer;

        public JsonDataStore(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _serializer = new DataContractJsonSerializer(typeof(List<T>));
        }

        public Task<List<T>> LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                if (!File.Exists(_filePath))
                    return new List<T>();

                try
                {
                    using (var stream = File.OpenRead(_filePath))
                    {
                        var result = _serializer.ReadObject(stream) as List<T>;
                        return result ?? new List<T>();
                    }
                }
                catch (Exception)
                {
                    return new List<T>();
                }
            }, cancellationToken);
        }

        public Task SaveAsync(List<T> items, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (var stream = File.Open(_filePath, FileMode.Create))
                {
                    _serializer.WriteObject(stream, items);
                }
            }, cancellationToken);
        }
    }
}
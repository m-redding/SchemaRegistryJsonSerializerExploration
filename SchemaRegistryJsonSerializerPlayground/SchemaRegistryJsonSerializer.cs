using Azure.Data.SchemaRegistry;
using Azure.Messaging;
using Newtonsoft.Json.Linq;
using NJsonSchema.Generation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SchemaRegistryJsonSerializerPlayground
{
    internal class SchemaRegistryJsonSerializer
    {
        private SchemaRegistryClient _client;
        private SchemaRegistryJsonSerializerOptions _options;
        private string _groupName;
        private string _jsonType = "todo";

        public SchemaRegistryJsonSerializer(SchemaRegistryClient client, string groupName, SchemaRegistryJsonSerializerOptions options) 
        {
            _client = client;
            _options = options;
            _groupName = groupName;
        }

        public async ValueTask<MessageContent> SerializeAsync(object data, Type dataType = default, Type messageType = default, CancellationToken cancellationToken = default)
        {
            messageType ??= typeof(MessageContent);
            if (messageType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new InvalidOperationException(
                    $"The type {messageType} must have a public parameterless constructor in order to use it as the 'MessageContent' type to serialize to.");
            }

            var message = (MessageContent)Activator.CreateInstance(messageType);

            (string schemaId, BinaryData bd) = await SerializeInternalAsync(data, dataType, true, cancellationToken).ConfigureAwait(false);

            message.Data = bd;
            message.ContentType = $"{_jsonType}+{schemaId}";
            return message;
        }

        private async ValueTask<(string SchemaId, BinaryData Data)> SerializeInternalAsync(
            object value,
            Type dataType,
            bool async,
            CancellationToken cancellationToken)
        {
            dataType ??= value?.GetType() ?? typeof(object);

            var settings = new JsonSchemaGeneratorSettings();
            settings.SerializerSettings = null; // use System.Text.Json instead of Newtonsoft
            settings.SerializerOptions  = new JsonSerializerOptions();
            var generator = new JsonSchemaGenerator(settings);
            var schema = generator.Generate(dataType);
            var jsonSerializerOptions = new JsonSerializerOptions();

            using Stream stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, value, dataType, options: jsonSerializerOptions, cancellationToken).ConfigureAwait(false);

            stream.Position = 0;
            BinaryData data = BinaryData.FromStream(stream);

            var schemaString = schema.ActualSchema.ToJson();
            var schemaProperties = _options.AutoRegisterSchemas 
                ? await _client.RegisterSchemaAsync(_groupName, schema.Title, schemaString, SchemaFormat.Json, cancellationToken).ConfigureAwait(false)
                : await _client.GetSchemaPropertiesAsync(_groupName, schema.Title, schemaString, SchemaFormat.Json, cancellationToken).ConfigureAwait(false);

            var id = schemaProperties.Value.Id;
            return (id, data);
        }
    }
}

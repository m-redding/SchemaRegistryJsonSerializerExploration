//using Azure.Data.SchemaRegistry;
//using Azure.Messaging;
//using Newtonsoft.Json.Schema;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SchemaRegistryJsonSerializerPlayground
//{
//    internal class SchemaRegistryJsonSerializerGeneric
//    {
//        private SchemaRegistryClient _client;
//        private SchemaRegistryJsonSerializerOptions _options;
//        private string _groupName;
//        private string _jsonType = "jsonType"; //TODO

//        public SchemaRegistryJsonSerializerNewtonsoft(SchemaRegistryClient client, string groupName, SchemaRegistryJsonSerializerOptions options)
//        {
//            _client = client;
//            _options = options;
//            _groupName = groupName;
//        }

//        public async ValueTask<MessageContent> SerializeAsync(object data, Type dataType = default, Type messageType = default, CancellationToken cancellationToken = default)
//        {
//            messageType ??= typeof(MessageContent);
//            if (messageType.GetConstructor(Type.EmptyTypes) == null)
//            {
//                throw new InvalidOperationException(
//                    $"The type {messageType} must have a public parameterless constructor in order to use it as the 'MessageContent' type to serialize to.");
//            }

//            var message = (MessageContent)Activator.CreateInstance(messageType);

//            (string schemaId, BinaryData bd) = await SerializeInternalAsync(data, dataType, true, cancellationToken).ConfigureAwait(false);

//            message.Data = bd;
//            message.ContentType = $"{_jsonType}+{schemaId}";
//            return message;
//        }

//        private async ValueTask<(string SchemaId, BinaryData Data)> SerializeInternalAsync(
//            object value,
//            Type dataType,
//            bool async,
//            CancellationToken cancellationToken)
//        {
//            dataType ??= value?.GetType() ?? typeof(object);

//            // generate schema from type

//            using Stream stream = new MemoryStream();
//            // serialize the data into the stream

//            stream.Position = 0;
//            BinaryData data = BinaryData.FromStream(stream);

//            // get the schema string
//            var schemaProperties = _options.AutoRegisterSchemas
//                ? await _client.RegisterSchemaAsync(_groupName, schema.Title, schemaString, SchemaFormat.Json, cancellationToken).ConfigureAwait(false)
//                : await _client.GetSchemaPropertiesAsync(_groupName, schema.Title, schemaString, SchemaFormat.Json, cancellationToken).ConfigureAwait(false);

//            var id = schemaProperties.Value.Id;
//            return (id, data);
//        }

//        public async ValueTask<TData> DeserializeAsync<TData>(
//            MessageContent content,
//            CancellationToken cancellationToken = default) => (TData)await DeserializeAsync(content, typeof(TData), cancellationToken).ConfigureAwait(false);


//        public async ValueTask<object> DeserializeAsync(
//            MessageContent content,
//            Type dataType,
//            CancellationToken cancellationToken = default)
//        {
//            var contentType = content.ContentType;

//            string[] contentTypeArray = contentType.ToString().Split('+');
//            if (contentTypeArray.Length != 2 || contentTypeArray[0] != _jsonType)
//            {
//                throw new FormatException("Content type was not in the expected format of 'avro/binary+schema-id', where 'schema-id' " +
//                                          "is the Schema Registry schema ID.");
//            }
//            string schemaId = contentTypeArray[1];

//            return await DeserializeInternalAsync(content.Data, dataType, schemaId, cancellationToken).ConfigureAwait(false);
//        }

//        private async ValueTask<object> DeserializeInternalAsync(
//            BinaryData data,
//            Type dataType,
//            string schemaId,
//            CancellationToken cancellationToken)
//        {
//            // Pull the schema from the service using the id stored in the Content Type
//            var schemaDefinition = (await _client.GetSchemaAsync(schemaId, cancellationToken).ConfigureAwait(false)).Value.Definition;
//            // Convert to a json schema type

//            var dataStream = data.ToStream();
//            // deserialize from stream

//            // validate data with schema

//            // validate the payload?

//            return objectToReturn;
//        }
//    }
//}

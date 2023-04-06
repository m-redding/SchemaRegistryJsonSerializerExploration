using Azure.Identity;
using Azure.Data.SchemaRegistry;
using Azure.Messaging.EventHubs;

namespace SchemaRegistryJsonSerializerPlayground
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var fullyQualifiedNamespace = "mredding-schematest.servicebus.windows.net";
            var groupName = "mredding-test-json";
            var client = new SchemaRegistryClient(fullyQualifiedNamespace: fullyQualifiedNamespace, credential: new DefaultAzureCredential());

            var employee = new Employee { Age = 42, Name = "Caketown" };

            var serializer = new SchemaRegistryJsonSerializer(client, groupName, new SchemaRegistryJsonSerializerOptions { AutoRegisterSchemas = true });

            EventData eventData = (EventData)await serializer.SerializeAsync(employee, messageType: typeof(EventData));

            string name = "product";
            SchemaFormat format = SchemaFormat.Json;
            // Example schema's definition

            string draft4definition = @"
            {
                ""name"":""Product"",
                ""properties"":{
                ""id"":{
                    ""type"":""number"",
                    ""description"":""Product identifier"",
                    ""required"":true
                },
                ""name"":{
                    ""description"":""Name of the product"",
                    ""type"":""string"",
                    ""required"":true
                },
                ""price"":{
                    ""required"":true,
                    ""type"": ""number"",
                    ""minimum"":0,
                    ""required"":true
                },
                ""tags"":{
                    ""type"":""array"",
                    ""items"":{
                    ""type"":""string""
                    }
                }
                }
            }";

            //Response<SchemaProperties> schemaProperties = client.RegisterSchema(groupName, name, draft4definition, format);
        }
    }
}
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

            var employee = new Employee { Age = 42, Name = "SampleEmployee" };

            var serializer = new SchemaRegistryJsonSerializerNJson(client, groupName, new SchemaRegistryJsonSerializerOptions { AutoRegisterSchemas = true });

            EventData eventData = (EventData)await serializer.SerializeAsync(employee, messageType: typeof(EventData));

            Employee deserialized = await serializer.DeserializeAsync<Employee>(eventData);
        }
    }
}
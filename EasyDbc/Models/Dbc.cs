using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DbcParserLib.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace EasyDbc.Models
{
    public class Dbc
    {
        public IEnumerable<Node> Nodes {get;}
        public IEnumerable<Message> Messages {get;}
        public IEnumerable<EnvironmentVariable> EnvironmentVariables { get; }
        public IEnumerable<CustomProperty> GlobalProperties { get; }

        public Dbc(IEnumerable<Node> nodes, IEnumerable<Message> messages, IEnumerable<EnvironmentVariable> environmentVariables,
            IEnumerable<CustomProperty> globalProperties)
        {
            Nodes = nodes;
            Messages = messages;
            EnvironmentVariables = environmentVariables;
            GlobalProperties = globalProperties;
        }
    }
}
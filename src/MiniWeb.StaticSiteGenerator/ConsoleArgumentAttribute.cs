namespace MiniWeb.StaticSiteGenerator
{
    internal class ConsoleArgumentAttribute : Attribute
    {
        public ConsoleArgumentAttribute(string prefix)
        {
            CommandPrefix = prefix;
        }

        public string CommandPrefix { get; }
    }
}

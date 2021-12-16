namespace MiniWeb.StaticSiteGenerator
{
    public class GeneratorConfig
    {

        [ConsoleArgument("-c")]
        public string ContentRoot { get; set; } = string.Empty;

        [ConsoleArgument("-o")]
        public string OutputFolder { get; set; } = string.Empty;

        [ConsoleArgument("--replace")]
        public bool ReplaceOutput { get; set; }

        public static GeneratorConfig ParseConfig(params string[] args)
        {
            var config = new GeneratorConfig();

            foreach (var prop in config.GetType().GetProperties())
            {
                foreach (var attr in prop.GetCustomAttributes(false))
                {
                    if (attr is ConsoleArgumentAttribute consoleArgument)
                    {
                        var index = Array.IndexOf(args, consoleArgument.CommandPrefix);
                        if (index == -1)
                        {
                            index = Array.IndexOf(args, $"-{prop.Name.ToLower()}");
                        }
                        if (index != -1)
                        {
                            if (prop.PropertyType == typeof(bool))
                            {
                                if (index + 1 < args.Length && !args[index + 1].StartsWith("-"))
                                    prop.SetValue(config, bool.Parse(args[index + 1]));
                                else
                                    prop.SetValue(config, true);
                            }
                            else if (index + 1 < args.Length && !args[index + 1].StartsWith("-"))
                            {
                                prop.SetValue(config, args[index + 1]);
                            }
                        }
                    }
                }
            }

            return config;
        }

    }


}

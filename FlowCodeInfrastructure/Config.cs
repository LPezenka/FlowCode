using System.Xml.Linq;

namespace FlowCodeInfrastructure
{
    public static class Config
    {
        public static void Load()
        {
            // Load configuration settings
            // This is a placeholder for actual configuration loading logic
            Console.WriteLine("Loading configuration...");
            if (File.Exists("./infrastructureconfig.xml"))
            {
                XDocument doc = XDocument.Load("./infrastructureconfig.xml");

                //string rootId = string.Empty;
                
                foreach (var c in doc.Root.Elements("KeyWord"))
                {
                    var k = c.Attribute("Key")?.Value;
                    var v = c.Attribute("Value")?.Value;
                    SetKeyWord(Enum.Parse<KeyWord>(k.ToString()), v);
                }

                var delay = doc.Root.Elements("Delay").FirstOrDefault();
                var ms = delay?.Attributes("miliseconds").FirstOrDefault().Value.ToString();
                if (ms is not null)
                    Delay = int.Parse(ms);
            }

        }

        public static void Save()
        {
            // Save configuration settings
            // This is a placeholder for actual configuration saving logic
            Console.WriteLine("Saving configuration...");
            XElement root = new XElement("FlowEditorConfig");

            XElement delay = new XElement("Delay");
            XAttribute d = new XAttribute("miliseconds", Delay);
            delay.Add(d);
            root.Add(delay);
            
            foreach (var keyword in keywordMapper)
            {
                XElement kw = new XElement("KeyWord");
                XAttribute k = new XAttribute("Key", keyword.Key);
                XAttribute v = new XAttribute("Value", keyword.Value);
                kw.Add(k);
                kw.Add(v);
                root.Add(kw);
            }

            root.Save("./infrastructureconfig.xml");
        }

        public static int Delay { get; set; }

        public enum KeyWord
        {
            Function,
            None,
            If,
            Else,
            While,
            For,
            Do,
            Switch,
            Case,
            Break,
            Continue,
            Return,
            True,
            False,
            Input,
            Output
        }

        private static Dictionary<KeyWord, string> keywordMapper = new();


        public static string GetKeyword(KeyWord key)
        {
            if (keywordMapper.ContainsKey(key))
            {
                return keywordMapper[key];
            }
            else
            {
                return key.ToString();
            }
        }

        public static void SetKeyWord(KeyWord key, string value)
        {
            if (!keywordMapper.TryAdd(key, value))
            {
                keywordMapper[key] = value;
            }
        }
    }
}

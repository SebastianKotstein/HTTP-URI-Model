using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace skotstein.net.http.urimodel.openapi.regextest
{
    class Program
    {
        public static void Main(string[] args)
        {

            string input1 = "hello";
            string input2 = "hello{id}";
            string input3 = "{id}hello{name}";
            string input4 = "name{id}:ass";
            string input5 = "name{id}:ass?={op}";
            string input6 = "name{id}:ass{op}as";

            Console.WriteLine(input1 + " --> " + CreateMatchRegex(input1).ToString());
            Console.WriteLine(input2 + " --> " + CreateMatchRegex(input2).ToString());
            Console.WriteLine(input3 + " --> " + CreateMatchRegex(input3).ToString());
            Console.WriteLine(input4 + " --> " + CreateMatchRegex(input4).ToString());
            Console.WriteLine(input5 + " --> " + CreateMatchRegex(input5).ToString());
            Console.WriteLine(input6 + " --> " + CreateMatchRegex(input6).ToString());
            Console.ReadLine();

            /*
            string test = "/a{id}b{id}cd{id2}";

            Regex regex = new Regex(@"(\{.*?\})");

            foreach(Match match in regex.Matches(test))
            {
                Console.WriteLine(match.Index+" "+match.Length);
            }


            int count = regex.Matches(test).Count;
            for (int i = 0; i < count; i++)
            {
                test = regex.Replace(test, "[" + i + "]", 1);
            }
            Console.WriteLine(test);
            Console.ReadLine();
            */
        }

        public static Regex CreateMatchRegex(string input)
        {
            Regex pathParameterRegex = new Regex(@"(\{.*?\})");

            //replace all path parameter placeholder with '$', e.g.: a{x}:{y}b --> a$:$b
            string modifiedValue = pathParameterRegex.Replace(input, "$");

            string[] staticElements = modifiedValue.Split('$');

            string pattern = "";
            for (int i = 0; i < staticElements.Length; i++)
            {
                

                if (i > 0)
                {
                    pattern += @"(.+)";
                }
                if (!String.IsNullOrWhiteSpace(staticElements[i]))
                {
                    //add a slash to special characters
                    string modifiedStaticElement = "";
                    foreach(char c in staticElements[i])
                    {
                        if (!char.IsLetterOrDigit(c))
                        {
                            modifiedStaticElement += @"\"+c;
                        }
                        else
                        {
                            modifiedStaticElement += c;
                        }
                    }

                    pattern += @"(?:(" + modifiedStaticElement.ToLower() + "))";
                }
            }
            return new Regex(pattern);
        }
    }
}

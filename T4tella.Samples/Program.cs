using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4tella.Samples
{
    class Program
    {
        private const string TEXT_TEMPLATE_PARTIAL_CLASS = @"
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(""T4tella"")]
namespace {0}
{{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

    public partial class {1}  
    {{
        public dynamic Data {{ get; set; }}
 
    }}
}}";

        static void Main(string[] args)
        {
            //File based
            var scriptellaFileIngredient =
                T4FileExecuter.ProcessTemplate(
                    "FileBasedT4Sample1.tt",
                    null,
                    null,
                    null,
                    (templatefilename, classname, ns) =>
                    {
                        return new T4FilePartialRecord
                        {
                            ToBeAddedClasses = new string[]
                            {
                                string.Format(TEXT_TEMPLATE_PARTIAL_CLASS, ns, classname)
                            }
                        };
                    },
                    (o) =>
                    {
                        ((dynamic) o).Data =  "TryTable" ;
                    }
                    );
            var generatedSQLs = ScriptellaRunner.ExecuteTemplate(scriptellaFileIngredient);
            Console.WriteLine(string.Join(",", generatedSQLs));

            //Runtime based
              scriptellaFileIngredient =(new RuntimeT4Sample1(){ Data=  "TryTable"}).TransformText() ;
            generatedSQLs = ScriptellaRunner.ExecuteTemplate(scriptellaFileIngredient);
            Console.WriteLine(string.Join(",", generatedSQLs));
        }
    }
}

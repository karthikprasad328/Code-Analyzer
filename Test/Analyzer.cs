using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
    class Analyzer
    {
        static public string[] getFiles(string path, List<string> patterns)
        {
            FileMgr fm = new FileMgr();
            foreach (string pattern in patterns)
                fm.addPattern(pattern);
            fm.findFiles(path);
            return fm.getFiles().ToArray();
        }

        static void doAnalysis(string[] files)
        {
            Console.Write("\n  Demonstrating Parser");
            Console.Write("\n ======================\n");

            foreach (object file in files)
            {
                string file1=file as string;
                if((file1.Contains("TemporaryGeneratedFile"))||(file1.Contains("AssemblyInfo")))
                    continue;
                Console.Write("\n  Processing file {0}\n", file1);

                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", file as string);
                    return;
                }

                Console.Write("\n  Type and Function Analysis");
                Console.Write("\n ----------------------------\n");

                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build();

                try
                {
                    while (semi.getSemi())
                        parser.parse(semi);
                    Console.Write("\n\n  locations table contains:");
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository rep = Repository.getInstance();
                List<Elem> table = rep.locations;
                foreach (Elem e in table)
                {
                    Console.Write("\n  {0,10}, {1,20}, {2,5}, {3,5}, {4,5}, {5,5}", e.type, e.name, e.begin, e.end, e.end - e.begin, e.scopecount);
                }
                Console.WriteLine();
                Console.Write("\n\n  That's all folks!\n\n");
                semi.close();
            }

            //  /// if you wanna print final repository
            //Console.WriteLine("\n the finalrepository after pass 1:\n");
            //Repository repo_ = Repository.getInstance();
            //List<Elem> table1 = repo_.locations2;
            //  foreach(Elem e in table1)
            //      Console.Write("\n  {0,10}, {1,20}, {2,5}, {3,5}, {4,5}, {5,5}", e.type, e.name, e.begin, e.end, e.end - e.begin, e.scopecount);




            ////Print user types if needed
            //Repository repo_=Repository.getInstance();
            //Console.WriteLine("\n User types defined in all files :");
            // foreach(Elem2 e in repo_.usertypes)
            //      Console.WriteLine("{0,10} {1,20}",e.type,e.name);


            //Pass2-detect relationships between the files
            Console.WriteLine("\n\n\n Processed all files- Time for Pass 2 ");
            Repository.pass = 2;
            foreach (object file in files)
            {
                Console.Write("\n  Processing file for relationship analysis- {0}\n", file as string);

                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();

                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", file as string);
                    return;
                }

                Console.Write("\n  Relationship  Analysis");
                Console.Write("\n ----------------------------\n");

                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build2();

                try
                {
                    while (semi.getSemi())
                        parser.parse(semi);

                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Console.Write("\n\n  Relationship table contains:\n");
                Repository rep = Repository.getInstance();
                List<Elem3> table3 = rep.relationships;
                foreach (Elem3 e in table3)
                {
                    Console.WriteLine("{0,10} {1,20} {2,10} {3,10} {4,20}", e.childtype, e.childname, e.relation, e.parenttype, e.parentname);
                }
            }
        }

        static void Main(string[] args)
        {
            string path = "../../";
            List<string> patterns = new List<string>();
            patterns.Add("*.cs");
            string[] files = Analyzer.getFiles(path, patterns);
            doAnalysis(files);
        
        }
    }
}

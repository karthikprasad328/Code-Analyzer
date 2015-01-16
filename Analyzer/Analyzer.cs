//////////////////////////////////////////////////////////////////////////////
// Analyzer.cs - Analyzer is used to dispatch individual files for parsing  //
// ver 1.1                                                                  //
// Language:    C#, 2013, .Net Framework 5.0                                //
// Platform:    Lenovo g550, Win8.1, SP1                                    //
// Application: Package used for solution to project 2 CSE-681              //
// Author:      Karthik Nanjundaswamy Guru Prasad(SUID:994344418)           //
//              Syracuse University                                         //
//Source Code:  Jim Fawcett, CST 4-187, Syracuse University                 //
//              (315) 443-3948, jfawcett@twcny.rr.com                       //
//////////////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ------------------
 * This module defines the following class:
 *   Analyzer  - defines two functions, one for fetching files from FileMgr, one for performing type and relationship analysis
 */
/* Required Files:
 *   parser.cs FileMgr.cs
 *   
 *   
 * Maintenance History:
 * --------------------
 * ver 1.1 : 27 Sep 2014
 * - Added functionality to call display package
 * - Added relationship analysis
 * ver 1.0 : 23 Sep 2014
 * - first release
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
    public class Analyzer
    {
        private static bool relationflag = false;//flag to perform pass 2
        public static void setRelationFlag(bool flag)
        {
            relationflag = flag;
        }
        public static bool checkRelationFlag()
        {
            return relationflag;
        }
        static public string[] getFiles(string path, List<string> patterns)
        {
            FileMgr fm = new FileMgr();
            foreach (string pattern in patterns)
                fm.addPattern(pattern);
            fm.findFiles(path);
            return fm.getFiles().ToArray();
        }

        public static void doAnalysis(string[] files)
        {
            Console.Write("\n  Demonstrating Parser");
            Console.Write("\n ======================\n");

            foreach (object file in files)
            {
                string file1 = file as string;
                Console.Write("\n  Processing file {0}\n", file1);
                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", file as string);
                    return;
                }
                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build();
                try
                {
                    while (semi.getSemi())
                        parser.parse(semi);
                    
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository rep = Repository.getInstance();
                List<Elem> table = rep.locations;
                Display disp = new Display();
                disp.displaytypes(table);//pass table to display
                semi.close();
            }
            Console.WriteLine("\n\n TYPE EXTRACTION DONE\n\n");
          
            //Pass2-detect relationships between the files
            if (Analyzer.checkRelationFlag())
            {
                Console.WriteLine("\n\n\n FINDING RELATIONSHIPS BETWEEN TYPES OF ALL FILES .............\n\n  ");
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
                    Repository rep = Repository.getInstance();
                    List<Elem3> table3 = rep.relationships;
                    Display disp = new Display();
                    //pass table to display
                    disp.displayrelations(table3);
                    }
            }
        }
#if(TEST_ANALYZER)
        static void Main(string[] args)
        {
            string path = "../../../Test";
            List<string> patterns = new List<string>();
            patterns.Add("*.cs");
            string[] files = Analyzer.getFiles(path, patterns);
            doAnalysis(files);
        
        }
#endif
    }
}


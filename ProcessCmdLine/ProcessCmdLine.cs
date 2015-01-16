///////////////////////////////////////////////////////////////////////////////
// ProcessCmdLine.cs - ProcessCmdLine processes commandline parameters       //
// ver 1.0                                                                   //
// Language:    C#, 2013, .Net Framework 5.0                                 //
// Platform:    Lenovo g550, Win8.1, SP1                                     //
// Application: Package used for solution to Project 2 CSE-681               //
// Author:      Karthik Nanjundaswamy Guru Prasad(SUID:994344418)            //
//              Syracuse University                                          //
///////////////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ------------------
 * This module defines the following class:
 *   ProcessCmdLine - Accepts parameters from Executive and processes them to find path, patterns and options
 */
/* Required Files:
 *   Analyzer.cs FileMgr.cs
 *      
 * Maintenance History:
 * --------------------
 * ver 1.0 : 07 Oct 2014
 * - first release
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CodeAnalysis
{
    public class ProcessCmdLine
    {
        public static void process(string[] args)
        {
            //List<string> files = new List<string>();
            List<string> patterns = new List<string>();
            bool patternflag=false; //flag to check if any patterns are sent as args. if no patterns then default value will be set
            
            string path = args[0];
            Console.WriteLine("\n\n Processing.......................................");
            for(int i=1;i<args.Length;i++)
            {
                if (args[i] == "\\S" || args[i] == "\\s")
                {
                    FileMgr.setRecurse(true);
                    Console.WriteLine("\n \\S option is specified. All subdirectories will be searched");
                }
                if (args[i] == "\\R" || args[i] == "\\r")
                {
                    Analyzer.setRelationFlag(true);
                    Console.WriteLine("\n \\R option is specified. Relationship Ananlysis will be done");
                }
                if(args[i].Contains("*"))//check if it is a pattern
                {
                    patterns.Add(args[i]);
                    patternflag = true;
                }
            }
            if (!patternflag)
            {
                Console.WriteLine("\n Input contains no parameters. Default parameter is \"*.cs\"");
                patterns.Add("*.cs");
            }
            string[] files = Analyzer.getFiles(path, patterns);
            Analyzer.doAnalysis(files);
        

        }


#if(TEST_PROCESS)
        static void Main(string[] args)
        {
            string[] arg = { "../../../Test", "*.*", "\\S","\\R" };//default parameters
            ProcessCmdLine.process(arg);
        }
#endif
    }

}

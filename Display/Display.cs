///////////////////////////////////////////////////////////////////////
// Display.cs - Displays types and relationships                     //
// ver 1.0                                                           //
// Language:    C#, 2013, .Net Framework 5.0                         //
// Platform:    Lenovo g550, Win8.1, SP1                             //
// Application: Package used for solution to Project-2 CSE-681       //
// Author:      Karthik Nanjundaswamy Guru Prasad(SUID:994344418)    //
//              Syracuse University                                  //
///////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ------------------
 * This module defines the following class:
 *   Display  - Defines two functions one to display types and one to display relationships
 *   
 * Note-this package does not have a test stub since it cannot execute without request from Analyzer.
 */
/* Required Files:
 *   parser.cs
 *   
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 05 Oct 2014
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
    public class Display
    {
        public void displaytypes(List<Elem> table) //displays the table containing the types
        {
            //Console.WriteLine("\n Printing from display package");
            Console.Write("\n  Type and Function Analysis");
            Console.Write("\n ----------------------------\n");

            Console.Write("\n\n  Types table contains:");
            Console.WriteLine("\n       TYPE                       NAME    SIZE");
            foreach (Elem e in table)
               {
                   Console.Write("\n  {0,10}, {1,25}, {2,5}", e.type, e.name,  e.end - e.begin);
               }
        }

        public void displayrelations(List<Elem3> table)
        {
            Console.Write("\n  Relationship  Analysis");
            Console.Write("\n ----------------------------\n");
            Console.Write("\n\n  Relationship table contains:\n");
                   
            Console.WriteLine("\n     TYPE               NAME      RELATION      TYPE                NAME");
            foreach (Elem3 e in table)
            {
                Console.WriteLine("{0,10} {1,20} {2,10} {3,10} {4,20}", e.childtype, e.childname, e.relation, e.parenttype, e.parentname);
            }
        }

        public void displaySummary()
        {
            Console.WriteLine("\n Analysis Summary");
            Console.Write("\n ----------------------------\n");
            int ns = 0, cl = 0, func = 0, str = 0, en = 0, inter = 0;
            Repository repo = Repository.getInstance();
            foreach (Elem e in repo.locations2)
            {
                //Console.Write("\n  {0,10}, {1,25}, {2,5}, {3,5}", e.type, e.name, e.end - e.begin,e.scopecount);
                if(e.type=="namespace")
                    ns++;
                if (e.type == "class")
                    cl++;
                if (e.type == "function")
                    func++;
                if (e.type == "struct")
                    str++;
                if (e.type == "enum")
                    en++;
                if (e.type == "interface")
                    inter++;
            }
            Console.WriteLine("\n {0} namespaces \n{1} classes \n{2} functions \n{3} structs \n{4} enums \n{5} interfaces\n",ns,cl,func,str,en,inter);
                       
         }

        
        static void Main(string[] args)
        {
        }
    }
}

///////////////////////////////////////////////////////////////////////////////
// Executive.cs - Executive accepts commandline arguments and processes them //
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
 *   Executive - Accepts parameters from commandline and calls the ProcessCmdLine function.
 */
/* Required Files:
 *   Parser.cs FileMgr.cs
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
    class Executive
    {
        static void Main(string[] args)
        {
                                   
            string[] arg={ "../../../Test", "*.*","\\S","\\r"};//these will become default argument if no arguments are specified

            if (args.Length == 0)
            {
                ProcessCmdLine.process(arg);
            }
            else
            {
                ProcessCmdLine.process(args);
            }
            //display the summary of files
            Display disp = new Display();
            
            disp.displaySummary();

        }
    }
}

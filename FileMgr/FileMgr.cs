///////////////////////////////////////////////////////////////////////
// FileMgr.cs - FileMgr is used to fetch all files from a fiven path //
// ver 1.0                                                           //
// Language:    C#, 2013, .Net Framework 5.0                         //
// Platform:    Lenovo g550, Win8.1, SP1                             //
// Application: Package used for solution to Project-2,CSE-681       //
// Author:      Karthik Nanjundaswamy Guru Prasad(SUID:994344418)    //
//              Syracuse University                                  //
//Source Code:  Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ------------------
 * This module defines the following class:
 *   FileMgr  - defines three functions, one to add patterns to search, one to find all files in a path 
 *              matching particular patterns, one to fetch these files.
 */
/*   
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
using System.IO;

namespace CodeAnalysis
{
    public class FileMgr
    {
        private List<string> files = new List<string>();
        private List<string> patterns = new List<string>();
        private static bool recurse = false;

        public static void setRecurse(bool flag)
        {
            recurse=flag;
        }
        public void findFiles(string path)
        {
            if (patterns.Count == 0)
                addPattern("*.*");
            foreach (string pattern in patterns)
            {
                string[] newFiles = Directory.GetFiles(path, pattern);
                List<string> searchFiles = new List<string>(); ;
                
                for (int i = 0; i < newFiles.Length; ++i)
                {
                    newFiles[i] = Path.GetFullPath(newFiles[i]);
                    if (Path.GetExtension(newFiles[i]) != ".cs")
                    {
                        Console.WriteLine("\n {0} - cannot process because it is not a *.cs file", newFiles[i]);
                        continue;
                    }
                    try
                    {
                        searchFiles.Add( newFiles[i]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\n inside findfiles-{0}", ex.Message);
                    }
                 }
                try
                {
                  
                        files.AddRange(searchFiles);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n inside findfiles-{0}", ex.Message);
                }
                
            }
            if (recurse)
            {
                string[] dirs = Directory.GetDirectories(path);
                foreach (string dir in dirs)
                    findFiles(dir);
            }
        }

        public void addPattern(string pattern)
        {
            patterns.Add(pattern);
        }

        public List<string> getFiles()
        {
            return files;
        }

#if(TEST_FILEMGR)
        static void Main(string[] args)
        {
            Console.Write("\n  Testing FileMgr Class");
            Console.Write("\n =======================\n");

            FileMgr fm = new FileMgr();
            fm.addPattern("*.cs");
            fm.findFiles("../../../Test");
            List<string> files = fm.getFiles();
            foreach (string file in files)
                Console.Write("\n  {0}", file);
            Console.Write("\n\n");
            
        }
#endif
    }
}

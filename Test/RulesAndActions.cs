///////////////////////////////////////////////////////////////////////
// RulesAndActions.cs - Parser rules specific to an application      //
// ver 2.1                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Platform:    Dell Precision T7400, Win7, SP1                      //
// Application: Demonstration for CSE681, Project #2, Fall 2011      //
// Author:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * RulesAndActions package contains all of the Application specific
 * code required for most analysis tools.
 *
 * It defines the following Four rules which each have a
 * grammar construct detector and also a collection of IActions:
 *   - DetectNameSpace rule
 *   - DetectClass rule
 *   - DetectFunction rule
 *   - DetectScopeChange
 *   
 *   Three actions - some are specific to a parent rule:
 *   - Print
 *   - PrintFunction
 *   - PrintScope
 * 
 * The package also defines a Repository class for passing data between
 * actions and uses the services of a ScopeStack, defined in a package
 * of that name.
 *
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *  
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
 *   Semi.cs, Toker.cs
 *   
 * Build command:
 *   csc /D:TEST_PARSER Parser.cs IRuleAndAction.cs RulesAndActions.cs \
 *                      ScopeStack.cs Semi.cs Toker.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 2.2 : 24 Sep 2011
 * - modified Semi package to extract compile directives (statements with #)
 *   as semiExpressions
 * - strengthened and simplified DetectFunction
 * - the previous changes fixed a bug, reported by Yu-Chi Jen, resulting in
 * - failure to properly handle a couple of special cases in DetectFunction
 * - fixed bug in PopStack, reported by Weimin Huang, that resulted in
 *   overloaded functions all being reported as ending on the same line
 * - fixed bug in isSpecialToken, in the DetectFunction class, found and
 *   solved by Zuowei Yuan, by adding "using" to the special tokens list.
 * - There is a remaining bug in Toker caused by using the @ just before
 *   quotes to allow using \ as characters so they are not interpreted as
 *   escape sequences.  You will have to avoid using this construct, e.g.,
 *   use "\\xyz" instead of @"\xyz".  Too many changes and subsequent testing
 *   are required to fix this immediately.
 * ver 2.1 : 13 Sep 2011
 * - made BuildCodeAnalyzer a public class
 * ver 2.0 : 05 Sep 2011
 * - removed old stack and added scope stack
 * - added Repository class that allows actions to save and 
 *   retrieve application specific data
 * - added rules and actions specific to Project #2, Fall 2010
 * ver 1.1 : 05 Sep 11
 * - added Repository and references to ScopeStack
 * - revised actions
 * - thought about added folding rules
 * ver 1.0 : 28 Aug 2011
 * - first release
 *
 * Planned Modifications (not needed for Project #2):
 * --------------------------------------------------
 * - add folding rules:
 *   - CSemiExp returns for(int i=0; i<len; ++i) { as three semi-expressions, e.g.:
 *       for(int i=0;
 *       i<len;
 *       ++i) {
 *     The first folding rule folds these three semi-expression into one,
 *     passed to parser. 
 *   - CToker returns operator[]( as four distinct tokens, e.g.: operator, [, ], (.
 *     The second folding rule coalesces the first three into one token so we get:
 *     operator[], ( 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CodeAnalysis
{
    public class Elem  // holds scope information
    {
        public string type { get; set; }
        public string name { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
        public int scopecount = 0;
        public bool flag = false;



        public override string ToString()
        {
            StringBuilder temp = new StringBuilder();
            temp.Append("{");
            temp.Append(String.Format("{0,-10}", type)).Append(" : ");
            temp.Append(String.Format("{0,-10}", name)).Append(" : ");
            temp.Append(String.Format("{0,-5}", begin.ToString()));  // line of scope start
            temp.Append(String.Format("{0,-5}", end.ToString()));    // line of scope end
            temp.Append("}");
            return temp.ToString();
        }
    }
    public class Elem2
    {
        public string type;
        public string name;
    }
    public class Elem3
    {
        public string childtype;
        public string childname;
        public string parentname;
        public string parenttype;
        public string relation;
    }

    public class Repository
    {
        ScopeStack<Elem> stack_ = new ScopeStack<Elem>();
        public List<Elem> locations_ = new List<Elem>();
        public List<Elem> locations2 = new List<Elem>();
        public List<Elem3> relationships = new List<Elem3>();
        public List<Elem3> relationships2 = new List<Elem3>();
        public List<Elem2> usertypes = new List<Elem2>();
        static Repository instance;
        public static int pass = 1;


        public Repository()
        {
            instance = this;
        }

        public static Repository getInstance()
        {
            return instance;
        }
        // provides all actions access to current semiExp

        public CSsemi.CSemiExp semi
        {
            get;
            set;
        }

        // semi gets line count from toker who counts lines
        // while reading from its source

        public int lineCount  // saved by newline rule's action
        {
            get { return semi.lineCount; }
        }
        public int prevLineCount  // not used in this demo
        {
            get;
            set;
        }
        // enables recursively tracking entry and exit from scopes

        public ScopeStack<Elem> stack  // pushed and popped by scope rule's action
        {
            get { return stack_; }
        }
        // the locations table is the result returned by parser's actions
        // in this demo

        public List<Elem> locations
        {
            get { return locations_; }
        }
    }
    /////////////////////////////////////////////////////////
    // pushes scope info on stack when entering new scope

    public class PushStack : AAction
    {
        Repository repo_;

        public PushStack(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Elem elem = new Elem();
            Elem2 element = new Elem2();
            elem.type = semi[0];  // expects type
            elem.name = semi[1];  // expects name
            elem.begin = repo_.semi.lineCount - 1;
            elem.end = 0;

            repo_.stack.push(elem);
           
            if (elem.type == "control" || elem.name == "anonymous")
            {

                return;
            }
            if (elem.type == "class" || elem.type == "struct" || elem.type == "enum")
            {
                element.name = elem.name;
                element.type = elem.type;
                repo_.usertypes.Add(element);

            }
            if (elem.type != "struct")
            {
                repo_.locations.Add(elem);

                if (Repository.pass != 2)
                    repo_.locations2.Add(elem);
            }
        }
    }
    /////////////////////////////////////////////////////////
    // pops scope info from stack when leaving scope

    public class PopStack : AAction
    {
        Repository repo_;

        public PopStack(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Elem elem;
            try
            {
                elem = repo_.stack.pop();
                
                for (int i = 0; i < repo_.locations.Count; ++i)
                {
                    Elem temp = repo_.locations[i];
                    if (elem.type == temp.type)
                    {
                        if (elem.name == temp.name)
                        {
                            if ((repo_.locations[i]).end == 0)
                            {
                                (repo_.locations[i]).end = repo_.semi.lineCount;
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                Console.Write("popped empty stack on semiExp: ");
                semi.display();
                return;
            }
            CSsemi.CSemiExp local = new CSsemi.CSemiExp();
            local.Add(elem.type).Add(elem.name);
            if (local[0] == "control")
                return;

        }
    }

    //This is action to store inheritance relationships
    public class FoundInheritance : AAction
    {

        Repository repo_;
        public FoundInheritance(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            string typesemi = null;
            bool flag = false;

            foreach (Elem e in repo_.locations2)
            {
                if (semi[1] == e.name && (e.type != "function"))
                {
                    flag = true;
                    typesemi = e.type;
                }
            }

            if (flag)
            {
                Elem3 element = new Elem3();
                element.parenttype = typesemi;
                element.parentname = semi[1];
                element.childname = semi[0];
                element.childtype = semi[2];
                element.relation = "inherits";
                //Console.WriteLine("{0,10} {1,20} {2,10} {3,10} {4,20}", element.childtype, element.childname, element.relation, element.parenttype, element.parentname);
                repo_.relationships.Add(element);
                repo_.relationships2.Add(element);

            }
        }
    }
    //This is action to store aggregation relationships
    public class FoundAggregation : AAction
    {

        Repository repo_;
        public FoundAggregation(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            string typesemi = null;
            bool flag = false;

            foreach (Elem e in repo_.locations2)
            {
                if (semi[0] == e.name && (e.type != "function"))
                {
                    flag = true;
                    typesemi = e.type;

                }
            }
            if (flag)
            {
                for (int i = repo_.locations.Count - 1; i >= 0; i--)
                    if (repo_.locations[i].type == "class")
                    {
                        Elem3 element = new Elem3();
                        element.parenttype = typesemi;
                        element.parentname = semi[0];
                        element.childname = repo_.locations[i].name;
                        element.childtype = repo_.locations[i].type;
                        element.relation = "aggregates";

                        bool present = repo_.relationships2.Exists(x => x.childname == element.childname && x.childtype == element.childtype && x.parentname == element.parentname && x.parenttype == element.parenttype && x.relation == element.relation);
                        if (!present)
                        {
                            repo_.relationships.Add(element);
                            repo_.relationships2.Add(element);
                        }
                        break;

                    }

            }

        }
    }

    ///this  stores composes relationships
    public class FoundComposition : AAction
    {

        Repository repo_;
        public FoundComposition(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            for (int i = repo_.locations.Count - 1; i >= 0; i--)
                if (repo_.locations[i].type == "class")
                {
                    Elem3 element = new Elem3();
                    element.parenttype = semi[1];
                    element.parentname = semi[0]; 
                    element.childname = repo_.locations[i].name;
                    element.childtype =  "class";
                    element.relation = "composes";

                    bool present = repo_.relationships2.Exists(x => x.childname == element.childname && x.childtype == element.childtype && x.parentname == element.parentname && x.parenttype == element.parenttype && x.relation == element.relation);
                    if (!present)
                    {
                        repo_.relationships.Add(element);
                        repo_.relationships2.Add(element);
                    }
                    break;
                }

        }
    }

    ////this stores Using relationships
     public class FoundUsing : AAction
    {
         
        Repository repo_;
        public FoundUsing(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            string namesemi = null;//stores name of class which is Using
            for (int i = repo_.locations.Count - 1; i >= 0; i--)
                if (repo_.locations[i].type == "class")
                {
                    namesemi = repo_.locations[i].name;
                    break;
                }

            for(int i=0;i<semi.count;i++)
                foreach(Elem e in repo_.locations2)
                    if(e.name==semi[i]&&e.type!="namespace"&&e.type!="function")
                    {
                        
                        Elem3 element = new Elem3();
                        element.parenttype = e.type;
                        element.parentname = e.name;
                        element.childname = namesemi;
                        element.childtype = "class";
                        element.relation = "uses";

                        bool present = repo_.relationships2.Exists(x => x.childname == element.childname && x.childtype == element.childtype && x.parentname == element.parentname && x.parenttype == element.parenttype && x.relation == element.relation);
                        if (!present)
                        {
                            repo_.relationships.Add(element);
                            repo_.relationships2.Add(element);
                        }
                        
                

                    }
   
        }
     }
    /////////////////////////////////////////////////////////
    // rule to detect namespace declarations

    public class DetectNamespace : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains("namespace");
            if (index != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // create local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add(semi[index]).Add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    /////////////////////////////////////////////////////////
    // rule to dectect class definitions

    public class DetectClass : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int indexCL = semi.Contains("class");
            int indexIF = semi.Contains("interface");
            int indexST = semi.Contains("struct");
            int indexEn = semi.Contains("enum");
            int indexDEL = semi.Contains("delegate");

            int index = Math.Max(indexCL, indexIF);
            index = Math.Max(index, indexST);
            index = Math.Max(index, indexEn);
            index = Math.Max(index, indexDEL);

            if (index != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // local semiExp with tokens for type and name
                local.displayNewLines = false;
                string str = null;
                if (indexDEL > 0)
                    str = semi[index + 2];
                else str = semi[index + 1];
                local.Add(semi[index]).Add(str);
                //local.Add(semi[index].Add(semi[index+1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    /////////////////////////////////////////////////////////
    // rule to dectect function definitions

    public class DetectFunction : ARule
    {
        public static bool isSpecialToken(string token)
        {
            string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
            foreach (string stoken in SpecialToken)
                if (stoken == token)
                    return true;
            return false;
        }
        public override bool test(CSsemi.CSemiExp semi)
        {
            if (semi[semi.count - 1] != "{")
                return false;


            int index = semi.FindFirst("(");
            if (index > 0 && !isSpecialToken(semi[index - 1]))
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                local.Add("function").Add(semi[index - 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }

    //////////////////////////////////////
    // detect entering anonymous scope
    // - expects namespace, class, and function scopes
    //   already handled, so put this rule after those
    public class DetectAnonymousScope : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains("{");

            if (index != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // create local semiExp with tokens for type and name
                local.displayNewLines = false;
                
                local.Add("control").Add("anonymous");
                doActions(local);
                return true;
            }
            return false;
        }
    }
    /////////////////////////////////////////////////////////
    // detect leaving scope

    public class DetectLeavingScope : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains("}");
            if (index != -1)
            {
                doActions(semi);
                return true;
            }
            return false;
        }
    }


    //////////////////////////////////////////////////////////////
    //This finds inheritance among the classes in Repository
    public class DetectInheritance : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.FindFirst(":");
            int index1 = semi.Contains("class");
            int index2 = semi.Contains("interface");

            if (index > 0 && (index1 > 0 || index2 > 0))
            {
                //Console.WriteLine("\n karthik {2} {0} inherits {1}",semi[index-1],semi[index+1],semi[index-2]);
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                local.Add(semi[index - 1]).Add(semi[index + 1]).Add(semi[index - 2]);
                doActions(local);

            }
            return false;
        }
    }

    public class DetectAggregation : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.FindFirst("new");
            int index2 = semi.Contains("<");
            int index3 = semi.Contains(".");
            int index4 = semi.Contains("StringBuilder");
            int index5 = semi.Contains("StreamReader");

            string str = null;

            if (index > 0 && index2 < 0 && index4 < 0 && index5 < 0)
            {

                if (index3 > 0)
                {
                    str = semi[index3 + 1];
                }
                else
                {
                    str = semi[index + 1];
                }
                //Console.WriteLine("/n Karthik {0}\n",str);

                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                local.Add(str);
                doActions(local);
                return true;
            }
            return false;
        }

        }



        /// <summary>
        /// Detects a composition
        /// </summary>
        public class DetectComposition : ARule
        {
            public override bool test(CSsemi.CSemiExp semi)
            {
                Repository repo = Repository.getInstance();
                for (int i = 0; i < semi.count - 1; i++)
                {
                    foreach (Elem2 e in repo.usertypes)
                    {
                        if (e.type == "struct" || e.type == "enum")
                            if (e.name == semi[i])
                            {
                                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                                local.Add(semi[i]).Add(e.type);
                                doActions(local);
                                return true;

                            }

                    }
                }
                return false;

            }
        }

    /// <summary>
    /// Detects a Using relationship
    /// </summary>
        public class DetectUsing : ARule
        {
            public static bool isSpecialToken(string token)
            {
                string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using","throw" };
                foreach (string stoken in SpecialToken)
                    if (stoken == token)
                        return true;
                return false;
            }
            public override bool test(CSsemi.CSemiExp semi)
            {
                if (semi[semi.count - 1] != "{")
                    return false;


                int index = semi.FindFirst("(");
                if (index > 0 && !isSpecialToken(semi[index - 1]))
                {
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    int last=semi.Contains(")");
                    for (int i = index + 1; i <last ; i++)
                    {
                       //Console.WriteLine("\n Karthik {0}",semi[i-1]);
                        local.Add(semi[i]);
                    }
                    doActions(local);
                    return true;
                }
                return false;
            }
        }


            public class BuildCodeAnalyzer
            {
                static Repository repo = new Repository();

                public BuildCodeAnalyzer(CSsemi.CSemiExp semi)
                {
                    repo.semi = semi;
                }
                public virtual Parser build()
                {
                    Parser parser = new Parser();
                    repo.locations_ = new List<Elem>();

                    // decide what to show
                    AAction.displaySemi = true;
                    AAction.displayStack = false;  // this is default so redundant

                    // action used for namespaces, classes, and functions
                    PushStack push = new PushStack(repo);

                    // capture namespace info
                    DetectNamespace detectNS = new DetectNamespace();
                    detectNS.add(push);
                    parser.add(detectNS);

                    // capture class info
                    DetectClass detectCl = new DetectClass();
                    detectCl.add(push);
                    parser.add(detectCl);

                    // capture function info
                    DetectFunction detectFN = new DetectFunction();
                    detectFN.add(push);
                    parser.add(detectFN);

                    // handle entering anonymous scopes, e.g., if, while, etc.
                    DetectAnonymousScope anon = new DetectAnonymousScope();
                    anon.add(push);
                    parser.add(anon);

                    // handle leaving scopes
                    DetectLeavingScope leave = new DetectLeavingScope();
                    PopStack pop = new PopStack(repo);
                    leave.add(pop);
                    parser.add(leave);


                    // parser configured
                    return parser;
                }
                public virtual Parser build2()
                {
                    Parser parser = new Parser();

                    repo.relationships = new List<Elem3>();
                    PushStack push = new PushStack(repo);

                    //capture inheritance info
                    DetectInheritance detectIN = new DetectInheritance();
                    FoundInheritance p1 = new FoundInheritance(repo);
                    detectIN.add(p1);
                    parser.add(detectIN);

                    // capture class info
                    DetectClass detectCl = new DetectClass();
                    detectCl.add(push);
                    parser.add(detectCl);

                    //capture aggregation info
                    DetectAggregation detectAG = new DetectAggregation();
                    FoundAggregation p2 = new FoundAggregation(repo);
                    detectAG.add(p2);
                    parser.add(detectAG);

                    //capture composition info
                    DetectComposition detectCMP = new DetectComposition();
                    FoundComposition p3 = new FoundComposition(repo);
                    detectCMP.add(p3);
                    parser.add(detectCMP);

                    // capture "using" relationship
                    DetectUsing detectUS = new DetectUsing();
                    FoundUsing p4 = new FoundUsing(repo);
                    detectUS.add(p4);
                    parser.add(detectUS);


                    //return conigured parser object
                    return parser;




                }
            }
        }
    





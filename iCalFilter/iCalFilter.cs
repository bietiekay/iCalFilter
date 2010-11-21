using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace iCalFilter
{
    class iCalFilter
    {
        static void Main(string[] args)
        {
            List<String> Categories = new List<string>();
            Boolean Exclude = false;
            Boolean RemoveDescription = false;

            Console.WriteLine("iCalFilter Tool 0.1");
            Console.WriteLine("(C) Daniel Kirstenpfad 2009-2010 - http://www.technology-ninja.com");
            Console.WriteLine("    Released under AGPLv3 License - see license.txt for details.");
            Console.WriteLine();

            #region Parameter Check
            
            if (args.Length < 4)
            {
                Console.WriteLine("Small tool which filters iCalendar files by categories and outputs a new iCalendar file.");
                Console.WriteLine();
                Console.WriteLine("Syntax:");
                Console.WriteLine("icalfilter <input-ical-file> <output-ical-file> <exclude / include> <category-name> [-remove-description] [more-categories-to-include-separated-by-comma]");
                Console.WriteLine();
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("The file "+args[0]+" does not exists! Please specify an existing Input File");
                return;
            }

            #region fill categories list
            if (args[2].ToUpper()[0] == 'E')
            {
                Exclude = true;
                Console.WriteLine("Excluding Categories: ");
            }
            else
            {
                Exclude = false;
                Console.WriteLine("Including Categories: ");
            }

            for (int i = 3; i <= args.Length - 1; i++)
            {
                Categories.Add(args[i].ToUpper());

                if (args[i].ToUpper() == "-REMOVE-DESCRIPTION")
                {
                    RemoveDescription = true;
                }
                else
                    Console.WriteLine("                      "+args[i]);
            }

            #endregion

            #endregion

            String[] iCalInputFile = File.ReadAllLines(args[0]);

            // open output
            TextWriter iCalOutputWriter = new StringWriter();
            TextWriter iCalOutputFile = new StreamWriter(args[1]);

            int CurrentPosition = 0;

            while (CurrentPosition <= iCalInputFile.Length - 1)
            {
                // split everything...
                String[] splitted = iCalInputFile[CurrentPosition].Split(new char[] { ':' });
                String[] splitted2;

                // found a begin split...
                if (splitted[0] == "BEGIN")
                {
                    List<String> CompleteElement = new List<string>();
                    CompleteElement.Add(iCalInputFile[CurrentPosition]);
                    String CategoriesLine = "";

                    switch (splitted[1])
                    {
                        case "VEVENT":
                            CurrentPosition++;

                            while (true)
                            {
                                CompleteElement.Add(iCalInputFile[CurrentPosition]);
                                splitted = iCalInputFile[CurrentPosition].Split(new char[] { ':' });
                                splitted2 = iCalInputFile[CurrentPosition].Split(new char[] { ';' });

                                if (splitted[0] == "CATEGORIES")
                                    CategoriesLine = iCalInputFile[CurrentPosition];

                                CurrentPosition++;

                                if (RemoveDescription)
                                {
                                    if ( (splitted[0] == "DESCRIPTION") | (splitted2[0] == "X-ALT-DESC") )
                                    {
                                        CompleteElement.RemoveAt(CompleteElement.Count-1);
                                        while (true)
                                        {
                                            if (iCalInputFile[CurrentPosition][0] == '\t')
                                                CurrentPosition++;
                                            else
                                                break;
                                        }
                                    }
                                }

                                if ((splitted[0] == "END") && (splitted[1] == "VEVENT"))
                                    break;
                            }
                            break;
                        case "VALARM":
                            CurrentPosition++;
                            while (true)
                            {
                                CompleteElement.Add(iCalInputFile[CurrentPosition]);
                                splitted = iCalInputFile[CurrentPosition].Split(new char[] { ':' });

                                if (splitted[0] == "CATEGORIES")
                                    CategoriesLine = iCalInputFile[CurrentPosition];

                                CurrentPosition++;

                                if ((splitted[0] == "END") && (splitted[1] == "VALARM"))
                                    break;
                            }

                            break;
                        default:
                            iCalOutputFile.WriteLine(iCalInputFile[CurrentPosition]);
                            CurrentPosition++;
                            break;
                    }

                    if (CompleteElement.Count > 0)
                    {
                        bool takethisElement = false;
                        // found something...now output it...
                        if (Exclude)
                        {
                            takethisElement = true;
                            // if we should exclude
                            foreach (String Category in Categories)
                            {
                                if (CategoriesLine.ToUpper().Contains(Category))
                                    takethisElement = false;
                            }
                        }
                        else
                        {
                            // if we should include
                            foreach (String Category in Categories)
                            {
                                if (CategoriesLine.ToUpper().Contains(Category))
                                    takethisElement = true;
                            }
                        }

                        if (takethisElement)
                        {
                            foreach (String line in CompleteElement)
                            {
                                iCalOutputFile.WriteLine(line);
                            }
                        }
                    }
                }
                else
                {
                    iCalOutputFile.WriteLine(iCalInputFile[CurrentPosition]);
                    CurrentPosition++;
                }
            }

            iCalOutputFile.Close();
        }
    }
}

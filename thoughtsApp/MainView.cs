﻿using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace thoughtsApp
{
    public static class MainView
    {
        public static (int option, bool loop) FolderListLoop() 
        {
            (int option, bool loop) choice = (0, true);
            do
            {
                Console.Clear();
                Console.WriteLine("CHOOSE FOLDER.\n\n");
                Console.WriteLine("1. Journal.");
                Console.WriteLine("2. Thoughts.");
                Console.WriteLine("3. Rules.");
                choice = MainViewLogic.OptionsLoopCondition(3);
                if (choice.loop == true)
                    break;
            }
            while (choice.option == 0); 

            return choice;
        }
        public static void MainWindowOptionListLoop(string folderId)
        {
            (int option, bool loop) choice = (0, true);
            do
            {
                Console.Clear();
                Console.WriteLine("1. Upload your thought.");
                Console.WriteLine("2. View thoughts explorer.");
                Console.WriteLine("3. View random thought.");
                Console.WriteLine("4. Perform operations on notes.");
                choice = MainViewLogic.OptionsLoopCondition(4);
                if (choice.option != 0)
                {

                    switch (choice.option)
                    {
                        case 1:
                            ThoughtLoop(folderId);
                            break;
                        case 2:
                            DriveExplorer(folderId);
                            break;
                        case 3:
                            RandomFileViewer(folderId);
                            break;
                        case 4:
                            OperateOnNotes(folderId);
                            break;
                        default: return;
                    }
                }
            }
            while (!choice.loop); 
        }
        public static void ThoughtLoop(string folderId)
        {
            do
            {
                Console.Clear();
                Console.WriteLine("Welcome to thought uploader!\n");
                Console.WriteLine("----------------------------");
                Console.WriteLine("1. Type X to go back to menu.");
                Console.WriteLine("2. Type XD to leave program");
                Console.WriteLine("----------------------------\n");
                Console.WriteLine("Share your thought below: \n\n\n");
            }
            while (MainViewLogic.ThoughtLoopCondition(folderId).Result);
        }
        public static void DriveExplorer(string folderId)
        {
            var list = new List<(string name, string id)>();
            do
            {
                list = FileManager.GetNotesInfoFromDrive(folderId);
                var length = list.Count();
                Console.Clear();
                Console.WriteLine("List of current notes.");
                Console.WriteLine("----------------------");
                for (int i = 0; i < length; i++)
                {
                    Console.WriteLine($"{i + 1} {list[i].name}");
                }
            }
            while (MainViewLogic.ThoughtListLoop(folderId, list));
        }
        public static void FilesLookup((string name, string id) fileInfo, string text)
        {
            Console.WriteLine($"Notatka: {fileInfo.name}");
            Console.WriteLine(text);
        }
        public static void RandomFileViewer(string folderId)
        {
            var list = FileManager.GetNotesInfoFromDrive(folderId);
            int note = new Random().Next(0, list.Count - 1);
            (bool loop, int note) continuation = (true, note);
            do
            {
                Console.Clear();
                Console.WriteLine("| X- Go Back | XD - Leave Program |\n| Next - Next Note |\n");
                var text = FileManager.GetFileText(list[continuation.note].id);
                FilesLookup(list[continuation.note], text);
                continuation = MainViewLogic.RandomThoughtLoop(continuation.note, list.Count - 1);
            }
            while (continuation.loop);
        }
        public static void FileViewer(List<(string name, string id)> list, int note)
        {
            (bool loop, int note, bool edit) continuation = (true, note, false);
            do
            {
                Console.Clear();
                if (continuation.edit)
                    Console.WriteLine("Add text to your current note.");
                else
                    Console.WriteLine("| X- Go Back | XD - Leave Program |\n| Next - Next Note | Prev - Previous Note |\n| Update - Update Note | Delete - Delete Note");

                var text = FileManager.GetFileText(list[continuation.note].id);
                FilesLookup(list[continuation.note], text);

                continuation =  MainViewLogic.FileViewerLoop(continuation.note, list.Count - 1, continuation.edit, (text,list[continuation.note].id));
            }
            while (continuation.loop);
        }
        public static void OperateOnNotes(string folderId)
        {
            bool isDownloading = true;
            FileManager.DownloadCompleted += () => isDownloading = false;
            (int option, bool exit) info = (0,false);
            do
            {
                Console.Clear();
                Console.WriteLine("1. Update downloaded data");
                Console.WriteLine("2. List sentences with given expression.");
                Console.WriteLine("3. Some other option.");
                Console.WriteLine("4. Some other option.");
                info = MainViewLogic.OptionsLoopCondition(4);

                if (info.option!=0)
                {
                    //If Exitconditions is true, option == 0, default case is activated and you go back
                    switch (info.option)
                    {
                        case 1:
                            Console.Clear();
                            Task downloadTask = Task.Run(() => FileManager.DownloadAllNotesJson(folderId));
                            while (isDownloading)
                                WaitingAnimation("Downloading");
                            break;
                        case 2:
                            ViewExpressionSentences();
                            break;
                        case 3:
                            Console.WriteLine("Trzeci");
                            Console.ReadLine();
                            break;
                        case 4:
                            Console.WriteLine("Default");
                            Console.ReadLine();
                            break;

                        default:

                            break;
                    }
                }
            }
            while (!info.exit);
        }
        public static void ViewExpressionSentences()
        {
            (int option, bool exit) info = (0, false);
            do
            {
                Console.Clear();
                Console.WriteLine("Type in expression: ");
                string expression = Console.ReadLine();
                if (Verifiers.ExitConditions(expression)) // it makes sense to do it here trust me
                    break;

                var expressionSentences = MainViewLogic.GetExpressionSentences(expression);
                if (expressionSentences.Count > 0)
                    ViewExpressionSentencesChoice(expressionSentences,expression);

            }
            while (true); // I know, hurts my feelings as well

        }
        public static void ViewExpressionSentencesChoice(List<(string name, List<string> sentence)> expressionSentences,string expression) 
        {
            (int option, bool exit) info = (0, false);
            do
            {
                ReadDictSentences(expressionSentences,expression);
                Console.WriteLine("\n\n\nLookup given note:");
                info = MainViewLogic.OptionsLoopCondition(expressionSentences.Count);

                if (info.exit)
                    break;
                else if(info.option!=0)
                    ReadNoteBySentence(expressionSentences[info.option - 1].name,expression);
                Console.Clear();

            }
            while (true);
        }
        public static void ReadNoteBySentence(string name, string expression)
        {
            Console.Clear();
            JObject notatki = FileManager.GetJsonObject(FileConfig.combinedNotes);
            JArray array = (JArray)notatki["data"];
            var properNote = array.Where(x => x["name"].Value<string>()==name).FirstOrDefault();
            string text = properNote["text"].Value<string>();


            ReadSentenceWithColoredExpression(text, expression);
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
            Console.Clear();
        }
        public static void ReadDictSentences(List<(string name, List<string> sentence)> notes, string expression )
        {
            int noteNumber = 0;
            if (notes.Count > 0)
            {
                foreach (var note in notes)
                {
                    noteNumber++;
                    Console.WriteLine("\n----------------------------");
                    Console.WriteLine(noteNumber+ ". " + note.name);
                    Console.WriteLine("----------------------------\n");
                    foreach (var line in note.sentence)
                    {
                        ReadSentenceWithColoredExpression(line+".", expression);
                    }
                }
            }
            else
            {
                Console.WriteLine("Phrase wasn't found.");
            }
        }
        public static void ReadSentenceWithColoredExpression(string sentence, string expression) 
        {
            int startIndex = 0;
            while (startIndex < sentence.Length)
            {
                int wordIndex = sentence.IndexOf(expression, startIndex);
                if (wordIndex != -1)
                {
                    Console.Write(sentence.Substring(startIndex, wordIndex - startIndex));
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(expression);
                    Console.ResetColor();
                    startIndex = wordIndex + expression.Length;
                }
                else
                {
                    Console.WriteLine(sentence.Substring(startIndex));
                    break;
                }
            }
        
        
        }
        public static void WaitingAnimation(string text) 
        {
            Console.Clear();
            for (int i = 1; i < 3; i++)
            {
                Console.Write(text.ToUpper());
                for (int j = 0; j <= i; j++)
                {
                    Console.Write(".");
                    Thread.Sleep(500);
                }
                Console.Clear();
            };
        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace ReMime.Cli
{
    public static class Program
    {
        private const string USAGE = "refile [-r] file/directory/-...\n" +
                                     "refile --help for more help.";

        private const string HELP =
            "ReMime Command Line Tool - Determine file Media Type\n" +
            "\n" +
            "    refile [-r] file/directory/-...\n" +
            "\n" +
            "    file       infer a file\n"+
            "    directory  infer files in directory. Requires -r\n"+
            "    -          infer from standard input.\n"+
            "    -r         search files and folders recursively.\n"+
            "    -a         include hidden files.\n" +
            "    -v         verbose mode, use full paths.\n" +
            "    --list     list known mime types. Will ignore files.\n" +
            "    --help     show this help text.";

        private static bool HiddenFiles = false;
        private static bool FullPaths = false;
        private static bool Recursive = false;
        private static bool MinorError = false;
        private static bool MajorError = false;

        [DoesNotReturn]
        private static void Usage()
        {
            Console.WriteLine(USAGE);
            Environment.Exit(0);
        }

        [DoesNotReturn]
        private static void Help()
        {
            Console.WriteLine(HELP);
            Environment.Exit(0);
        }

        [DoesNotReturn]
        private static void ListTypes()
        {
            foreach (MediaType type in MediaTypeResolver.KnownTypes)
            {
                Console.WriteLine("{0}\t{1}", type.FullTypeNoParameters, string.Join(' ', type.Extensions));
            }

            Environment.Exit(0);
        }

        private static string GetPath(FileSystemInfo info)
        {
            return FullPaths ? info.FullName : Path.GetRelativePath(Environment.CurrentDirectory, info.FullName);
        }

        private static void PrintInferenceResult(MediaTypeResult result, string file, MediaType type)
        {
            Console.WriteLine("{0}{1}\t{2}\t{3}",
                result.HasFlag(MediaTypeResult.Extension) ? 'e' : '-',
                result.HasFlag(MediaTypeResult.Content) ? 'c' : '-',
                file,
                type.FullTypeNoParameters);
        }
        private static void PrintInferenceResult(MediaTypeResult result, FileInfo file, MediaType type)
         => PrintInferenceResult(result, GetPath(file), type);


        private static void InferStdin()
        {
            using MemoryStream ms = new MemoryStream(1024);
            using Stream stdin = Console.OpenStandardInput();
            stdin.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            MediaTypeResult result = MediaTypeResolver.TryResolve(ms, out MediaType mediaType) ? MediaTypeResult.Content : 0;
            PrintInferenceResult(result, "<stdin>", mediaType);
        }

        private static void InferFile(FileInfo file)
        {
            if (file.Attributes.HasFlag(FileAttributes.Hidden) && !HiddenFiles)
                return;

            MediaTypeResult result = MediaTypeResolver.TryResolve(file, out MediaType type);
            PrintInferenceResult(result, file, type);
        }

        private static void InferDirectory(DirectoryInfo directory)
        {
            if (directory.Attributes.HasFlag(FileAttributes.Hidden) && !HiddenFiles)
            {
                return;
            }

            foreach (var node in directory.GetFileSystemInfos())
            {
                if (node.Attributes.HasFlag(FileAttributes.Directory))
                {
                    if (Recursive)
                    {
                        InferDirectory((DirectoryInfo)node);
                    }
                    else
                    {
                        Console.WriteLine("# Skipping directory {0}, set -r to traverse.", GetPath(node));
                        MinorError = true;
                    }
                }
                else
                {
                    InferFile((FileInfo)node);
                }
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Usage();
            }

            List<FileSystemInfo> nodes = new List<FileSystemInfo>();
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                case "--help":
                case "/?":
                    Help();
                    return;
                case "--list":
                    ListTypes();
                    return;
                case "-a":
                    HiddenFiles = true;
                    break;
                case "-v":
                    FullPaths = true;
                    break;
                case "-r":
                    Recursive = true;
                    break;
                case "-":
                    InferStdin();
                    break;
                default:
                    if (Directory.Exists(args[i]))
                    {
                        nodes.Add(new DirectoryInfo(args[i]));
                    }
                    else if (File.Exists(args[i]))
                    {
                        nodes.Add(new FileInfo(args[i]));
                    }
                    else
                    {
                        Console.WriteLine("# Path {0} does not exist. Skipping...", args[i]);
                        MajorError = true;                        
                    }
                    break;
                }
            }

            foreach (var node in nodes)
            {
                try
                {
                    if (node.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        InferDirectory((DirectoryInfo)node);
                    }
                    else
                    {
                        InferFile((FileInfo)node);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("# Error while processing {0}: {1}", node.FullName, ex.Message);
                    MajorError = true;
                }
            }

            Environment.Exit(MajorError ? 1 : (MinorError ? 2 : 0));
        }
    }
}
///Vedant Sharma
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;


namespace DiskUtility
{
    public class DiskUtility
    {
        public static int _numFiles;
        public static int _numFolders;
        public static long _totalSize;

        private static String usage = "Usage: du [-s] [-p] [-b] <path> \n" +
                                      "Summarize disk usage of the set of FILES, recursively for directories.\n" +
                                      "\nYou MUST specify one of the parameters, -s, -p, or -b\n" +
                                      "-s  \tRun in single threaded mode\n" +
                                      "-p  \tRun in parallel mode (uses all available processors)\n" +
                                      "-b  \tRun in both parallel and single threaded mode.\n" +
                                      "    \tRuns parallel followed by sequential mode";

        /// <summary>
        /// Main Function
        /// </summary>
        /// <param name="args"></param>
        ///
        /// static void Main(string[] args)
        static void Main(string[] args)
        {
            bool single = false, multi = false;
            if (args.Length == 0)
            {
                Console.WriteLine(usage);
                return;
            }

            switch (args[0])
            {
                case "-s":
                    single = true;
                    break;
                case "-p":
                    multi = true;
                    break;
                case "-b":
                    single = multi = true;
                    break;
                default:
                    Console.WriteLine(usage);
                    return;
            }

            String path = "";
            for (int i = 1; i < args.Length; i++)
            {
                path += args[i] + " ";
            }

            path = path.Trim();

            if (!Directory.Exists(@path) && !File.Exists(@path))
            {
                Console.WriteLine("Please enter a valid path");
                return;
            }

            if (multi && single)
            {
                Console.WriteLine("Directory '" + path + "' :\n");
            }

            if (multi)
            {
                var mu = new MultiThreaded();
                mu.Start(path);
            }

            if (single)
            {
                SingleThreaded.Start(path);
            }
        }
        public static void writeOutput(String threadingType, int numFiles, int numFolders, long totalSize, Stopwatch sp)
        {
            sp.Stop();
            Console.WriteLine(threadingType +" Calculated in: " + ((double)sp.ElapsedMilliseconds/1000).ToString() + "s");
            Console.WriteLine($"{numFolders:N0}"+" folders, "+$"{numFiles:N0}"+" files, "+$"{totalSize:N0}"+" bytes");
        }
    }
    
    public class SingleThreaded
        {
            public static void Start(String path)
            {
                var sp = Stopwatch.StartNew();
                DiskUtility._numFiles = DiskUtility._numFolders = 0;
                DiskUtility._totalSize = 0;
                DirectoryTraverser(path);
                //if (DiskUtility._numFolders > 0)
                  //  --DiskUtility._numFolders;
                DiskUtility.writeOutput("Sequential", DiskUtility._numFiles, DiskUtility._numFolders, DiskUtility._totalSize, sp);
            }

        
            public static void DirectoryTraverser(String path)
            {
                if(File.Exists(path))
                {
                    FileInfo fileInfo;
                    try
                    {
                        fileInfo = new FileInfo(path);
                    }
                    catch (Exception  e)
                    {
                        return;
                    }
                    DiskUtility._totalSize += fileInfo.Length;
                    DiskUtility._numFiles++;
                    return;
                }

                DiskUtility._numFolders++;
                String[] subdirectories = {""}, files = {""};
                try
                {
                    subdirectories = Directory.GetFileSystemEntries(path);
                }
                catch (Exception  e)
                {
                    return;
                }

                foreach(var di in subdirectories)
                {
                    DirectoryTraverser(di);
                }
                return;
            }
        }
    
    public class MultiThreaded
    {
        public void Start(String path)
        { 
            
            var sp = Stopwatch.StartNew();
/*
            if (Directory.GetDirectories(path).Length <= 10)
            {
                SingleThreaded.DirectoryTraverser(path);
                DiskUtility.writeOutput("Parallel", DiskUtility._numFiles, DiskUtility._numFolders, DiskUtility._totalSize, sp);
                return;
            }
            */
            DiskUtility._numFiles = DiskUtility._numFolders = 0;
            DiskUtility._totalSize = 0;
            //var directoryTraverser = Task.Run(() => DirectoryTraverser(path));
            //directoryTraverser.Wait();
            //DirectoryTraverser(path);
            Parallel.ForEach(Directory.GetDirectories(path), diPath => DirectoryTraverser2(diPath));
            sp.Stop();
            DiskUtility.writeOutput("Parallel", DiskUtility._numFiles, DiskUtility._numFolders, DiskUtility._totalSize, sp);

        }
        public static void DirectoryTraverser2(String path)
        {
            if(File.Exists(path))
            {
                //foreach (var file in Directory.GetFiles(Directory.GetParent(path).FullName))
                {
                    FileInfo fileInfo;
                    try
                    {
                        fileInfo = new FileInfo(path);
                    }
                    catch (Exception  e)
                    {
                        return;
                    }
                    Interlocked.Add(ref DiskUtility._totalSize, fileInfo.Length);
                    Interlocked.Increment(ref DiskUtility._numFiles);
                    return;
                }
            }

            Interlocked.Increment(ref DiskUtility._numFolders);
            String[] subdirectories = { "" };
            try
            {
                subdirectories = Directory.GetFileSystemEntries(path);
            }
            catch (Exception  e)
            {
                return;
            }
            //Interlocked.Add(ref DiskUtility._numFolders, Directory.GetDirectories(path).Length);
            Parallel.ForEach(subdirectories, di => DirectoryTraverser2(di));
           /* foreach (var di in subdirectories)
            {
                DirectoryTraverser2(di);
            }*/
        }
        public static void DirectoryTraverser(String path)
        {
            if(File.Exists(path))
            {
                FileInfo fileInfo;
                try
                {
                    fileInfo = new FileInfo(path);
                }
                catch (Exception  e)
                {
                    return;
                }
                DiskUtility._totalSize += fileInfo.Length;
                DiskUtility._numFiles++;
                return;
            }

            DiskUtility._numFolders++;
            String[] subdirectories = {""}, files = {""};
            try
            {
                subdirectories = Directory.GetFileSystemEntries(path);
            }
            catch (Exception  e)
            {
                return;
            }

            foreach(var di in subdirectories)
            {
                DirectoryTraverser(di);
            }
            return;
        }
    }

}
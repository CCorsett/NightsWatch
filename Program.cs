using System;
using System.IO;
using System.Collections;
using System.Timers;

namespace NightsWacht
{
    class Program
    {
        private static System.Timers.Timer snap_timer;
        public static bool changed = false;

        static string watch_dir_path = string.Empty;
        static string dest_path = string.Empty;

        static void Main(string[] args)
        {
            string m1 = "Night's Watch: simple tool for backup snapshot's folder";
            string mm1 = "Give me path's folder for my watch (absolute path please):";
            string m2 = "Great. Where should i save the snapshots? (absolute path please):";

            Console.WriteLine(m1);
            Console.WriteLine(mm1);

            bool ok = false;
            do
            {
                watch_dir_path = Console.ReadLine();
                if (Directory.Exists(watch_dir_path))
                {
                    ok = true;
                }
                else
                {
                    Console.WriteLine("It's not a directory. Give me a valid path:");
                }
            } while (ok == false);

            Console.WriteLine(m2);

            ok = false;

            do
            {
                dest_path = Console.ReadLine();
                if (Directory.Exists(dest_path))
                {
                    ok = true;
                }
                else
                {
                    Console.WriteLine("It's not a directory. Give me a valid path:");
                }
            } while (ok == false);

            Console.WriteLine("Time (in minute) for the snap?:");

            ok = false;
            int time;
            do
            {
                ok = int.TryParse(Console.ReadLine(), out time);
                if (ok == false)
                {
                    Console.WriteLine("Give me an integer value:");
                }
            } while (ok == false);

            SetTimer(time * 60000);
            Console.WriteLine("Perfect. Start my job :)");
            Console.WriteLine("From now, every {0} minutes, if you change something in the folder i create the snapshot of it !!", time);

            // Create a new FileSystemWatcher and set its properties.
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = watch_dir_path;//args[1];

                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                //watcher.NotifyFilter = NotifyFilters.LastAccess
                watcher.NotifyFilter = NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName;

                watcher.IncludeSubdirectories = true;
                
                // Only watch text files.
                watcher.Filter = "*.*";

                // Add event handlers.
                watcher.Changed += OnChanged;
                watcher.Created += OnChanged;
                watcher.Deleted += OnChanged;
                watcher.Renamed += OnChanged;

                // Begin watching.
                watcher.EnableRaisingEvents = true;

                // Wait for the user to quit the program.
                Console.WriteLine("Press 'q' for Exit.");
                while (Console.Read() != 'q') ;
            }

            snap_timer.Stop();
            snap_timer.Dispose();
        }

        // Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine($"{DateTime.Now}: {e.FullPath} {e.ChangeType}");
            WriteLogFile($"{DateTime.Now}: {e.FullPath} {e.ChangeType}");
            changed = true;
        }

        private static void SetTimer(int t)
        {
            // Create a timer with a two second interval.
            snap_timer = new System.Timers.Timer(t);
            // Hook up the Elapsed event for the timer. 
            snap_timer.Elapsed += NewSnapshot;
            snap_timer.AutoReset = true;
            snap_timer.Enabled = true;
        }

        private static void NewSnapshot(Object source, ElapsedEventArgs e)
        {
            if (changed == true)
            {
                changed = false;
                string path = watch_dir_path;
                string result = new DirectoryInfo(path).Name;
                string dir_name = string.Format("snap-{0}-{1:MM-dd-yyy-HH-mm}", result, DateTime.Now);
                string[] paths = { dest_path, dir_name };
                //string[] paths = { dest_path, "pippo"};
                string save_path = Path.Combine(paths);
                Console.WriteLine(save_path);

                DirectoryCopy(watch_dir_path, save_path, true);
                Console.WriteLine($"{DateTime.Now} create snapshot");
                WriteLogFile($"{DateTime.Now} create snapshot");
            }
        }

        private static void WriteLogFile(string message)
        {
            string fname = new DirectoryInfo(watch_dir_path).Name;
            fname += "-log.txt";
            string[] paths = { dest_path, fname };
            string full_path = Path.Combine(paths);
            //Console.WriteLine(full_path);
            if (File.Exists(full_path))
            {
                using (StreamWriter sw = File.AppendText(full_path))
                {
                    sw.WriteLine(message);
                }
            }
            else
            {
                using (StreamWriter sw = File.CreateText(full_path))
                {
                    sw.WriteLine(message);
                }
            }
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Console.WriteLine("Create {0}", destDirName);
                Directory.CreateDirectory(destDirName);
            }
            Console.WriteLine("Write the files");
            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                //Console.WriteLine("Copy {0} to {1}", file.FullName, destDirName);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}

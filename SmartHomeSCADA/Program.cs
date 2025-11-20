using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeSCADA.SecurityModule;

namespace SmartHomeSCADA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // === Create Modules ===
            Doorbell doorbell = new Doorbell();
            DoorLock doorLock = new DoorLock();

            // === Background monitoring loop ===
            CancellationTokenSource cts = new CancellationTokenSource();
            Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    doorbell.Poll();
                    doorLock.Poll();
                    Thread.Sleep(1000); // ~0.7 seconds
                }
            });

            // === Console Menu ===
            Console.WriteLine("====== SECURITY MODULE CONSOLE TEST ======\n");

            Console.WriteLine("DOORBELL:");
            Console.WriteLine("  1 - Simulate Doorbell Ring");
            Console.WriteLine("  2 - Acknowledge (ACK)");
            Console.WriteLine("  3 - Mute (MUTE)\n");

            Console.WriteLine("DOOR LOCK:");
            Console.WriteLine("  4 - LOCK Door");
            Console.WriteLine("  5 - UNLOCK Door");
            Console.WriteLine("  6 - TOGGLE Lock\n");

            Console.WriteLine("FORCED ENTRY:");
            Console.WriteLine("  7 - Simulate Forced Entry");
            Console.WriteLine("  8 - Clear Forced Entry Alert\n");

            Console.WriteLine("SYSTEM:");
            Console.WriteLine("  Q - Quit");
            Console.WriteLine("------------------------------------------\n");

            bool running = true;
            while (running)
            {
                Console.Write("Choice: ");
                var key = Console.ReadKey(intercept: true);
                Console.WriteLine();

                switch (key.Key)
                {
                    // === DOORBELL ===
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        File.WriteAllText("doorbell_status.txt", "RING");
                        Console.WriteLine("Doorbell set to RING.");
                        break;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        doorbell.Acknowledge();
                        Console.WriteLine("ACK sent.");
                        break;

                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        doorbell.Mute();
                        Console.WriteLine("MUTE sent.");
                        break;

                    // === LOCK CONTROL ===
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        doorLock.Lock();
                        Console.WriteLine("LOCK command sent.");
                        break;

                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        doorLock.Unlock();
                        Console.WriteLine("UNLOCK command sent.");
                        break;

                    case ConsoleKey.D6:
                    case ConsoleKey.NumPad6:
                        doorLock.Toggle();
                        Console.WriteLine("TOGGLE command sent.");
                        break;

                    // === FORCED ENTRY ===
                    case ConsoleKey.D7:
                    case ConsoleKey.NumPad7:
                        doorLock.ReportForcedEntry();
                        Console.WriteLine("FORCED ENTRY simulated!");
                        break;

                    case ConsoleKey.D8:
                    case ConsoleKey.NumPad8:
                        doorLock.ClearAlert();
                        Console.WriteLine("Forced Entry alert cleared to Door LOCKED.");
                        break;

                    // === QUIT ===
                    case ConsoleKey.Q:
                        running = false;
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Use valid keys shown above.");
                        break;
                }
            }

            cts.Cancel();
            Console.WriteLine("Exiting... press any key.");
            Console.ReadKey();
        }
    }
}

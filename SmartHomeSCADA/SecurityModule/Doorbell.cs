using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SmartHomeSCADA.SecurityModule
{
    /// <summary>
    /// Doorbell module for the Security system.
    /// Uses text files to simulate device communication:
    ///   doorbell_status.txt  - RING / NORMAL / MISSED / MUTED
    ///   doorbell_cmd.txt     - ACK / MUTE / (blank)
    ///   doorbell_log.txt     - event log
    /// </summary>
    public class Doorbell
    {
        // Files (stored in the app's bin/Debug folder)
        private readonly string statusFile;
        private readonly string commandFile;
        private readonly string logFile;

        // Timing
        private const int RingDurationSeconds = 10;   // max ring time
        private const int MutedHoldSeconds = 3;       // how long MUTED stays before NORMAL

        // Internal state
        public string Status { get; private set; } = "NORMAL";

        private bool ringActive = false;
        private DateTime ringStartUtc;

        private bool mutedActive = false;
        private DateTime mutedStartUtc;

        private DateTime lastConsoleTick = DateTime.MinValue;

        // ----------------- constructor -----------------
        public Doorbell()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            statusFile = Path.Combine(basePath, "doorbell_status.txt");
            commandFile = Path.Combine(basePath, "doorbell_cmd.txt");
            logFile = Path.Combine(basePath, "doorbell_log.txt");

            // make sure files exist with starting values
            if (!File.Exists(statusFile))
            {
                File.WriteAllText(statusFile, "NORMAL");
            }

            if (!File.Exists(commandFile))
            {
                File.WriteAllText(commandFile, "");
            }
        }

        // ----------------- public API -----------------

        // will be called by the dashboard "Acknowledge" button
        public void Acknowledge()
        {
            WriteCommand("ACK");
        }

        // will be called by the dashboard "Mute" button
        public void Mute()
        {
            WriteCommand("MUTE");
        }

        /// <summary>
        /// Main logic. Call this every 700–1000 ms from a Timer
        /// OR from StartMonitoringLoop() for console testing.
        /// </summary>
        public void Poll()
        {
            try
            {
                // 1. Read current doorbell status from file
                ReadStatus();

                // 2. If someone has pressed the bell
                if (Status == "RING")
                {
                    // First time we see RING → start 10 second window
                    if (!ringActive)
                    {
                        ringActive = true;
                        ringStartUtc = DateTime.UtcNow;
                        Log("Ring detected. Starting 10-second window.");
                        // simple sound simulation
                        Console.Beep();
                    }

                    // 3. Check command file for ACK or MUTE
                    string cmd = ReadCommand();
                    if (cmd == "ACK")
                    {
                        HandleAcknowledged();
                        return;
                    }

                    if (cmd == "MUTE")
                    {
                        HandleMuted();
                        return;
                    }

                    // 4. No command yet – check timeout (10 seconds)
                    double elapsed = (DateTime.UtcNow - ringStartUtc).TotalSeconds;
                    if (elapsed >= RingDurationSeconds)
                    {
                        AutoStopAsMissed();
                        return;
                    }

                    // For debugging: show ringing once per second
                    WriteConsoleOncePerSecond("[Doorbell] Ringing... waiting for ACK or MUTE.");
                }
                else
                {
                    // Not RING state

                    // If we are MUTED, keep muted for a small time then NORMAL
                    if (Status == "MUTED" && mutedActive)
                    {
                        double mutedElapsed = (DateTime.UtcNow - mutedStartUtc).TotalSeconds;
                        if (mutedElapsed >= MutedHoldSeconds)
                        {
                            WriteStatus("NORMAL");
                            Status = "NORMAL";
                            mutedActive = false;
                            Log("Mute finished. Reset to NORMAL.");
                        }
                    }
                    else
                    {
                        // ensure ring flag off when not ringing
                        ringActive = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("ERROR in Poll(): " + ex.Message);
            }
        }

        /// <summary>
        /// Console-only helper. In WinForms, use a Timer instead.
        /// </summary>
        public void StartMonitoringLoop()
        {
            Log("Doorbell monitoring loop started.");
            while (true)
            {
                Poll();
                Thread.Sleep(700);
            }
        }

        // ----------------- internal handlers -----------------

        private void HandleAcknowledged()
        {
            // user pressed ACK → stop ring & reset to NORMAL
            WriteCommand("");
            WriteStatus("NORMAL");
            Status = "NORMAL";
            ringActive = false;
            mutedActive = false;

            Log("Doorbell ACK: ring stopped, status NORMAL.");
        }

        private void HandleMuted()
        {
            // user pressed MUTE → stop sound, keep visual alert briefly
            WriteCommand("");
            Status = "MUTED";
            mutedActive = true;
            mutedStartUtc = DateTime.UtcNow;
            ringActive = false;

            Log("Doorbell MUTE: sound off, visual alert MUTED.");
            WriteStatus("MUTED");
        }

        private void AutoStopAsMissed()
        {
            // nobody responded within 10 seconds
            Status = "MISSED";
            WriteStatus("MISSED");
            ringActive = false;
            mutedActive = false;

            Log("No ACK/MUTE within 10s → MISSED.");

            // after logging MISSED, reset to NORMAL for next ring
            WriteStatus("NORMAL");
            Status = "NORMAL";
        }

        // ----------------- file helpers -----------------

        private void ReadStatus()
        {
            if (!File.Exists(statusFile))
            {
                Status = "NORMAL";
                return;
            }

            string text = File.ReadAllText(statusFile).Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                Status = "NORMAL";
            }
            else
            {
                Status = text.ToUpperInvariant();
            }
        }

        private void WriteStatus(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                value = "NORMAL";
            }

            File.WriteAllText(statusFile, value);
        }

        private string ReadCommand()
        {
            if (!File.Exists(commandFile))
            {
                return "";
            }

            string text = File.ReadAllText(commandFile).Trim();
            return text.ToUpperInvariant();
        }

        private void WriteCommand(string value)
        {
            if (value == null)
            {
                value = "";
            }

            File.WriteAllText(commandFile, value);
        }

        private void Log(string message)
        {
            string line = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " +
                          message + Environment.NewLine;
            File.AppendAllText(logFile, line);
            Console.WriteLine(line.TrimEnd());
        }

        private void WriteConsoleOncePerSecond(string msg)
        {
            if ((DateTime.UtcNow - lastConsoleTick).TotalSeconds >= 1)
            {
                lastConsoleTick = DateTime.UtcNow;
                Console.WriteLine(msg);
            }
        }
    }
}


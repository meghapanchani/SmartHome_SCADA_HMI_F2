using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SmartHomeSCADA.SecurityModule
{
   
    public class DoorLock
    {
        // --- File paths (relative to app working directory) ---
        private readonly string statusFile = "lock_status.txt";
        private readonly string commandFile = "lock_cmd.txt";
        private readonly string logFile = "lock_log.txt";
        private readonly string alertFile = "lock_alert.txt";

        // --- Current lock state ---
        // Possible values: LOCKED, UNLOCKED, ALERT
        public string Status { get; private set; } = "LOCKED";

        // Optional: expose alert status for UI/tests
        public string AlertStatus { get; private set; } = "NONE";

        // --------- constructor ----------
        public DoorLock()
        {
            // Ensure status file exists
            if (!File.Exists(statusFile))
            {
                File.WriteAllText(statusFile, "LOCKED");
                Status = "LOCKED";
            }
            else
            {
                string text = File.ReadAllText(statusFile).Trim();
                if (string.IsNullOrWhiteSpace(text))
                    Status = "LOCKED";
                else
                    Status = text.ToUpperInvariant();
            }

            // Ensure command file exists
            if (!File.Exists(commandFile))
            {
                File.WriteAllText(commandFile, "");
            }

            // Ensure alert file exists
            if (!File.Exists(alertFile))
            {
                File.WriteAllText(alertFile, "NONE");
                AlertStatus = "NONE";
            }
            else
            {
                string a = File.ReadAllText(alertFile).Trim();
                if (string.IsNullOrWhiteSpace(a))
                    AlertStatus = "NONE";
                else
                    AlertStatus = a.ToUpperInvariant();
            }
        }

        // ------------- Public API (for UI / console / tests) -------------

        // Called when user clicks "Lock" button
        public void Lock()
        {
            WriteCommand("LOCK");
        }

        // Called when user clicks "Unlock" button
        public void Unlock()
        {
            WriteCommand("UNLOCK");
        }

        // Called when user clicks "Toggle" button (optional)
        public void Toggle()
        {
            WriteCommand("TOGGLE");
        }

        /// <summary>
        /// Forced entry detection.
        /// Call this if a sensor or logic detects someone is trying
        /// to open the door forcefully (for example while LOCKED).
        /// </summary>
        public void ReportForcedEntry()
        {
            Status = "ALERT";
            AlertStatus = "FORCED_ENTRY";

            WriteStatus(Status);
            WriteAlert(AlertStatus);

            Log("FORCED ENTRY detected! Door is in ALERT state.");
        }

        /// <summary>
        /// Clears the forced entry alert after the user acknowledges it.
        /// Typically you will call this from the UI when the user presses
        /// an 'Acknowledge Alert' button.
        /// </summary>
        public void ClearAlert()
        {
            AlertStatus = "NONE";
            WriteAlert(AlertStatus);

            // After alert, we keep the door locked for safety
            Status = "LOCKED";
            WriteStatus(Status);

            Log("Forced entry alert cleared. Door set to LOCKED.");
        }

        /// <summary>
        /// Main logic. Call this from a Timer (UI) or a loop (console).
        /// It reads lock_cmd.txt, applies the command, updates lock_status.txt,
        /// and then clears the command.
        /// </summary>
        public void Poll()
        {
            try
            {
                // Sync internal status with file
                ReadStatus();
                ReadAlertStatus();

                // If we are in ALERT state, ignore normal commands until user clears it
                if (Status == "ALERT")
                {
                    // Do not process LOCK/UNLOCK/TOGGLE while in ALERT
                    return;
                }

                // read current command (LOCK / UNLOCK / TOGGLE / blank)
                string cmd = ReadCommand();
                if (string.IsNullOrEmpty(cmd))
                {
                    // nothing to do
                    return;
                }

                if (cmd == "LOCK")
                {
                    if (Status != "LOCKED")
                    {
                        Status = "LOCKED";
                        WriteStatus(Status);
                        Log("Door locked by command.");
                    }
                }
                else if (cmd == "UNLOCK")
                {
                    if (Status != "UNLOCKED")
                    {
                        Status = "UNLOCKED";
                        WriteStatus(Status);
                        Log("Door unlocked by command.");
                    }
                }
                else if (cmd == "TOGGLE")
                {
                    if (Status == "LOCKED")
                    {
                        Status = "UNLOCKED";
                        WriteStatus(Status);
                        Log("Door toggled to UNLOCKED.");
                    }
                    else
                    {
                        Status = "LOCKED";
                        WriteStatus(Status);
                        Log("Door toggled to LOCKED.");
                    }
                }

                // clear the command file after handling
                WriteCommand("");
            }
            catch (Exception ex)
            {
                Log("ERROR in Lock Poll(): " + ex.Message);
            }
        }

        // ------------- File helpers -------------

        private void ReadStatus()
        {
            if (!File.Exists(statusFile))
            {
                Status = "LOCKED";
                return;
            }

            string text = File.ReadAllText(statusFile).Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                Status = "LOCKED";
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
                value = "LOCKED";
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

        private void ReadAlertStatus()
        {
            if (!File.Exists(alertFile))
            {
                AlertStatus = "NONE";
                return;
            }

            string text = File.ReadAllText(alertFile).Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                AlertStatus = "NONE";
            }
            else
            {
                AlertStatus = text.ToUpperInvariant();
            }
        }

        private void WriteAlert(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                value = "NONE";
            }

            File.WriteAllText(alertFile, value);
        }

        private void Log(string message)
        {
            string line = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] "
                          + message + Environment.NewLine;
            File.AppendAllText(logFile, line);
            Console.WriteLine(line.TrimEnd());
        }
    }
}

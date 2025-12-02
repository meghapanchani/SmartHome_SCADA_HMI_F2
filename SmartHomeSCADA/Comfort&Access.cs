using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHomeSCADA
{
    internal class main
    {
    }

    //Thermostat
    public class Thermostat
    {
        private readonly string statusFile = "thermo_status.txt";
        private readonly string commandFile = "thermo_cmd.txt";

        public double Temperature { get; private set; }
        public double Humidity { get; private set; }
        public double SetPoint { get; private set; }
        public string Mode { get; private set; }  // "HEAT", "COOL", "OFF"

        public void ReadStatus()
        {
            if (!File.Exists(statusFile)) return;

            foreach (var line in File.ReadAllLines(statusFile))
            {
                var parts = line.Split('=');
                if (parts.Length != 2) continue;

                switch (parts[0])
                {
                    case "temp": Temperature = double.Parse(parts[1]); break;
                    case "hum": Humidity = double.Parse(parts[1]); break;
                    case "setpoint": SetPoint = double.Parse(parts[1]); break;
                    case "mode": Mode = parts[1]; break;
                }
            }
        }

        public void ApplyCommand()
        {
            if (!File.Exists(commandFile)) return;

            foreach (var line in File.ReadAllLines(commandFile))
            {
                var parts = line.Split('=');
                if (parts.Length != 2) continue;

                switch (parts[0])
                {
                    case "setpoint": SetPoint = double.Parse(parts[1]); break;
                    case "mode": Mode = parts[1]; break;
                }
            }

            // Simulate writing updated status
            File.WriteAllLines(statusFile, new[]
            {
                $"temp={Temperature}",
                $"hum={Humidity}",
                $"setpoint={SetPoint}",
                $"mode={Mode}"
            });
        }
    }

    //Smart Plug
    public class SmartPlug
    {
        private readonly string statusFile = "plug_status.txt";
        private readonly string commandFile = "plug_cmd.txt";

        public bool IsOn { get; private set; }

        public void ReadStatus()
        {
            if (!File.Exists(statusFile)) return;

            foreach (var line in File.ReadAllLines(statusFile))
            {
                var parts = line.Split('=');
                if (parts[0] == "state")
                    IsOn = parts[1] == "ON";
            }
        }

        public void ApplyCommand()
        {
            if (!File.Exists(commandFile)) return;

            foreach (var line in File.ReadAllLines(commandFile))
            {
                var parts = line.Split('=');
                if (parts[0] == "cmd")
                    IsOn = parts[1] == "ON";
            }

            // Save current state
            File.WriteAllText(statusFile, $"state={(IsOn ? "ON" : "OFF")}");
        }
    }
}
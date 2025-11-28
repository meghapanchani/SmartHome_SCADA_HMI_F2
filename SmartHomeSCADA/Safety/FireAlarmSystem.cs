using System;

namespace SmartHomeSCADA.Safety
{
    /// <summary>
    /// Fire alarm logic for the Smart Home SCADA system.
    /// Rules:
    /// - Alarm turns ON when smoke >= 60.
    /// - Alarm can only turn OFF when reset is requested AND smoke <= 20.
    /// - Acknowledge only has effect when alarm is ON.
    /// - Negative smoke values are treated as 0.
    /// </summary>
    public class FireAlarmSystem
    {
        private const double SmokeAlarmThreshold = 60.0;
        private const double SmokeSafeThreshold = 20.0;

        public bool IsAlarmOn { get; private set; }
        public bool IsAcknowledged { get; private set; }
        public double CurrentSmokeLevel { get; private set; }

        public FireAlarmSystem()
        {
            IsAlarmOn = false;
            IsAcknowledged = false;
            CurrentSmokeLevel = 0.0;
        }

        /// <summary>
        /// Core update method. Call this whenever new sensor data or commands arrive.
        /// </summary>
        /// <param name="smokeLevel">Current smoke level (0+). If negative, treated as 0.</param>
        /// <param name="resetRequested">True if user pressed reset in this cycle.</param>
        /// <param name="ackRequested">True if user pressed acknowledge in this cycle.</param>
        public void Update(double smokeLevel, bool resetRequested, bool ackRequested)
        {
            if (smokeLevel < 0)
            {
                smokeLevel = 0;
            }

            CurrentSmokeLevel = smokeLevel;

            // 1) If alarm is OFF, maybe turn it ON
            if (!IsAlarmOn)
            {
                if (smokeLevel >= SmokeAlarmThreshold)
                {
                    IsAlarmOn = true;
                    IsAcknowledged = false; // new alarm, not acknowledged yet
                }

                // Ack/Reset while alarm is OFF do nothing
                return;
            }

            // 2) Alarm is ON — handle acknowledge
            if (ackRequested)
            {
                IsAcknowledged = true;
            }

            // 3) Handle reset:
            // Only turn alarm OFF if danger is gone (smoke <= safe threshold)
            if (resetRequested && smokeLevel <= SmokeSafeThreshold)
            {
                IsAlarmOn = false;
                IsAcknowledged = false;
            }
        }
    }
}

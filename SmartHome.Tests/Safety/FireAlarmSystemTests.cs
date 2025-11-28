using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartHomeSCADA.Safety;

namespace SmartHome.Tests.Safety
{
    [TestClass]
    public class FireAlarmSystemTests
    {
        [TestMethod]
        public void Alarm_Should_RemainOff_When_Smoke_Below_Threshold()
        {
            var alarm = new FireAlarmSystem();

            alarm.Update(smokeLevel: 30.0, resetRequested: false, ackRequested: false);

            Assert.IsFalse(alarm.IsAlarmOn);
            Assert.IsFalse(alarm.IsAcknowledged);
            Assert.AreEqual(30.0, alarm.CurrentSmokeLevel);
        }

        [TestMethod]
        public void Alarm_Should_TurnOn_When_Smoke_At_Or_Above_Threshold()
        {
            var alarm = new FireAlarmSystem();

            alarm.Update(smokeLevel: 60.0, resetRequested: false, ackRequested: false);

            Assert.IsTrue(alarm.IsAlarmOn);
            Assert.IsFalse(alarm.IsAcknowledged);
            Assert.AreEqual(60.0, alarm.CurrentSmokeLevel);
        }

        [TestMethod]
        public void Acknowledge_Should_Set_Acknowledged_Only_When_Alarm_Is_On()
        {
            var alarm = new FireAlarmSystem();

            // Trigger alarm
            alarm.Update(smokeLevel: 70.0, resetRequested: false, ackRequested: false);
            Assert.IsTrue(alarm.IsAlarmOn);
            Assert.IsFalse(alarm.IsAcknowledged);

            // Acknowledge
            alarm.Update(smokeLevel: 70.0, resetRequested: false, ackRequested: true);

            Assert.IsTrue(alarm.IsAlarmOn);
            Assert.IsTrue(alarm.IsAcknowledged);
        }

        [TestMethod]
        public void Acknowledge_Should_Do_Nothing_When_Alarm_Is_Off()
        {
            var alarm = new FireAlarmSystem();

            alarm.Update(smokeLevel: 10.0, resetRequested: false, ackRequested: true);

            Assert.IsFalse(alarm.IsAlarmOn);
            Assert.IsFalse(alarm.IsAcknowledged);
        }

        [TestMethod]
        public void Reset_Should_Not_Turn_Off_Alarm_If_Smoke_Still_High()
        {
            var alarm = new FireAlarmSystem();

            // Trigger alarm
            alarm.Update(smokeLevel: 80.0, resetRequested: false, ackRequested: false);
            Assert.IsTrue(alarm.IsAlarmOn);

            // Try reset while danger persists
            alarm.Update(smokeLevel: 80.0, resetRequested: true, ackRequested: false);

            Assert.IsTrue(alarm.IsAlarmOn);
        }

        [TestMethod]
        public void Reset_Should_Turn_Off_Alarm_When_Smoke_Is_Safe()
        {
            var alarm = new FireAlarmSystem();

            // Trigger alarm
            alarm.Update(smokeLevel: 80.0, resetRequested: false, ackRequested: false);
            Assert.IsTrue(alarm.IsAlarmOn);

            // Now smoke is safe and reset is requested
            alarm.Update(smokeLevel: 10.0, resetRequested: true, ackRequested: false);

            Assert.IsFalse(alarm.IsAlarmOn);
            Assert.IsFalse(alarm.IsAcknowledged);
            Assert.AreEqual(10.0, alarm.CurrentSmokeLevel);
        }

        [TestMethod]
        public void Negative_Smoke_Values_Should_Be_Treated_As_Zero()
        {
            var alarm = new FireAlarmSystem();

            alarm.Update(smokeLevel: -5.0, resetRequested: false, ackRequested: false);

            Assert.AreEqual(0.0, alarm.CurrentSmokeLevel);
            Assert.IsFalse(alarm.IsAlarmOn);
        }
    }
}

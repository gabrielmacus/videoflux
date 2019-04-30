using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using videoflux.components.DeviceInfo;

namespace videofluxTest
{
    [TestClass]
    public class DeviceInfoTest
    {
        [TestMethod]
        public void TestExtractDeviceNumber()
        {
            var deviceInfo = new Info(@"I:\2019-02-16_4");
            Assert.AreEqual(4,deviceInfo.DeviceNumber);

            deviceInfo = new Info(@"I:\2019-02-16_5");
            Assert.AreEqual(5,deviceInfo.DeviceNumber);

            deviceInfo = new Info(@"I:\2019-02-16_5.2");
            Assert.AreEqual(5,deviceInfo.DeviceNumber );

            deviceInfo = new Info(@"I:\2019-02-16_5.9");
            Assert.AreEqual(5,deviceInfo.DeviceNumber);

            deviceInfo = new Info(@"I:\2019-02-16_5,9");
            Assert.AreEqual(5,deviceInfo.DeviceNumber);

            deviceInfo = new Info(@"I:\2019-02-16_5[DEMO]");
            Assert.AreEqual(5,deviceInfo.DeviceNumber);

            Assert.ThrowsException<WrongFolderException>(()=> {
                deviceInfo = new Info(@"C:\");
            });

            Assert.ThrowsException<WrongFolderException>(() => {
                deviceInfo = new Info(@"I:\2019-02-16_A");
            });

        }
    }
}

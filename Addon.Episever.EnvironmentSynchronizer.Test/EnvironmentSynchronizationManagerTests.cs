using Addon.Episerver.EnvironmentSynchronizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using Addon.Episerver.EnvironmentSynchronizer.DynamicData;

namespace Addon.Episerver.EnvironmentSynchronizer.Test
{
    [TestClass]
    public class EnvironmentSynchronizationManagerTests
    {
        [TestMethod]
        public void Should_Call_Synchronizers_Synchronize_Method()
        {
            var synchronizer1 = new Mock<IEnvironmentSynchronizer>();
            var synchronizer2 = new Mock<IEnvironmentSynchronizer>();

            var store = new Mock<IEnvironmentSynchronizationStore>();

            var synchronizers = new List<IEnvironmentSynchronizer>() { synchronizer1.Object , synchronizer2.Object};

            var _subject = new EnvironmentSynchronizationManager(synchronizers, store.Object);

            var resultLog = _subject.Synchronize();

            synchronizer1.Verify(m => m.Synchronize(It.IsAny<string>()));
            synchronizer2.Verify(m => m.Synchronize(It.IsAny<string>()));
        }

        [TestMethod]
        public void Should_Provide_Current_Environment_Name_To_Synchronizers()
        {
            var synchronizer1 = new Mock<IEnvironmentSynchronizer>();
            var store = new Mock<IEnvironmentSynchronizationStore>();

            var synchronizers = new List<IEnvironmentSynchronizer>() { synchronizer1.Object };

            var _subject = new EnvironmentSynchronizationManager(synchronizers, store.Object);

            string environmentName = "abc";

            var resultLog = _subject.Synchronize(environmentName);

            synchronizer1.Verify(m => m.Synchronize(environmentName));
        }


    }
}

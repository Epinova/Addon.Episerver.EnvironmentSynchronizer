using Xunit;
using Moq;
using System.Collections.Generic;
using Addon.Episerver.EnvironmentSynchronizer.DynamicData;

namespace Addon.Episerver.EnvironmentSynchronizer.Test
{
    public class EnvironmentSynchronizationManagerTests
    {
        [Fact]
        public void Should_Call_Synchronizers_Synchronize_Method()
        {
            //Arrange
            var synchronizer1 = new Mock<IEnvironmentSynchronizer>();
            var synchronizer2 = new Mock<IEnvironmentSynchronizer>();

            var store = new Mock<IEnvironmentSynchronizationStore>();

            var synchronizers = new List<IEnvironmentSynchronizer>() { synchronizer1.Object , synchronizer2.Object};

            var _subject = new EnvironmentSynchronizationManager(synchronizers, store.Object);

            //Act
            var resultLog = _subject.Synchronize();

            //Assert
            synchronizer1.Verify(m => m.Synchronize(It.IsAny<string>()));
            synchronizer2.Verify(m => m.Synchronize(It.IsAny<string>()));
        }

        [Fact]
        public void Should_Provide_Current_Environment_Name_To_Synchronizers()
        {
            //Arrange
            var synchronizer1 = new Mock<IEnvironmentSynchronizer>();
            var store = new Mock<IEnvironmentSynchronizationStore>();

            var synchronizers = new List<IEnvironmentSynchronizer>() { synchronizer1.Object };

            var _subject = new EnvironmentSynchronizationManager(synchronizers, store.Object);

            string environmentName = "abc";

            //Act
            var resultLog = _subject.Synchronize(environmentName);

            //Assert
            synchronizer1.Verify(m => m.Synchronize(environmentName));
        }


    }
}

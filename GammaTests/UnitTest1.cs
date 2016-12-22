using System;
using System.Data.Entity.Core.EntityClient;
using Effort;
using Effort.DataLoaders;
using Gamma.Entities;
using Gamma.Models;
using Gamma.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GammaTests
{
    [TestClass]
    public class UnitTest1
    {
        private GammaEntities Context { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            var loader = new EntityDataLoader("name=GammaEntities");
            var connection = EntityConnectionFactory.CreateTransient("name=GammaEntities",loader);
            Context = new GammaEntities(connection);
        }

        [TestMethod]
        public void TestMethod1()
        {
            var viewModel = new ProductionTaskSGIViewModel(Context);

            Assert.IsNotNull(viewModel.Places);
        }
    }
}

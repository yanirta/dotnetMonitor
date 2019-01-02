using System;
using System.Collections.Generic;
using System.Drawing;
using log4net;
using Monitor;
using Monitor.Interfaces;
using Monitor.Providers;
using Moq;
using Xunit;

namespace monitor.Tests
{
    public class EngineTests
    {
        [Fact]
        public void EngineTestConcurrency()
        {
            var urlprovMock = new Mock<IUrlProvider>();
            var validatorMock = new Mock<IValidator>();

            Stack<string> urls = new Stack<string>(new[] {
                "https://www.zotac.com/",
                "https://www.nvidia.com/"
                });
            urlprovMock.Setup(prov => prov.nextField(0))
            .Returns(() =>
            {
                if (urls.Count == 0)
                    return null;
                else
                    return urls.Pop();
            });

            var engine = new Engine(urlprovMock.Object, 2);
            engine.registerValidator(validatorMock.Object);
            engine.run();
        }

        [Fact]
        public void EngineTestVisualValidation()
        {
            var urlprovMock = new Mock<IUrlProvider>();
            var visualValidator = new ApplitoolsValidator(
                Environment.GetEnvironmentVariable("APPLITOOLS_API_KEY"),
                "Test app",
                "Test batch",
                new Size(1000, 650));

            Stack<string> urls = new Stack<string>(new[] {
                "https://www.zotac.com/",
                "https://www.nvidia.com/"
                });
            urlprovMock.Setup(prov => prov.nextField(0))
            .Returns(() =>
            {
                if (urls.Count == 0)
                    return null;
                else
                    return urls.Pop();
            });

            var engine = new Engine(urlprovMock.Object, 2);
            engine.registerValidator(visualValidator);
            engine.run();
        }

        [Fact]
        public void EngineTestVisualValidationAndExcelProvider()
        {
            var urlprov = new ExcelFileUrlProvider(@"../../../TestData/TestUrlList.xlsx");
            var visualValidator = new ApplitoolsValidator(
                Environment.GetEnvironmentVariable("APPLITOOLS_API_KEY"),
                "Test app",
                "Test batch",
                new Size(1000, 650));

            var engine = new Engine(urlprov, 3);
            engine.registerValidator(visualValidator);
            engine.run();
        }
    }
}

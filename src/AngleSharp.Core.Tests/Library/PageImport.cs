﻿namespace AngleSharp.Core.Tests.Library
{
    using AngleSharp.Core.Tests.Mocks;
    using AngleSharp.Dom.Html;
    using AngleSharp.Extensions;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [TestFixture]
    public class PageImportTests
    {
        [Test]
        public async Task ImportPageFromVirtualRequest()
        {
            var requester = new MockRequester();
            var receivedRequest = new TaskCompletionSource<String>();
            requester.OnRequest = request => receivedRequest.SetResult(request.Address.Href);
            var config = Configuration.Default.WithDefaultLoader(setup => setup.IsResourceLoadingEnabled = true, new[] { requester });

            var document = await BrowsingContext.New(config).OpenAsync(m => m.Content("<!doctype html><link rel=import href=http://example.com/test.html>"));
            var link = document.QuerySelector<IHtmlLinkElement>("link");
            var result = await receivedRequest.Task;

            Assert.AreEqual("import", link.Relation);
            Assert.IsNotNull(link.Import);
            Assert.AreEqual("http://example.com/test.html", result);
        }

        [Test]
        public async Task ImportPageFromDataRequest()
        {
            var receivedRequest = new TaskCompletionSource<Boolean>();
            var config = Configuration.Default.WithDefaultLoader(setup =>
            {
                setup.IsResourceLoadingEnabled = true;
                setup.Filter = request =>
                {
                    receivedRequest.SetResult(true);
                    return true;
                };
            });

            var document = await BrowsingContext.New(config).OpenAsync(m => m.Content("<!doctype html><link rel=import href='data:text/html,<div>foo</div>'>"));
            var link = document.QuerySelector<IHtmlLinkElement>("link");
            var finished = await receivedRequest.Task;

            Assert.AreEqual("import", link.Relation);
            Assert.IsNotNull(link.Import);
            Assert.AreEqual("foo", link.Import.QuerySelector("div").TextContent);
        }

        [Test]
        public async Task ImportPageWithDirectCycle()
        {
            var content = "<!doctype html><link rel=import href=http://example.com/test.html>";
            var requester = new MockRequester();
            var requestCount = 0;
            requester.OnRequest = request => requestCount++;
            requester.BuildResponse = request => content;
            var config = Configuration.Default.WithDefaultLoader(setup => setup.IsResourceLoadingEnabled = true, new[] { requester });
            var document = await BrowsingContext.New(config).OpenAsync(m => m.Content(content));
            var link = document.QuerySelector<IHtmlLinkElement>("link");
            Assert.AreEqual("import", link.Relation);
            Assert.IsNotNull(link.Import);
            Assert.AreEqual(1, requestCount);
        }

        [Test]
        public async Task ImportPageWithIndirectCycle()
        {
            var content = "<!doctype html><link rel=import href=http://example.com/test.html>";
            var nested = new Queue<String>(new [] 
            {
                "<!doctype html><link rel=import href=http://example.com/foo.html>",
                "<!doctype html><link rel=import href=http://example.com/bar.html>",
                "<!doctype html><link rel=import href=http://example.com/test.html>",
                "<!doctype html><link rel=import href=http://example.com/foo.html>"
            });
            var requester = new MockRequester();
            var requestCount = 0;
            requester.OnRequest = request => requestCount++;
            requester.BuildResponse = request => nested.Dequeue();
            var config = Configuration.Default.WithDefaultLoader(setup => setup.IsResourceLoadingEnabled = true, new[] { requester });
            var document = await BrowsingContext.New(config).OpenAsync(m => m.Content(content));
            var link = document.QuerySelector<IHtmlLinkElement>("link");
            Assert.AreEqual("import", link.Relation);
            Assert.IsNotNull(link.Import);
            Assert.AreEqual(3, requestCount);
        }
    }
}

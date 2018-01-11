using Moq;
using NLog.Config;
using NUnit.Framework;
using SharpRaven;
using SharpRaven.Data;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace NLog.Targets.Sentry.UnitTests
{
   [TestFixture]
   class SentryTargetTests
   {
      [SetUp]
      public void Setup()
      {
         LogManager.ThrowExceptions = true;
      }

      [TearDown]
      public void Teardown()
      {
         LogManager.ThrowExceptions = false;
      }

      [Test]
      public void TestPublicConstructor()
      {
         // ReSharper disable ObjectCreationAsStatement
         Assert.DoesNotThrow(() => new SentryTarget());
         // ReSharper restore ObjectCreationAsStatement
         Assert.Throws<NLogConfigurationException>(() =>
         {
            var sentryTarget = new SentryTarget();
            var configuration = new LoggingConfiguration();
            configuration.AddTarget("NLogSentry", sentryTarget);
            configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, sentryTarget));
            LogManager.Configuration = configuration;
         });
      }

      [Test]
      public void TestBadDsn()
      {
         // ReSharper disable ObjectCreationAsStatement
         Assert.Throws<ArgumentException>(() => new SentryTarget(null) { Dsn = "http://localhost" });
         // ReSharper restore ObjectCreationAsStatement
      }

      [Test]
      public void TestLoggingToSentry()
      {
         var sentryClient = new Mock<IRavenClient>();
         ErrorLevel lErrorLevel = ErrorLevel.Debug;
         IDictionary<string, string> lTags = null;
         Exception lException = null;
         string lMessage = null;
         sentryClient
             .Setup(x => x.Capture(It.IsAny<SentryEvent>()))
             .Callback((SentryEvent sentryEvent) =>
             {
                lException = sentryEvent.Exception;
                lErrorLevel = sentryEvent.Level;
                lTags = sentryEvent.Tags;
                lMessage = sentryEvent.Message;
             })
             .Returns("Done");

         // Setup NLog
         var sentryTarget = new SentryTarget(sentryClient.Object)
         {
            Dsn = "http://25e27038b1df4930b93c96c170d95527:d87ac60bb07b4be8908845b23e914dae@test/4",
            Layout = "${message}"
         };
         var configuration = new LoggingConfiguration();
         configuration.AddTarget("NLogSentry", sentryTarget);
         configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, sentryTarget));
         
         LogManager.Configuration = configuration;

         try
         {
            throw new Exception("Oh No!");
         }
         catch (Exception e)
         {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Error(e, "Error Message");
         }

         Assert.IsNotNull(lException);
         Assert.IsTrue(lException.Message == "Oh No!");
         Assert.IsTrue(lMessage == "Error Message");
         Assert.IsNotNull(lTags);
         Assert.IsTrue(0 == lTags.Count);
         Assert.IsTrue(lErrorLevel == ErrorLevel.Error);
      }

      [Test]
      public void TestLoggingToSentry_SendEventWithTags()
      {
         var sentryClient = new Mock<IRavenClient>();
         ErrorLevel lErrorLevel = ErrorLevel.Debug;
         IDictionary<string, string> lTags = null;
         Exception lException = null;
         string lMessage = null;

         sentryClient
             .Setup(x => x.Capture(It.IsAny<SentryEvent>()))
             .Callback((SentryEvent sentryEvent) =>
             {
                lException = sentryEvent.Exception;
                lErrorLevel = sentryEvent.Level;
                lTags = sentryEvent.Tags;
                lMessage = sentryEvent.Message;
             })
             .Returns("Done");

         // Setup NLog
         var sentryTarget = new SentryTarget(sentryClient.Object)
         {
            Dsn = "http://25e27038b1df4930b93c96c170d95527:d87ac60bb07b4be8908845b23e914dae@test/4",
            Layout = "${message}${processid:sentrytag=pid}"
         };
         var configuration = new LoggingConfiguration();
         configuration.AddTarget("NLogSentry", sentryTarget);
         configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, sentryTarget));
         LogManager.Configuration = configuration;

         try
         {
            throw new Exception("Oh No!");
         }
         catch (Exception e)
         {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Error(e, "Error Message");
         }

         Assert.IsNotNull(lException);
         Assert.IsTrue(lException.Message == "Oh No!");
         Assert.IsTrue(lMessage == "Error Message");
         Assert.IsNotNull(lTags);
         Assert.IsTrue(1 == lTags.Count);
         Assert.IsTrue(lTags.ContainsKey("pid"));
         Assert.IsTrue(lErrorLevel == ErrorLevel.Error);
      }
   }
}

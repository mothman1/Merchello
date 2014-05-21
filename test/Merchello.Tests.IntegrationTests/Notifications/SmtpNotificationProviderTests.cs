﻿using System;
using System.Configuration;
using System.Linq;
using Merchello.Core.Gateways.Notification.Smtp;
using Merchello.Core.Models;
using Merchello.Core.Services;
using Merchello.Tests.IntegrationTests.TestHelpers;
using NUnit.Framework;
using Umbraco.Core.Events;

namespace Merchello.Tests.IntegrationTests.Notifications
{
    [TestFixture]
    public class SmtpNotificationProviderTests : DatabaseIntegrationTestBase
    {
        private SmtpNotificationGatewayProvider _provider;

        private readonly Guid _key = new Guid("5F2E88D1-6D07-4809-B9AB-D4D6036473E9");

        [TestFixtureSetUp]
        public override void FixtureSetup()
        {
            base.FixtureSetup();

            _provider = MerchelloContext.Gateways.Notification.GetProviderByKey(_key, false) as SmtpNotificationGatewayProvider;

            Assert.NotNull(_provider, "Provider was not resolved");

            GatewayProviderService.Saving += GatewayProviderServiceOnSaved;
        }

        private void GatewayProviderServiceOnSaved(IGatewayProviderService sender, SaveEventArgs<IGatewayProviderSettings> args)
        {
            var key = new Guid("5F2E88D1-6D07-4809-B9AB-D4D6036473E9");
            var provider = args.SavedEntities.FirstOrDefault(x => key == x.Key && !x.HasIdentity);
            if (provider == null) return;

            provider.ExtendedData.SaveSmtpProviderSettings(new SmtpNotificationGatewayProviderSettings());
        }

        [SetUp]
        public void Init()
        {
            if (!_provider.Activated)
            {
                MerchelloContext.Gateways.Notification.ActivateProvider(_provider);
            }

            PreTestDataWorker.DeleteAllNotificationMethods();

        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            GatewayProviderService.Saved -= GatewayProviderServiceOnSaved;
        }

        /// <summary>
        /// Test confirms that the provider can be deactivated
        /// </summary>
        [Test]
        public void Can_DeActivate_The_SmtpNotificationGatewayProvider()
        {
            //// Arrange
            // handled by setup

            //// Act
            MerchelloContext.Gateways.Notification.DeactivateProvider(_provider);
            _provider = MerchelloContext.Gateways.Notification.GetProviderByKey(_key, false) as SmtpNotificationGatewayProvider;

            //// Assert
            Assert.NotNull(_provider);
            Assert.IsFalse(_provider.Activated);            
        }

        /// <summary>
        /// Test confirms that SMTP Provider Settings can be retrieved from the extended data collection
        /// </summary>
        [Test]
        public void Can_Retrieve_SmtpProviderSettings_From_ExtendedData()
        {
            //// Arrange
            // handled in Setup

            //// Act
            var settings = _provider.ExtendedData.GetSmtpProviderSettings();

            //// Assert
            Assert.NotNull(settings);
            Assert.AreEqual("127.0.0.1", settings.Host);
        }

        [Test]
        public void Can_Create_A_SmtpNotificationGatewayMethod()
        {
            //// Arrange
            var resource = _provider.ListResourcesOffered().FirstOrDefault();
            Assert.NotNull(resource, "Smtp Provider returned null for GatewayResource");

            //// Act
            var method = _provider.CreateNotificationMethod(resource, resource.Name, "SMTP Relayed Email");

            //// Assert
            Assert.NotNull(method);
            Assert.IsTrue(method.NotificationMethod.HasIdentity);
        }

        /// <summary>
        /// Test verifies that a host value can be saved to Extended Data
        /// </summary>
        [Test]
        public void Can_Save_SmtpProviderSettings_ToExtendedData()
        {
            //// Arrange
            var host = "moria";
            var key = _provider.Key;

            //// Act
            var settings = _provider.ExtendedData.GetSmtpProviderSettings();
            settings.Host = host;
            _provider.ExtendedData.SaveSmtpProviderSettings(settings);

            PreTestDataWorker.GatewayProviderService.Save(_provider.GatewayProviderSettings);

            var smtpProviderSettings = PreTestDataWorker.GatewayProviderService.GetGatewayProviderByKey(key);

            //// Assert
            Assert.AreEqual(host, smtpProviderSettings.ExtendedData.GetSmtpProviderSettings().Host);

        }

        /// <summary>
        /// Test verifies that an email can be sent using the SMTP provider
        /// </summary>
        [Test]
        public void Can_Send_A_Test_Email()
        {
            // check configuration to see if we want to do this
            if (!bool.Parse(ConfigurationManager.AppSettings["sendTestEmail"])) Assert.Ignore("Skipping test");

            //// Arrange
            var settings = _provider.ExtendedData.GetSmtpProviderSettings();
            settings.Host = "moria";
            _provider.ExtendedData.SaveSmtpProviderSettings(settings);

            var resource = _provider.ListResourcesOffered().FirstOrDefault();
            Assert.NotNull(resource, "Smtp Provider returned null for GatewayResource");

            var method = _provider.CreateNotificationMethod(resource, resource.Name, "Test email method");

            //// Act
            var message = new NotificationMessage(method.NotificationMethod.Key, "Test email", "Can_Send_A_Test_Email@merchello.com")
            {
                Recipients = "rusty@mindfly.com",
                BodyText = "Successful test?"
            };

            method.Send(message);

        }
    }
}
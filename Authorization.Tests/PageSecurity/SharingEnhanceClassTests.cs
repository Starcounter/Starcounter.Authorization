using System;
using System.Diagnostics;
using FluentAssertions;
using NUnit.Framework;
using Starcounter.Authorization.PageSecurity;
using Starcounter.Authorization.Tests.PageSecurity.Fixtures;
using Starcounter.Authorization.Tests.PageSecurity.Fixtures.Sharing;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    public class SharingEnhanceClassTests: EnhanceClassTestsBase
    {
        [Test]
        public void FirstSecuredThenUnsecured()
        {
            CreatePage<SecurePageOne>();
            SetupPolicyToReturn(Roles.ThingViewer, false);
            var unsecuredPage = new UnsecuredPage();
            var column = unsecuredPage.Common.Columns.Add();

            ChangePropertyInPage(column, p => p.Template.Filter);

            VerifyChangedAndCheckDeniedHandlerNotCalled(column.Filter);
        }

        [Test]
        public void FirstSecuredThenOtherSecured()
        {
            SetupPolicyToReturn(Roles.ThingViewer, false);
            var securePageOne = CreatePage<SecurePageOne>();
            CreatePage<SecurePageTwo>();
            var column = securePageOne.Common.Columns.Add();

            ChangePropertyInPage(column, p => p.Template.Filter);

            column.Filter.Should().Be(0);
        }

        [Test]
        public void FirstSecuredThenOtherSecured_SeparatePageSecurity()
        {
            var checkersCache = new CheckersCache();
            CreatePageSecurity(checkersCache).EnhanceClass(typeof(SecurePageOne));
            SetupPolicyToReturn(Roles.ThingViewer, true);
            CreatePageSecurity(checkersCache).EnhanceClass(typeof(SecurePageTwo));
            var securePageOne = new SecurePageOne();
            var column = securePageOne.Common.Columns.Add();

            ChangePropertyInPage(column, p => p.Template.Filter);

            column.Filter.Should().Be(1);
        }

        [Test]
        public void FirstSecuredThenOtherSecured_DifferentPermissions()
        {
            SetupPolicyToReturn(Roles.ThingViewer, true);
            SetupPolicyToReturn(Roles.ThingEditor, false);
            var securePageOne = CreatePage<SecurePageOne>();
            CreatePage<SecurePageTwo>();
            var column = securePageOne.Common.Columns.Add();

            ChangePropertyInPage(column, p => p.Template.Filter);

            column.Filter.Should().Be(1);
        }

        [Test]
        public void FirstSecuredThenOtherSecured_ColumnsFilter_DifferentPermissions()
        {
            SetupPolicyToReturn(Roles.ThingViewer, true);
            SetupPolicyToReturn(Roles.ThingEditor, false);
            var securePageOne = CreatePage<SecurePageOne>();
            CreatePage<SecurePageTwo>();
            var common = securePageOne.Common;

            ChangePropertyInPage(common, p => p.Template.ColumnsFilter);

            common.ColumnsFilter.Should().Be(1);
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using Quantumart.QP8.Configuration.Authentication.WindowsAuthentication;

namespace Quantumart.QP8.Configuration.Authentication
{
    public class AuthenticationElement : ConfigurationElement
    {
        [ConfigurationProperty("allowSaveUserInformationInCookie", DefaultValue = true, IsRequired = true)]
        public bool AllowSaveUserInformationInCookie
        {
            get { return (bool)base["allowSaveUserInformationInCookie"]; }
            set { base["allowSaveUserInformationInCookie"] = value; }
        }

        [ConfigurationProperty("windowsAuthentication", IsRequired = true)]
        public WindowsAuthenticationElement WindowsAuthentication
        {
            get { return (WindowsAuthenticationElement)base["windowsAuthentication"]; }
        }
    }
}

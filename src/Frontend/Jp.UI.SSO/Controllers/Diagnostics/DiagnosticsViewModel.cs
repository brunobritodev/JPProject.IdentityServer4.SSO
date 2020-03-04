// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Jp.UI.SSO.Controllers.Diagnostics
{

    public class DiagnosticsViewModel
    {
        public DiagnosticsViewModel(HttpContext httpContext, AuthenticateResult result, IConfiguration configuration)
        {
            AuthenticateResult = result;

            if (result.Properties.Items.ContainsKey("client_list"))
            {
                var encoded = result.Properties.Items["client_list"];
                var bytes = Base64Url.Decode(encoded);
                var value = Encoding.UTF8.GetString(bytes);

                Clients = JsonConvert.DeserializeObject<string[]>(value);
            }

            UserManagementUrl = configuration["ApplicationSettings:UserManagementURL"];
            AdminPanelUrl = configuration["ApplicationSettings:IS4AdminUi"];
            ApiUrl = configuration["ApplicationSettings:ResourceServerURL"];

        }


        public string ApiUrl { get; set; }

        public string AdminPanelUrl { get; set; }

        public string UserManagementUrl { get; set; }

        public AuthenticateResult AuthenticateResult { get; }
        public IEnumerable<string> Clients { get; } = new List<string>();
    }
}
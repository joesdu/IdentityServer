﻿using IdentityServer;
using IdentityServer.Application;
using IdentityServer.Configuration;
using IdentityServer.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using static IdentityServer.Constants;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Core
    {
        #region required
        /// <summary>
        /// 必要的平台服务
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddRequiredPlatformServices(this IIdentityServerBuilder builder)
        {
            builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddOptions();
            builder.Services.AddSingleton(
                resolver => resolver.GetRequiredService<IOptions<IdentityServerOptions>>().Value);
            builder.Services.AddHttpClient();
            return builder;
        }
        #endregion

        #region core
        internal static IIdentityServerBuilder AddCoreServices(this IIdentityServerBuilder builder)
        {
            return builder;
        }
        #endregion       

        #region endpoints

        public static IIdentityServerBuilder AddEndpoint<T>(this IIdentityServerBuilder builder, string name, PathString path)
          where T : class, IEndpointHandler
        {
            builder.Services.AddTransient<T>();
            builder.Services.AddSingleton(new IdentityServer.Hosting.Endpoint(name, path, typeof(T)));
            return builder;
        }

        internal static IIdentityServerBuilder AddDefaultEndpoints(this IIdentityServerBuilder builder)
        {
            builder.Services.AddTransient<IEndpointRouter, EndpointRouter>();

            builder.AddEndpoint<DiscoveryKeyEndpoint>(EndpointNames.Discovery, ProtocolRoutePaths.DiscoveryWebKeys.EnsureLeadingSlash());
            builder.AddEndpoint<DiscoveryEndpoint>(EndpointNames.Discovery, ProtocolRoutePaths.DiscoveryConfiguration.EnsureLeadingSlash());
            builder.AddEndpoint<TokenEndpoint>(EndpointNames.Token, ProtocolRoutePaths.Token.EnsureLeadingSlash());

            return builder;
        }
        #endregion

        #region pluggable
        /// <summary>
        /// 可插拔的服务
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        internal static IIdentityServerBuilder AddPluggableServices(this IIdentityServerBuilder builder)
        {
            builder.Services.TryAddTransient<ITokenService, DefaultTokenService>();
            builder.Services.TryAddTransient<ITokenCreationService, DefaultTokenCreationService>();
            builder.Services.TryAddTransient<IServerUrls, ServerUrls>();
            return builder;
        }
        #endregion

        #region responseGenerators
        internal static IIdentityServerBuilder AddResponseGenerators(this IIdentityServerBuilder builder)
        {
            builder.Services.TryAddTransient<IDiscoveryResponseGenerator, DefaultDiscoveryResponseGenerator>();
            builder.Services.TryAddTransient<IDiscoveryKeyResponseGenerator, DefaultDiscoveryKeyResponseGenerator>();
            return builder;
        }
        #endregion

        #region cookieAuthentication
        public static IIdentityServerBuilder AddCookieAuthentication(this IIdentityServerBuilder builder)
        {
            builder.Services.AddAuthentication(IdentityServerConstants.DefaultCookieAuthenticationScheme)
                .AddCookie(IdentityServerConstants.DefaultCookieAuthenticationScheme)
                .AddCookie(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            return builder;
        }

        #endregion
    }
}

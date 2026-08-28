using Admin.Desktop.Tools;
using Admin.Desktop.Tools.Messages;
using CommunityToolkit.Mvvm.Messaging;
using Duende.IdentityModel.Client;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client.Authentication;

namespace Admin.Desktop
{
    [Dependency(ReplaceServices = true)]
    [ExposeServices(typeof(IRemoteServiceHttpClientAuthenticator))]
    public class AccessTokenRemoteServiceHttpClientAuthenticator : IRemoteServiceHttpClientAuthenticator, ITransientDependency
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AccessTokenRemoteServiceHttpClientAuthenticator> _logger;

        public AccessTokenRemoteServiceHttpClientAuthenticator(IHttpClientFactory httpClientFactory, ILogger<AccessTokenRemoteServiceHttpClientAuthenticator> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task Authenticate(RemoteServiceHttpClientAuthenticateContext context)
        {
            // 获取token
            var currentAccessToken = TokenManager.AccessToken;
            // 判断Token是否过期
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(currentAccessToken);
            // 直接使用ValidTo属性，它代表Token的过期时间
            // 注意：ValidTo是DateTime类型，但通常是UTC时间
            if (DateTime.UtcNow > jwtToken.ValidTo)
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var identityServerUrl = ConfigurationManager.AppSettings["RemoteServices"];
                var disco = await httpClient.GetDiscoveryDocumentAsync(identityServerUrl);
                if (disco.IsError)
                {
                    _logger.LogError("Failed to get discovery document: {Error}", disco.Error);
                    return;
                }

                var refreshTokenRequest = new RefreshTokenRequest
                {
                    Address = disco.TokenEndpoint, // 令牌端点地址不变
                    ClientId = ConfigurationManager.AppSettings["ClientId"] ?? string.Empty,
                    // 如果你的客户端配置了密钥，这里也需要提供
                    ClientSecret = null,
                    // 使用之前保存的刷新令牌
                    RefreshToken = TokenManager.RefreshToken,
                };

                // 发送刷新请求
                var refreshResponse = await httpClient.RequestRefreshTokenAsync(refreshTokenRequest);
                if (refreshResponse.IsError)
                {
                    // 刷新失败！
                    // 可能的原因：刷新令牌过期、被撤销、或客户端密钥错误等。
                    // 此时需要让用户重新登录。
                    _logger.LogError("刷新令牌失败: {Error}", refreshResponse.Error);
                    // 导向用户重新登录的页面...
                    WeakReferenceMessenger.Default.Send<LogoutMessage>();
                    return;
                }
                TokenManager.SetTokens(refreshResponse);
                currentAccessToken = TokenManager.AccessToken;
            }

            context.Request.SetBearerToken(currentAccessToken);
        }
    }
}

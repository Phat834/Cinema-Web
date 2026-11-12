using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CinemaManagement.Services
{
    public class MoMoPaymentService : IMoMoPaymentService
    {
        private readonly string _partnerCode;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _endpoint;
        private readonly string _returnUrl;
        private readonly string _notifyUrl;
        private readonly bool _useMockPayment;
        private readonly IHttpClientFactory _httpClientFactory;

        public MoMoPaymentService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _partnerCode = configuration["Momo:PartnerCode"];
            _accessKey = configuration["Momo:AccessKey"];
            _secretKey = configuration["Momo:SecretKey"];
            _endpoint = configuration["Momo:Endpoint"];
            _returnUrl = configuration["Momo:ReturnUrl"];
            _notifyUrl = configuration["Momo:NotifyUrl"];
            _useMockPayment = bool.TryParse(configuration["Momo:UseMockPayment"], out var mock) && mock;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> CreatePaymentRequestAsync(long amount, string orderId)
        {
            if (_useMockPayment)
            {
                return await Task.FromResult($"/Booking/MockMomoCheckout?orderId={orderId}&amount={amount}");
            }

            string requestId = Guid.NewGuid().ToString();
            string orderInfo = "Thanh toán vé xem phim";
            string extraData = "";
            string requestType = "captureWallet";

            string rawHash = $"accessKey={_accessKey}&amount={amount}&extraData={extraData}&ipnUrl={_notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={_partnerCode}&redirectUrl={_returnUrl}&requestId={requestId}&requestType={requestType}";

            string signature;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey)))
                signature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawHash))).ToLower();

            var body = new
            {
                partnerCode = _partnerCode,
                partnerName = "Cinema",
                storeId = "CinemaStore",
                requestId,
                amount = amount.ToString(),
                orderId,
                orderInfo,
                redirectUrl = _returnUrl,
                ipnUrl = _notifyUrl,
                extraData,
                requestType,
                signature,
                lang = "vi"
            };

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_endpoint, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode || responseBody.TrimStart().StartsWith("<"))
                throw new Exception($"Cổng MoMo Sandbox đang bảo trì hoặc từ chối kết nối (HTTP {(int)response.StatusCode}).");

            var result = JsonConvert.DeserializeObject<MomoResponse>(responseBody);

            if (result == null || result.ResultCode != 0)
                throw new Exception($"MoMo lỗi: {result?.Message} (Code {result?.ResultCode})");

            return result.PayUrl;
        }

        public MomoCallbackResult ProcessNotify(IDictionary<string, string> data)
        {
            string orderId = data.TryGetValue("orderId", out var oid) ? oid : null;
            bool isSuccess = data.TryGetValue("resultCode", out var rc) && rc == "0";
            return new(orderId, isSuccess);
        }

        public class MomoResponse
        {
            [JsonProperty("payUrl")]
            public string PayUrl { get; set; }
            [JsonProperty("resultCode")]
            public int ResultCode { get; set; }
            [JsonProperty("message")]
            public string Message { get; set; }
        }
    }
}
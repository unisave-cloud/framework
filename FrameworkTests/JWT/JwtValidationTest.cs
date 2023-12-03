using System;
using System.Security.Cryptography;
using FrameworkTests.Testing;
using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using LightJson;
using LightJson.Serialization;
using NUnit.Framework;
using Unisave.JWT;

namespace FrameworkTests.JWT
{
    [TestFixture]
    public class JwtValidationTest
    {
        // old json web token
        private readonly string token = "eyJ0IjoiaWRfdG9rZW4iLCJhbGciOiJSUzI1NiIsImtpZCI6IldNUzdFbmtJR3BjSDlER1pzdjJXY1k5eHN1Rm5aQ3R4WmpqNEFoYi1fOEUifQ.eyJzdWIiOiJhYmI2OTAxOGFjYmY0NzcyYjVlMTk0ZTg1Nzk0YTBjYyIsInBmc2lkIjoiMGViNTRjYjJhNGZjNGIzMGFmZmM4OGYyZDU0OTEzM2IiLCJpc3MiOiJodHRwczpcL1wvYXBpLmVwaWNnYW1lcy5kZXZcL2VwaWNcL29hdXRoXC92MSIsImN0eSI6IkNaIiwiZG4iOiJKaXJrYS1NYXllciIsIm5vbmNlIjoibi1GT0wxckQrU21mU0hGMmZhK3F2Wk0wRExRYTg9IiwicGZwaWQiOiI3NGJlYTE2MzVmZjE0OGFlOTY2NzlhNzFjOTdkOTVkMyIsImF1ZCI6Inh5emE3ODkxc0NCUVZxTk1XZFE2MzEyUEdQM2x6ak9rIiwicGZkaWQiOiI3OGJjZGQ3NmJjYTg0YzgzYTA5NjFhYmM2MzRkNzEwNSIsInQiOiJpZF90b2tlbiIsImFwcGlkIjoiZmdoaTQ1Njd1TVAyb1hJRG5raGNVZDROZEh3Q0RUcngiLCJleHAiOjE2OTIyMjgxMTMsImlhdCI6MTY5MjIyMDkxMywianRpIjoiNDk4N2I4YTYyOTlmNGZmMDg4MmZmY2NkZDZhZDRkN2IifQ.SEaQcGFYBmwGecQXiJd17y0oDjMXdpacjSb2mC7GbA6_Otd-XOYPvaJ6wFLFghjSZv2HoT1FgFgBQ0JwAyGrKBEqBIb1v43fJlZzdcedLVhfAHgQ2C3wuveyBJIJcDgDOcxnHwoXxW1K0cStEkTDgPjmuHIW9Ua0w7RFTuYqAqMD_E9pm8lBRXc85DaTTxiR5KPQSBOlJ7X3-gzj5k1oSaLkmZbynpz_mVYxeuKIXNPzArZm9NXC0HL_FMxMQuDJiEoyTex3rJrKrQDUkh2C9802qa_qBdBCVxLE_UvnvWULivAhvD4j0ZbyFXdKMNy4O2vsHo7VKVVX4o4tHKqMIg";
        
        // downloaded public key from Epic Games API
        private readonly string modulus = "l6XI48ujknQQlsJgpGXg4l2i_DuUxuG2GXTzkOG7UtX4MqkVBCfW1t1JIIc8q0kCInC2oBwhC599ZCmd-cOi0kS7Aquv68fjERIRK9oCUnF_lJg296jV8xcalFY0FOWX--qX3xGKL33VjJBMIrIu7ETjj06s-v4li22CnHmu2lDkrp_FPTVzFscn-XRIojqIFb7pKRFPt27m12FNE_Rd9bqlVCkvMNuE7VTpTOrSfKk5B01M5IuXKXk0pTAWnelqaD9bHjAExe2I_183lp_uFhNN4hLTjOojxl-dK8Jy2OCPEAsg5rs9Lwttp3zZ--y0sM7UttN2dE0w3F2f352MNQ";
        private readonly string exponent = "AQAB";

        private readonly JsonObject expectedHeader = new JsonObject {
            ["t"] = "id_token",
            ["alg"] = "RS256",
            ["kid"] = "WMS7EnkIGpcH9DGZsv2WcY9xsuFnZCtxZjj4Ahb-_8E"
        };
        
        private readonly JsonObject expectedBody = new JsonObject {
            ["sub"] = "abb69018acbf4772b5e194e85794a0cc",
            ["pfsid"] = "0eb54cb2a4fc4b30affc88f2d549133b",
            ["iss"] = "https://api.epicgames.dev/epic/oauth/v1",
            ["cty"] = "CZ",
            ["dn"] = "Jirka-Mayer",
            ["nonce"] = "n-FOL1rD+SmfSHF2fa+qvZM0DLQa8=",
            ["pfpid"] = "74bea1635ff148ae96679a71c97d95d3",
            ["aud"] = "xyza7891sCBQVqNMWdQ6312PGP3lzjOk",
            ["pfdid"] = "78bcdd76bca84c83a0961abc634d7105",
            ["t"] = "id_token",
            ["appid"] = "fghi4567uMP2oXIDnkhcUd4NdHwCDTrx",
            ["exp"] = 1692228113,
            ["iat"] = 1692220913,
            ["jti"] = "4987b8a6299f4ff0882ffccdd6ad4d7b"
        };
        
        [Test]
        public void ItValidatesTokens()
        {
            IJsonSerializer serializer = new LightJsonSerializer();
            IDateTimeProvider provider = new UtcDateTimeProvider();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            
            var parameters = new RSAParameters() {
                Modulus = urlEncoder.Decode(modulus),
                Exponent = urlEncoder.Decode(exponent)
            };
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(parameters);
            IJwtAlgorithm algorithm = new RS256Algorithm(rsa);
            
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

            Assert.Throws<TokenExpiredException>(() => {
                decoder.Decode(token);
            });

            // it decodes body well
            string bodyString = decoder.Decode(token, verify: false);
            JsonObject body = JsonReader.Parse(bodyString).AsJsonObject;
            JsonAssert.AreEqual(expectedBody, body);
            
            // it decodes header well
            string headerString = decoder.DecodeHeader(token);
            JsonObject header = JsonReader.Parse(headerString).AsJsonObject;
            JsonAssert.AreEqual(expectedHeader, header);
        }
    }
}
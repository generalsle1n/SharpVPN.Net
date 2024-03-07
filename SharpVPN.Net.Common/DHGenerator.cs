using System.Numerics;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using BigInteger = Org.BouncyCastle.Math.BigInteger;

namespace SharpVPN.Net.Common;

public class DHGenerator
{
    public int KeySize { get; set; } = 256;
    private const string _keyCipher = "ECDH";
    private const int _primeProbabilty = 100;
    public KeyValuePair<AsymmetricCipherKeyPair, DHParameters> CreateKeyPairAlice()
    {
        IAsymmetricCipherKeyPairGenerator KeyGenerator = GeneratorUtilities.GetKeyPairGenerator(_keyCipher);
        DHParametersGenerator DHGenerator = new DHParametersGenerator();
        DHGenerator.Init(KeySize, _primeProbabilty, new SecureRandom());
        DHParameters DHParameter = DHGenerator.GenerateParameters();

        KeyGenerationParameters KeyGenParameter = new DHKeyGenerationParameters(new SecureRandom(), DHParameter);
        KeyGenerator.Init(KeyGenParameter);
        return new KeyValuePair<AsymmetricCipherKeyPair, DHParameters>(KeyGenerator.GenerateKeyPair(), DHParameter);
    }

    public KeyValuePair<AsymmetricCipherKeyPair, DHParameters> CreateKeyPairBob(BigInteger AliceP, BigInteger AliceG)
    {
        IAsymmetricCipherKeyPairGenerator KeyGenerator = GeneratorUtilities.GetKeyPairGenerator(_keyCipher);
        DHParameters DHParameter = new DHParameters(AliceP, AliceG);

        KeyGenerationParameters KeyGenParameter = new DHKeyGenerationParameters(new SecureRandom(), DHParameter);
        KeyGenerator.Init(KeyGenParameter);

        return new KeyValuePair<AsymmetricCipherKeyPair, DHParameters>(KeyGenerator.GenerateKeyPair(), DHParameter);
    }

    public IBasicAgreement SetupKeyAgree(AsymmetricCipherKeyPair Key)
    {
        IBasicAgreement KeyAgree = AgreementUtilities.GetBasicAgreement(_keyCipher);
        KeyAgree.Init(Key.Private);
        return KeyAgree;
    }
}

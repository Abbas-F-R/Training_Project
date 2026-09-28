using Sqids;

namespace OC_System_Training.Shared.Helper;

// تعليق تدريبي: أداة تشفير وفك تشفير المعرفات (Sqids)
// تحوّل أرقام الـ ID (BIGINT) إلى نصوص عشوائية آمنة مثل "b9X7mK2p" والعكس
public static class SqidCodec
{
    private static readonly SqidsEncoder<long> Encoder = new(new SqidsOptions { MinLength = 8 });

    /// <summary>
    /// تشفير المعرف الرقمي إلى نص Sqid
    /// </summary>
    public static string Encode(long value) => Encoder.Encode(value);

    /// <summary>
    /// فك تشفير نص Sqid أو قراءة الرقم العادي إذا أرسله العميل
    /// </summary>
    public static long? TryDecode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Trim();

        if (long.TryParse(value, out var plain)) return plain;

        try
        {
            var decoded = Encoder.Decode(value);
            return decoded.Count > 0 ? decoded[0] : null;
        }
        catch
        {
            return null;
        }
    }
}

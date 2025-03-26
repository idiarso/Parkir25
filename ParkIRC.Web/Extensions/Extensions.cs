using System.Globalization;

namespace ParkIRC.Web.Extensions
{
    public static class Extensions
    {
        public static string ToRupiah(this decimal value)
        {
            return string.Format(new CultureInfo("id-ID"), "Rp {0:N0}", value);
        }

        public static string ToRupiah(this decimal? value)
        {
            return value.HasValue ? value.Value.ToRupiah() : "-";
        }
    }
}

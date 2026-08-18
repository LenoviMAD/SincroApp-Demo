using EntidadesMultieempresas;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
namespace EntidadesMultieempresas
{
    public static class PedidosCsvWriter
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static string ToCsv(List<PedidosJsonMultiempresaExcelItem> rows, char sep = ';')
        {
            var sb = new StringBuilder();

            // Header exacto como pediste
            sb.AppendLine(string.Join(sep,
                "VendedorID",
                "ClienteID",
                //"EmpresaID",
                "ClienteCodigo",
                "CodigoProducto",
                "ProductoID",
                "Unidades",
                "ListaDePrecioID",
                "PrecioUnitarioFinal",
                "PrecioUnitarioNeto",
                "TotalProducto",
                "TotalPedido"
            ));

            foreach (var r in rows)
            {
                sb.Append(r.VendedorID).Append(sep);
                sb.Append(r.ClienteID).Append(sep);
                //sb.Append(r.EmpresaID).Append(sep);
                sb.Append(Escape(r.ClienteCodigo, sep)).Append(sep);

                sb.Append(Escape(r.CodigoProducto, sep)).Append(sep);
                sb.Append(r.ProductoID).Append(sep);

                sb.Append(r.Unidades.ToString(Inv)).Append(sep);
                sb.Append(r.ListaDePrecioID).Append(sep);

                sb.Append(r.PrecioUnitarioFinal.ToString(Inv)).Append(sep);
                sb.Append(r.PrecioUnitarioNeto.ToString(Inv)).Append(sep);

                sb.Append(r.TotalProducto.ToString(Inv)).Append(sep);
                sb.Append(r.TotalPedido.ToString(Inv)).Append(sep);

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string Escape(string? value, char sep)
        {
            value ??= "";
            var mustQuote = value.Contains(sep) || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            if (!mustQuote) return value;

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }


    }
}

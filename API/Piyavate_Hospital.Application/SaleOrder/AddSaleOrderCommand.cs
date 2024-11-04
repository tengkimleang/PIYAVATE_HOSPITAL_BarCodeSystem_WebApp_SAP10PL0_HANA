using ErrorOr;
using MediatR;
using Piyavate_Hospital.Domain.Common;

namespace Piyavate_Hospital.Application.SaleOrder
{
    public class AddSaleOrderCommand : IRequest<ErrorOr<PostResponse>>
    {
        public string CardCode { get; set; } = null!;
        public int ContactPersonCode { get; set; }
        public string NumAtCard { get; set; } = "";
        public int Series { get; set; }
        public DateTime DocDate { get; set; } = DateTime.Today;
        public DateTime TaxDate { get; set; } = DateTime.Today;
        public string Remarks { get; set; } = "";
        public int BranchID { get; set; }
        public int DocEntry { get; set; }
        public List<SaleOrderLine>? Lines { get; set; }
    }
    public class SaleOrderLine
    {
        public string ItemCode { get; set; } = null!;
        public double Qty { get; set; }
        public double Price { get; set; }
        public string VatCode { get; set; } = "";
        public string WarehouseCode { get; set; } = "";

    }
}

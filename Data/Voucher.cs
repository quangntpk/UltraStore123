using System;
using System.Collections.Generic;

namespace UltraStrore.Data
{
    public partial class Voucher
    {
        public int? MaVoucher { get; set; } = null!;
        public string? TenVoucher { get; set; }
        public int? GiaTri { get; set; }
        public string? MoTa { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public byte[]? HinhAnh { get; set; }
        public decimal? DieuKien { get; set; }
        public int? SoLuong { get; set; }

        public int? TrangThai { get; set; }

        public virtual ICollection<Coupon> Coupons { get; set; }
    }
}

using UltraStrore.Helper;
using UltraStrore.Models.CreateModels;
using UltraStrore.Models.EditModels;
using UltraStrore.Models.ViewModels;

namespace UltraStrore.Repository
{
    public interface IVoucherServices
    {
        List<VoucherView> GetAllVouchers();

        Task<VoucherView> CreateVoucher(VoucherCreate voucher); 

        Task<VoucherView> EditVoucher(VoucherEdit voucher);

        Task<bool> DeleteVoucher(int maBinhLuan);
    }
}
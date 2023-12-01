using Microsoft.Extensions.Logging;
using WCM.EntityFrameworkCore.EntityFrameworkCore.DrWin;

namespace WebApi.DrWin.Reposites
{
    public interface IPhieuNhapXuatReposites
    {

    }
    public class PhieuNhapXuatReposites: IPhieuNhapXuatReposites
    {
        private readonly ILogger<PhieuNhapXuatReposites> _logger;
        private readonly DrWinDbContext _dbContext;
        public PhieuNhapXuatReposites(
            DrWinDbContext dbContext,
            ILogger<PhieuNhapXuatReposites> logger
        )
        {
            _logger = logger;
            _dbContext = dbContext;
        }

    }
}

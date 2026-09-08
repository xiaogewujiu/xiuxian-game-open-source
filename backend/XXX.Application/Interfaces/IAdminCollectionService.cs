using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    public interface IAdminCollectionService
    {
        // 文字图鉴系列
        Task<List<AdminTextCollectionSeriesListItemDto>> GetTextSeriesListAsync(string? keyword);
        Task<AdminTextCollectionSeriesDetailDto?> GetTextSeriesDetailAsync(string seriesId);
        Task<AdminTextCollectionSeriesDetailDto> SaveTextSeriesAsync(AdminTextCollectionSeriesDetailDto dto);
        Task<bool> DeleteTextSeriesAsync(string seriesId);

        // 文字图鉴项
        Task<List<AdminTextCollectionItemListItemDto>> GetTextItemListAsync(string? seriesId);
        Task<AdminTextCollectionItemDetailDto?> GetTextItemDetailAsync(string itemId);
        Task<AdminTextCollectionItemDetailDto> SaveTextItemAsync(AdminTextCollectionItemDetailDto dto);
        Task<bool> DeleteTextItemAsync(string itemId);

        // 文字图鉴属性加成
        Task<List<AdminTextCollectionBonusListItemDto>> GetTextBonusListAsync(string? seriesId);
        Task<AdminTextCollectionBonusDetailDto?> GetTextBonusDetailAsync(string bonusId);
        Task<AdminTextCollectionBonusDetailDto> SaveTextBonusAsync(AdminTextCollectionBonusDetailDto dto);
        Task<bool> DeleteTextBonusAsync(string bonusId);

        // 图片图鉴系列
        Task<List<AdminImageCollectionSeriesListItemDto>> GetImageSeriesListAsync(string? keyword);
        Task<AdminImageCollectionSeriesDetailDto?> GetImageSeriesDetailAsync(string seriesId);
        Task<AdminImageCollectionSeriesDetailDto> SaveImageSeriesAsync(AdminImageCollectionSeriesDetailDto dto);
        Task<bool> DeleteImageSeriesAsync(string seriesId);

        // 图片图鉴项
        Task<List<AdminImageCollectionItemListItemDto>> GetImageItemListAsync(string? seriesId);
        Task<AdminImageCollectionItemDetailDto?> GetImageItemDetailAsync(string itemId);
        Task<AdminImageCollectionItemDetailDto> SaveImageItemAsync(AdminImageCollectionItemDetailDto dto);
        Task<bool> DeleteImageItemAsync(string itemId);

        // 图片图鉴属性加成
        Task<List<AdminImageCollectionBonusListItemDto>> GetImageBonusListAsync(string? seriesId);
        Task<AdminImageCollectionBonusDetailDto?> GetImageBonusDetailAsync(string bonusId);
        Task<AdminImageCollectionBonusDetailDto> SaveImageBonusAsync(AdminImageCollectionBonusDetailDto dto);
        Task<bool> DeleteImageBonusAsync(string bonusId);
    }
}

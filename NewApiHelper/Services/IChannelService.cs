using NewApiHelper.Models;

namespace NewApiHelper.Services;

public interface IChannelService
{
    /// <summary>
    /// 获取渠道列表（分页）
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>包含渠道列表的API响应</returns>
    Task<ApiResponse<ChannelListResponseData>> GetChannelsAsync(int page = 1, int pageSize = 20);

    /// <summary>
    /// 根据ID获取单个渠道的详细信息
    /// </summary>
    /// <param name="id">渠道ID</param>
    /// <returns>包含单个渠道信息的API响应</returns>
    Task<ApiResponse<Channel>> GetChannelByIdAsync(int id);

    /// <summary>
    /// 测试指定渠道的连通性
    /// </summary>
    /// <param name="id">渠道ID</param>
    /// <param name="model">可选的测试模型</param>
    /// <returns>测试结果</returns>
    Task<TestChannelResponse> TestChannelAsync(int id, string? model = null);

    /// <summary>
    /// 新增一个渠道
    /// </summary>
    /// <param name="newChannel">要新增的渠道信息</param>
    /// <returns>表示操作结果的API响应</returns>
    Task<ApiResponse<object>> AddChannelAsync(AddChannelRequest newChannel);

    /// <summary>
    /// 更新一个现有渠道的信息
    /// </summary>
    /// <param name="channelToUpdate">要更新的渠道信息</param>
    /// <returns>表示操作结果的API响应</returns>
    Task<ApiResponse<object>> UpdateChannelAsync(UpdateChannelRequest channelToUpdate);

    /// <summary>
    /// 删除一个渠道
    /// </summary>
    /// <param name="id">要删除的渠道ID</param>
    /// <returns>表示操作结果的API响应</returns>
    Task<ApiResponse<object>> DeleteChannelAsync(int id);

    /// <summary>
    /// 批量删除渠道
    /// </summary>
    /// <param name="ids">要删除的渠道ID列表</param>
    /// <returns>表示操作结果的API响应，data为成功删除的数量</returns>
    Task<ApiResponse<int>> DeleteChannelsAsync(IEnumerable<int> ids);

    /// <summary>
    /// 根据输入的ModelSync列表，生成AddChannelRequest集合
    /// </summary>
    /// <param name="modelSyncs"></param>
    /// <returns></returns>
    IEnumerable<AddChannelRequest> GenerateChannels(IEnumerable<ModelSync> modelSyncs);
}
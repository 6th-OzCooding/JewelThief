using System.Collections.Generic;

namespace TeamConvention.Interfaces
{
    /// <summary>
    /// 인벤토리 안에서 사용할 수 있는 Tool의 공통 사용 규칙입니다.
    /// </summary>
    public interface IUseTool
    {
        /// <summary>
        /// Tool 아이템 데이터 Id입니다.
        /// </summary>
        string ToolId { get; }

        /// <summary>
        /// 현재 남아 있는 사용 가능 횟수입니다.
        /// </summary>
        int RemainingUseCount { get; }

        /// <summary>
        /// 대상이 요구하는 Tool 목록에 이 Tool이 포함되는지 확인합니다.
        /// </summary>
        bool CanUseTool(IReadOnlyList<string> requiredToolIds);

        /// <summary>
        /// 대상이 요구하는 Tool 목록에 맞으면 Tool을 1회 사용합니다.
        /// </summary>
        bool UseTool(IReadOnlyList<string> requiredToolIds);
    }
}

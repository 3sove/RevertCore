using Revert.Core.Common.Error_Handling;

namespace Revert.Core.Common
{
	public class ErrorLogResponse
	{
	    public string Message { get; set; }
	    public ErrorLogResponseTypes ErrorLogResponseType { get; set; }

		internal ErrorLogResponse(ErrorLogResponseTypes errorLogResponseType, string message)
        {
			Message = message;
			ErrorLogResponseType = errorLogResponseType;
		}
	}
}
